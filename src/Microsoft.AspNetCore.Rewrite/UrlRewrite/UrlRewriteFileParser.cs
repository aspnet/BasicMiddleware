// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite.RuleAbstraction;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    // TODO rename 
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
            foreach (var rule in globalRules.Elements(RewriteTags.Rule) ?? Enumerable.Empty<XElement>())
            {
                var res = new UrlRewriteRule();
                SetRuleAttributes(rule, res);
                // TODO handle full url with global rules - may or may not support
                res.Action = CreateUrlAction(rule.Element(RewriteTags.Action), true, res);
                result.Add(res);
            }
        }

        private static void GetRules(IEnumerable<XElement> rules, List<UrlRewriteRule> result)
        {
            // TODO Better null check?
            foreach (var rule in rules.Elements(RewriteTags.Rule) ?? Enumerable.Empty<XElement>())
            {
                var res = new UrlRewriteRule();
                SetRuleAttributes(rule, res);
                res.Action = CreateUrlAction(rule.Element(RewriteTags.Action), false);
                result.Add(res);
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

            var matchRes = new InitialMatch();

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
                            matchRes.Match = path =>
                            {
                                var pathMatch = regex.Match(path);
                                return new MatchResults { BackReference = pathMatch.Groups, Success = pathMatch.Success == matchRes.Negate};
                            };
                        }
                        else
                        {
                            var regex = new Regex(parsedInputString, RegexOptions.Compiled, RegexTimeout);
                            matchRes.Match = path =>
                            {
                                var pathMatch = regex.Match(path);
                                return new MatchResults { BackReference = pathMatch.Groups, Success = pathMatch.Success == matchRes.Negate };
                            };
                        }
                    }
                    break;
                case PatternSyntax.WildCard:
                    throw new NotImplementedException("Wildcard syntax is not supported.");
                case PatternSyntax.ExactMatch:
                    matchRes.Match = path =>
                    {
                        var pathMatch = string.Compare(parsedInputString, path, matchRes.IgnoreCase);
                        return new MatchResults { Success = pathMatch == 0 };
                    };
                    break;
            }
            res.InitialMatch = matchRes;
        }


        private static void CreateConditions(XElement conditions, UrlRewriteRule res)
        {
            if (conditions == null)
            {
                return; // TODO make sure no null exception on Conditions
            }


            res.Conditions = new Conditions();
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

            var condRes = new Condition();

            bool parBool;
            if (bool.TryParse(condition.Attribute(RewriteTags.IgnoreCase)?.Value, out parBool))
            {
                condRes.IgnoreCase = parBool;
            }

            if (bool.TryParse(condition.Attribute(RewriteTags.Negate)?.Value, out parBool))
            {
                condRes.Negate = parBool;
            }

            MatchType matchType;
            if (Enum.TryParse(condition.Attribute(RewriteTags.MatchPattern)?.Value, out matchType))
            {
                condRes.MatchType = matchType;
            }

            var parsedInputString = condition.Attribute(RewriteTags.Input)?.Value;
            if (parsedInputString != null)
            {
                condRes.Input = InputParser.ParseInputString(parsedInputString);
            }

            parsedInputString = condition.Attribute(RewriteTags.Pattern)?.Value;


            switch (res.PatternSyntax)
            {
                case PatternSyntax.ECMAScript:
                    {
                        if (condRes.IgnoreCase)
                        {
                            var regex = new Regex(parsedInputString, RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexTimeout);
                            condRes.Match = path =>
                            {
                                var pathMatch = regex.Match(path);
                                return new MatchResults { BackReference = pathMatch.Groups, Success = pathMatch.Success == condRes.Negate };
                            };
                        }
                        else
                        {
                            var regex = new Regex(parsedInputString, RegexOptions.Compiled, RegexTimeout);
                            condRes.Match = path =>
                            {
                                var pathMatch = regex.Match(path);
                                return new MatchResults { BackReference = pathMatch.Groups, Success = pathMatch.Success == condRes.Negate };
                            };
                        }
                    }
                    break;
                case PatternSyntax.WildCard:
                    throw new NotImplementedException("Wildcard syntax is not supported.");
                case PatternSyntax.ExactMatch:
                    condRes.Match = path =>
                    {
                        var pathMatch = string.Compare(parsedInputString, path, condRes.IgnoreCase);
                        return new MatchResults { Success = pathMatch == 0 };
                    };
                    break;
            }
            res.Conditions.ConditionList.Add(condRes);
        }

        private static UrlAction CreateUrlAction(XElement urlAction, bool globalRule)
        {
            if (urlAction == null)
            {
                throw new FormatException("Action is a required element of a rule.");
            }
            var actionRes = new UrlAction();

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

            CreateOnMatchAction(actionRes, globalRule);
            return actionRes;
        }

        public static void CreateOnMatchAction(UrlAction actionRes, bool globalRule)
        {
            switch (actionRes.Type)
            {
                case ActionType.None:
                    actionRes.Evaluate = (pattern, context) => { };
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
                            actionRes.Evaluate = (pattern, context) =>
                            {
                                context.Request.Path = new PathString(pattern);
                            };
                        }
                        else
                        {
                            actionRes.Evaluate = (pattern, context) =>
                            {
                                context.Request.Path = new PathString(pattern);
                                context.Request.QueryString = new QueryString();
                            };
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
                            actionRes.Evaluate = (pattern, context) =>
                            {
                                context.Response.StatusCode = (int)actionRes.RedirectType;
                                // TODO probably need to add a forward slash here
                                context.Response.Headers[HeaderNames.Location] = pattern + context.Request.QueryString;
                            };
                        }
                        else
                        {
                            actionRes.Evaluate = (pattern, context) =>
                            {
                                context.Response.StatusCode = (int)actionRes.RedirectType;
                                // TODO probably need to add a forward slash here
                                context.Response.Headers[HeaderNames.Location] = pattern;
                            };
                        }
                    }
                    break;
                case ActionType.AbortRequest:
                    actionRes
                    break;
                case ActionType.CustomResponse:
                    // TODO
                    throw new FormatException("Custom Responses are not supported");
            }
        }
    }
}
