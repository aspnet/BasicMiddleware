using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.AspNetCore.Rewrite.RuleAbstraction;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class FileParser
    {
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(1);
        public static List<Rule> Parse(TextReader reader)
        {
            var temp = XDocument.Load(reader);
            var xmlRoot = temp.Descendants("rewrite");
            var rules = new List<Rule>();
            if (xmlRoot != null)
            {
                // there is a valid rewrite block, go through each rule and process
                rules.AddRange(GetGlobalRules(xmlRoot.Descendants("globalRules")));
                rules.AddRange(GetRules(xmlRoot.Descendants("rules")));

            }
            return rules;
        }

        private static List<Rule> GetGlobalRules(IEnumerable<XElement> globalRules)
        {
            var result = new List<Rule>();
            foreach (var rule in globalRules.Elements("rule"))
            {
                
            }
            return result;
        }

        private static List<Rule> GetRules(IEnumerable<XElement> rules)
        {
            var result = new List<Rule>();
            foreach (var rule in rules.Elements("rule"))
            {
                var res = new Rule();
                SetRuleAttributes(rule, res);
                ProcessMatch(rule.Element("match"));
            }
            return result;
        }

        private static void ProcessMatch(XElement match)
        {
            var url = match?.Attribute("url");
            if (url == null)
            {
                throw new FormatException("Cannot have rule without match");
            }
            var ignoreCase = match?.Attribute("ignoreCase");
            var negate = match?.Attribute("negate");

            // add the regex to the precompiled operation 
        }

        private static void SetRuleAttributes(XElement rule, Rule res)
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

        private static Match CreateMatch(XElement match)
        {
            if (match == null)
            {
                return null;
            }
            var matchRes = new Match();
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
                return null; // TODO make sure no null pointer exception on Conditions
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
        private static void SetActionAttributes(XElement action)
        {
            var type = action.Attribute("type");
            var url = action.Attribute("url");
            var appendQueryString = action.Attribute("appendQueryString");
            var logRewrittenUrl =  action.Attribute("logRewrittenUrl");
            var redirectType = action.Attribute("redirectType");
        }

        private static void SetConditionAttributes(XElement condition)
        {
            var logicalGrouping = condition.Attribute("logicalGrouping");
            var trackAllCaptures = condition.Attribute("trackAllCaptures");
            foreach (var condElement in condition.Elements("add"))
            {
                var input = condElement.Attribute("input");
                var matchType = condElement.Attribute("matchType");
                var pattern = condElement.Attribute("pattern");
                var ignorePattern = condElement.Attribute("ignorePattern");
                var negate = condElement.Attribute("negate");
            }
        }
    }
}
