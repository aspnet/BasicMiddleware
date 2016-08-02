// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.AspNetCore.Rewrite.RuleAbstraction;
using Microsoft.AspNetCore.Rewrite.UrlRewrite.UrlActions;
using Microsoft.AspNetCore.Rewrite.UrlRewrite.UrlMatches;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public static class UrlRewriteFileParser
    {
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(1);

        public static List<UrlRewriteRule> Parse(TextReader reader)
        {
            var temp = XDocument.Load(reader);
            var xmlRoot = temp.Descendants(RewriteTags.Rewrite);
            var rules = new List<UrlRewriteRule>();

            if (xmlRoot != null)
            {
                // there is a valid rewrite block, go through each rule and process
                GetGlobalRules(xmlRoot.Descendants(RewriteTags.GlobalRules), rules);
                GetRules(xmlRoot.Descendants(RewriteTags.Rules), rules);
            }
            return rules;
        }

        private static void GetGlobalRules(IEnumerable<XElement> globalRules, List<UrlRewriteRule> result)
        {
            foreach (var rule in globalRules.Elements(RewriteTags.GlobalRules) ?? Enumerable.Empty<XElement>())
            {
                var res = new UrlRewriteRule();
                SetRuleAttributes(rule, res);
                // TODO handle full url with global rules - may or may not support
                CreateUrlAction(rule.Element(RewriteTags.Action), true, res);
                if (res.Enabled)
                {
                    result.Add(res);
                }
            }
        }

        private static void GetRules(IEnumerable<XElement> rules, List<UrlRewriteRule> result)
        {
            // TODO Better null check?
            foreach (var rule in rules.Elements(RewriteTags.Rule) ?? Enumerable.Empty<XElement>())
            {
                var res = new UrlRewriteRule();
                SetRuleAttributes(rule, res);
                CreateUrlAction(rule.Element(RewriteTags.Action), false, res);
                if (res.Enabled)
                {
                    result.Add(res);
                }
            }
        }

        private static void SetRuleAttributes(XElement rule, UrlRewriteRule res)
        {
            if (rule == null)
            {
                return;
            }

            res.Name =  rule.Attribute(RewriteTags.Name)?.Value;

            bool enabled;
            if (bool.TryParse(rule.Attribute(RewriteTags.Enabled)?.Value, out enabled))
            {
                res.Enabled = enabled;
            }

            PatternSyntax patternSyntax;
            if (Enum.TryParse(rule.Attribute(RewriteTags.PatternSyntax)?.Value, out patternSyntax))
            {
                res.PatternSyntax = patternSyntax;
            }

            bool stopProcessing;
            if (bool.TryParse(rule.Attribute(RewriteTags.StopProcessing)?.Value, out stopProcessing))
            {
                res.StopProcessing = stopProcessing;
            }

            CreateMatch(rule.Element(RewriteTags.Match), res);
            CreateConditions(rule.Element(RewriteTags.Conditions), res);
        }

        private static void CreateMatch(XElement match, UrlRewriteRule res)
        {
            if (match == null)
            {
                throw new FormatException("Rules must have an associated match.");
            }

            var matchRes = new ParsedUrlMatch();

            bool parBool;
            if (bool.TryParse(match.Attribute(RewriteTags.IgnoreCase)?.Value, out parBool))
            {
                matchRes.IgnoreCase = parBool;
            }

            if (bool.TryParse(match.Attribute(RewriteTags.Negate)?.Value, out parBool))
            {
                matchRes.Negate = parBool;
            }

            var parsedInputString = match.Attribute(RewriteTags.Url)?.Value;

            switch (res.PatternSyntax)
            {
                case PatternSyntax.ECMAScript:
                    {
                        if (matchRes.IgnoreCase)
                        {
                            var regex = new Regex(parsedInputString, RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexTimeout);
                            res.InitialMatch = new RegexMatch(regex, matchRes.Negate);
                        }
                        else
                        {
                            var regex = new Regex(parsedInputString, RegexOptions.Compiled, RegexTimeout);
                            res.InitialMatch = new RegexMatch(regex, matchRes.Negate);
                        }
                    }
                    break;
                case PatternSyntax.WildCard:
                    throw new NotImplementedException("Wildcard syntax is not supported.");
                case PatternSyntax.ExactMatch:
                    res.InitialMatch = new ExactMatch(matchRes.IgnoreCase, parsedInputString, matchRes.Negate);
                    break;
            }
        }


        private static void CreateConditions(XElement conditions, UrlRewriteRule res)
        {
            // This is to avoid nullptr exception on referencing conditions.
            res.Conditions = new Conditions();
            if (conditions == null)
            {
                return; // TODO make sure no null exception on Conditions
            }

            LogicalGrouping grouping;
            if (Enum.TryParse(conditions.Attribute(RewriteTags.MatchType)?.Value, out grouping))
            {
                res.Conditions.MatchType = grouping;
            }

            bool parBool;
            if (bool.TryParse(conditions.Attribute(RewriteTags.TrackingAllCaptures)?.Value, out parBool))
            {
                res.Conditions.TrackingAllCaptures = parBool;
            }

            foreach (var cond in conditions.Elements(RewriteTags.Add))
            {
                CreateCondition(cond, res);
            }
        }

        private static void CreateCondition(XElement condition, UrlRewriteRule res)
        {
            if (condition == null)
            {
                return;
            }

            var parsedCondRes = new ParsedCondition();

            bool parBool;
            if (bool.TryParse(condition.Attribute(RewriteTags.IgnoreCase)?.Value, out parBool))
            {
                parsedCondRes.IgnoreCase = parBool;
            }

            if (bool.TryParse(condition.Attribute(RewriteTags.Negate)?.Value, out parBool))
            {
                parsedCondRes.Negate = parBool;
            }

            MatchType matchType;
            if (Enum.TryParse(condition.Attribute(RewriteTags.MatchPattern)?.Value, out matchType))
            {
                parsedCondRes.MatchType = matchType;
            }

            var parsedInputString = condition.Attribute(RewriteTags.Input)?.Value;
            if (parsedInputString == null)
            {
                throw new FormatException("Null input for condition");
            }
            var input = InputParser.ParseInputString(parsedInputString);


            parsedInputString = condition.Attribute(RewriteTags.Pattern)?.Value;
            if (parsedInputString == null)
            {
                throw new FormatException("Null pattern for condition.");
            }
            switch (res.PatternSyntax)
            {
                case PatternSyntax.ECMAScript:
                    {
                        switch (parsedCondRes.MatchType)
                        {
                            case MatchType.Pattern:
                                {
                                    if (parsedCondRes.IgnoreCase)
                                    {
                                        var regex = new Regex(parsedInputString, RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexTimeout);
                                        res.Conditions.ConditionList.Add(new Condition { Input = input, Match = new RegexMatch(regex, parsedCondRes.Negate) });

                                    }
                                    else
                                    {
                                        var regex = new Regex(parsedInputString, RegexOptions.Compiled, RegexTimeout);
                                        res.Conditions.ConditionList.Add(new Condition { Input = input, Match = new RegexMatch(regex, parsedCondRes.Negate) });
                                    }
                                }
                                break;
                            case MatchType.IsDirectory:
                                {
                                    res.Conditions.ConditionList.Add(new Condition { Input = input, Match = new IsDirectoryMatch(parsedCondRes.Negate) });
                                }
                                break;
                            case MatchType.IsFile:
                                {
                                    res.Conditions.ConditionList.Add(new Condition { Input = input, Match = new IsFileMatch(parsedCondRes.Negate) });
                                }
                                break;
                            default:
                                throw new FormatException("Unrecognized matchType.");
                        }
                      
                    }
                    break;
                case PatternSyntax.WildCard:
                    throw new NotImplementedException("Wildcard syntax is not supported.");
                case PatternSyntax.ExactMatch:
                    res.Conditions.ConditionList.Add(new Condition { Input = input, Match = new ExactMatch(parsedCondRes.IgnoreCase, parsedInputString, parsedCondRes.Negate) });
                    break;
            }
        }

        private static void CreateUrlAction(XElement urlAction, bool globalRule, UrlRewriteRule res)
        {
            if (urlAction == null)
            {
                throw new FormatException("Action is a required element of a rule.");
            }
            var actionRes = new ParsedUrlAction();

            ActionType actionType;
            if (Enum.TryParse(urlAction.Attribute(RewriteTags.Type)?.Value, out actionType))
            {
                actionRes.Type = actionType;
            }

            bool parseBool;
            if (bool.TryParse(urlAction.Attribute(RewriteTags.AppendQuery)?.Value, out parseBool))
            {
                actionRes.AppendQueryString = parseBool;
            }

            if (bool.TryParse(urlAction.Attribute(RewriteTags.LogRewrittenUrl)?.Value, out parseBool))
            {
                actionRes.LogRewrittenUrl = parseBool;
            }

            RedirectType redirectType;
            if (Enum.TryParse(urlAction.Attribute(RewriteTags.RedirectType)?.Value, out redirectType))
            {
                actionRes.RedirectType = redirectType;
            }

            actionRes.Url = InputParser.ParseInputString(urlAction.Attribute(RewriteTags.Url)?.Value);

            CreateUrlActionFromParsedAction(actionRes, globalRule, res);
        }

        public static void CreateUrlActionFromParsedAction(ParsedUrlAction actionRes, bool globalRule, UrlRewriteRule res)
        {
            switch (actionRes.Type)
            {
                case ActionType.None:
                    res.Action = new VoidAction();
                    break;
                case ActionType.Rewrite:
                    if (globalRule)
                    {
                        // TODO create action 
                    }
                    else
                    {
                        if (actionRes.AppendQueryString)
                        {
                            res.Action = new RewriteAction(res.StopProcessing ? RuleTerminiation.StopRules : RuleTerminiation.Continue, actionRes.Url);
                        }
                        else
                        {
                            res.Action = new RewriteClearQueryAction(res.StopProcessing ? RuleTerminiation.StopRules : RuleTerminiation.Continue, actionRes.Url);
                        }
                    }
                    break;
                case ActionType.Redirect:
                    if (globalRule)
                    {
                        // TODO create action
                    }
                    else
                    {
                        if (actionRes.AppendQueryString)
                        {
                            res.Action = new RedirectAction((int)actionRes.RedirectType, actionRes.Url);
                        }
                        else
                        {
                            res.Action = new RedirectClearQueryAction((int)actionRes.RedirectType, actionRes.Url);
                        }
                    }
                    break;
                case ActionType.AbortRequest:
                    break;
                case ActionType.CustomResponse:
                    // TODO
                    throw new FormatException("Custom Responses are not supported");
            }
        }
    }
}
