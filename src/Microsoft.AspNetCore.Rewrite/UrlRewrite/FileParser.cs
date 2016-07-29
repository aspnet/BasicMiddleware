using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.AspNetCore.Rewrite.RuleAbstraction;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class FileParser
    {
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(1);
        public static List<UrlRewriteRule> Parse(TextReader reader)
        {
            var temp = XDocument.Load(reader);
            var xmlRoot = temp.Descendants("rewrite");
            var rules = new List<UrlRewriteRule>();
            if (xmlRoot != null)
            {
                // there is a valid rewrite block, go through each rule and process
                rules.AddRange(GetGlobalRules(xmlRoot.Descendants("globalRules")));
                rules.AddRange(GetRules(xmlRoot.Descendants("rules")));
            }
            return rules;
        }

        private static List<UrlRewriteRule> GetGlobalRules(IEnumerable<XElement> globalRules)
        {
            var result = new List<UrlRewriteRule>();
            foreach (var rule in globalRules.Elements("rule") ?? Enumerable.Empty<XElement>())
            {
                var res = new UrlRewriteRule();
                SetRuleAttributes(rule, res);
                //res.Action = CreateGlobalUrlAction(rule.Element("action"));
                result.Add(res);
            }
            return result;
        }

        private static List<UrlRewriteRule> GetRules(IEnumerable<XElement> rules)
        {
            var result = new List<UrlRewriteRule>();
            // TODO Better null check?
            foreach (var rule in rules.Elements("rule") ?? Enumerable.Empty<XElement>())
            {
                var res = new UrlRewriteRule();
                SetRuleAttributes(rule, res);
                res.Action = CreateUrlAction(rule.Element("action"));
                result.Add(res);
            }
            return result;
        }

        private static void SetRuleAttributes(XElement rule, UrlRewriteRule res)
        {
            if (rule == null)
            {
                return;
            }

            res.Name =  rule?.Attribute("name")?.Value;

            bool enabled;
            if (bool.TryParse(rule.Attribute("enabled")?.Value, out enabled))
            {
                res.Enabled = enabled;
            }

            PatternSyntax patternSyntax;
            if (Enum.TryParse(rule.Attribute("patternSyntax")?.Value, out patternSyntax))
            {
                res.PatternSyntax = patternSyntax;
            }

            bool stopProcessing;
            if (bool.TryParse(rule.Attribute("stopProcessing")?.Value, out stopProcessing))
            {
                res.StopProcessing = stopProcessing;
            }
            // Create match here:
            res.Match = CreateMatch(rule.Element("match"));
            res.Conditions = CreateConditions(rule.Element("condition"));
        }

        private static InitialMatch CreateMatch(XElement match)
        {
            if (match == null)
            {
                return null;
            }
            var matchRes = new InitialMatch();
            bool parBool;
            if (bool.TryParse(match.Attribute("ignoreCase")?.Value, out parBool))
            {
                matchRes.IgnoreCase = parBool;
            }

            if (bool.TryParse(match.Attribute("negate")?.Value, out parBool))
            {
                matchRes.Negate = parBool;
            }

            var parString = match?.Attribute("url")?.Value;

            if (matchRes.IgnoreCase)
            {
                matchRes.Url = new Regex(parString, RegexOptions.IgnoreCase | RegexOptions.Compiled, RegexTimeout);
            }
            else
            {
                matchRes.Url = new Regex(parString, RegexOptions.Compiled, RegexTimeout);
            }

            return matchRes;
        }


        private static Conditions CreateConditions(XElement conditions)
        {
            if (conditions == null)
            {
                return null; // TODO make sure no null exception on Conditions
            }

            var condRes = new Conditions();
            LogicalGrouping grouping;
            if (Enum.TryParse(conditions.Attribute("matchType")?.Value, out grouping))
            {
                condRes.MatchType = grouping;
            }

            bool parBool;
            if (bool.TryParse(conditions.Attribute("trackingAllCapture")?.Value, out parBool))
            {
                condRes.TrackingAllCaptures = parBool;
            }

            foreach (var cond in conditions.Elements("add"))
            {
                condRes.ConditionList.Add(CreateCondition(cond));
            }
            return condRes;
        }

        private static Condition CreateCondition(XElement condition)
        {
            if (condition == null)
            {
                return null;
            }
            var condRes = new Condition();
            bool parBool;
            if (bool.TryParse(condition.Attribute("ignoreCase")?.Value, out parBool))
            {
                condRes.IgnoreCase = parBool;
            }

            if (bool.TryParse(condition.Attribute("negate")?.Value, out parBool))
            {
                condRes.Negate = parBool;
            }

            MatchType matchType;
            if (Enum.TryParse(condition.Attribute("matchPattern")?.Value, out matchType))
            {
                condRes.MatchType = matchType;
            }
            var parString = condition?.Attribute("input")?.Value;
            if (parString != null)
            {
                condRes.Input = InputParser.ParseInputString(parString);
            }

            parString = condition.Attribute("pattern")?.Value;
            if (condRes.IgnoreCase)
            {
                condRes.MatchPattern = new Regex(parString, RegexOptions.IgnoreCase | RegexOptions.Compiled, RegexTimeout);
            }
            else
            {
                condRes.MatchPattern = new Regex(parString, RegexOptions.Compiled, RegexTimeout);
            }
            return condRes;
        }

        private static UrlAction CreateUrlAction(XElement urlAction)
        {
            var actionRes = new UrlAction();
            if (urlAction == null)
            {
                throw new FormatException("Action is a required element of a rule.");
            }

            ActionType actionType;
            if (Enum.TryParse(urlAction.Attribute("type")?.Value, out actionType))
            {
                actionRes.Type = actionType;
            }

            bool parseBool;
            if (bool.TryParse(urlAction.Attribute("appendQueryString")?.Value, out parseBool))
            {
                actionRes.AppendQueryString = parseBool;
            }

            if (bool.TryParse(urlAction.Attribute("logRewrittenUrl")?.Value, out parseBool))
            {
                actionRes.LogRewrittenUrl = parseBool;
            }

            RedirectType redirectType;
            if (Enum.TryParse(urlAction.Attribute("redirectType")?.Value, out redirectType))
            {
                actionRes.RedirectType = redirectType;
            }

            actionRes.Url = InputParser.ParseInputString(urlAction.Attribute("url")?.Value);
            return actionRes;
        }
    }
}
