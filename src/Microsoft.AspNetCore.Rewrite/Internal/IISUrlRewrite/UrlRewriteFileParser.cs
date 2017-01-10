// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

using Microsoft.AspNetCore.Rewrite.Internal.UrlMatches;

namespace Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite
{
    public class UrlRewriteFileParser
    {
        private readonly InputParser _inputParser = new InputParser();

        public IList<IISUrlRewriteRule> Parse(TextReader reader)
        {
            var xmlDoc = XDocument.Load(reader, LoadOptions.SetLineInfo);
            var xmlRoot = xmlDoc.Descendants(RewriteTags.Rewrite).FirstOrDefault();

            if (xmlRoot != null)
            {
                var result = new List<IISUrlRewriteRule>();
                ParseRules(xmlRoot.Descendants(RewriteTags.GlobalRules).FirstOrDefault(), result, global: true);
                ParseRules(xmlRoot.Descendants(RewriteTags.Rules).FirstOrDefault(), result, global: false);
                return result;
            }
            return null;
        }

        private void ParseRules(XElement rules, IList<IISUrlRewriteRule> result, bool global)
        {
            if (rules == null)
            {
                return;
            }

            foreach (var rule in rules.Elements(RewriteTags.Rule))
            {
                var builder = new UrlRewriteRuleBuilder();
                ParseRuleAttributes(rule, builder, global);

                if (builder.Enabled)
                {
                    result.Add(builder.Build(global));
                }
            }
        }

        private void ParseRuleAttributes(XElement rule, UrlRewriteRuleBuilder builder, bool global)
        {
            builder.Name = rule.Attribute(RewriteTags.Name)?.Value;

            if (ParseBool(rule, RewriteTags.Enabled, defaultValue: true))
            {
                builder.Enabled = true;
            }
            else
            {
                return;
            }

            var patternSyntax = ParseEnum(rule, RewriteTags.PatternSyntax, PatternSyntax.ECMAScript);
            var stopProcessing = ParseBool(rule, RewriteTags.StopProcessing, defaultValue: false);

            var match = rule.Element(RewriteTags.Match);
            if (match == null)
            {
                ThrowUrlFormatException(rule, "Cannot have rule without match");
            }

            var action = rule.Element(RewriteTags.Action);
            if (action == null)
            {
                ThrowUrlFormatException(rule, "Rule does not have an associated action attribute");
            }

            ParseMatch(match, builder, patternSyntax);
            ParseConditions(rule.Element(RewriteTags.Conditions), builder, patternSyntax, global);
            ParseUrlAction(action, builder, stopProcessing, global);
        }

        private void ParseMatch(XElement match, UrlRewriteRuleBuilder builder, PatternSyntax patternSyntax)
        {
            var parsedInputString = match.Attribute(RewriteTags.Url)?.Value;
            if (parsedInputString == null)
            {
                ThrowUrlFormatException(match, "Match must have Url Attribute");
            }

            var ignoreCase = ParseBool(match, RewriteTags.IgnoreCase, defaultValue: true);
            var negate = ParseBool(match, RewriteTags.Negate, defaultValue: false);
            builder.AddUrlMatch(parsedInputString, ignoreCase, negate, patternSyntax);
        }

        private void ParseConditions(XElement conditions, UrlRewriteRuleBuilder builder, PatternSyntax patternSyntax, bool global)
        {
            if (conditions == null)
            {
                return;
            }

            var grouping = ParseEnum(conditions, RewriteTags.LogicalGrouping, LogicalGrouping.MatchAll);
            var trackAllCaptures = ParseBool(conditions, RewriteTags.TrackAllCaptures, defaultValue: false);
            builder.ConfigureConditionBehavior(grouping, trackAllCaptures);

            foreach (var cond in conditions.Elements(RewriteTags.Add))
            {
                ParseCondition(cond, builder, patternSyntax, global);
            }
        }

        private void ParseCondition(XElement conditionElement, UrlRewriteRuleBuilder builder, PatternSyntax patternSyntax, bool global)
        {
            var ignoreCase = ParseBool(conditionElement, RewriteTags.IgnoreCase, defaultValue: true);
            var negate = ParseBool(conditionElement, RewriteTags.Negate, defaultValue: false);
            var matchType = ParseEnum(conditionElement, RewriteTags.MatchType, MatchType.Pattern);
            var parsedInputString = conditionElement.Attribute(RewriteTags.Input)?.Value;

            if (parsedInputString == null)
            {
                ThrowUrlFormatException(conditionElement, "Conditions must have an input attribute");
            }

            var parsedPatternString = conditionElement.Attribute(RewriteTags.Pattern)?.Value;

            try
            {
                Condition condition;
                UriMatchPart uriMatchPart = global ? UriMatchPart.Full : UriMatchPart.Path;

                switch (patternSyntax)
                {
                    case PatternSyntax.ECMAScript:
                        {
                            switch (matchType)
                            {
                                case MatchType.Pattern:
                                    {
                                        if (string.IsNullOrEmpty(parsedPatternString))
                                        {
                                            throw new FormatException("Match does not have an associated pattern attribute in condition");
                                        }
                                        condition = new UriMatchCondition(parsedInputString, parsedPatternString, uriMatchPart, ignoreCase, negate);
                                        break;
                                    }
                                case MatchType.IsDirectory:
                                    {
                                        condition = new Condition { Input = _inputParser.ParseInputString(parsedInputString, uriMatchPart), Match = new IsDirectoryMatch(negate) };
                                        break;
                                    }
                                case MatchType.IsFile:
                                    {
                                        condition = new Condition { Input = _inputParser.ParseInputString(parsedInputString, uriMatchPart), Match = new IsFileMatch(negate) };
                                        break;
                                    }
                                default:
                                    throw new FormatException("Unrecognized matchType");
                            }
                            break;
                        }
                    case PatternSyntax.Wildcard:
                        throw new NotSupportedException("Wildcard syntax is not supported");
                    case PatternSyntax.ExactMatch:
                        if (string.IsNullOrEmpty(parsedPatternString))
                        {
                            throw new FormatException("Match does not have an associated pattern attribute in condition");
                        }
                        condition = new Condition { Input = _inputParser.ParseInputString(parsedInputString, uriMatchPart), Match = new ExactMatch(ignoreCase, parsedPatternString, negate) };
                        break;
                    default:
                        throw new FormatException("Unrecognized pattern syntax");
                }

                builder.AddUrlCondition(condition);
            }
            catch (FormatException formatException)
            {
                ThrowUrlFormatException(conditionElement, formatException.Message, formatException);
            }
        }

        private void ParseUrlAction(XElement urlAction, UrlRewriteRuleBuilder builder, bool stopProcessing, bool global)
        {
            var actionType = ParseEnum(urlAction, RewriteTags.Type, ActionType.None);
            var redirectType = ParseEnum(urlAction, RewriteTags.RedirectType, RedirectType.Permanent);
            var appendQuery = ParseBool(urlAction, RewriteTags.AppendQueryString, defaultValue: true);

            string url = string.Empty;
            if (urlAction.Attribute(RewriteTags.Url) != null)
            {
                url = urlAction.Attribute(RewriteTags.Url).Value;
                if (string.IsNullOrEmpty(url))
                {
                    ThrowUrlFormatException(urlAction, "Url attribute cannot contain an empty string");
                }
            }

            try
            {
                var input = _inputParser.ParseInputString(url, global ? UriMatchPart.Full : UriMatchPart.Path);
                builder.AddUrlAction(input, actionType, appendQuery, stopProcessing, (int)redirectType);
            }
            catch (FormatException formatException)
            {
                ThrowUrlFormatException(urlAction, formatException.Message, formatException);
            }
        }

        private static void ThrowUrlFormatException(XElement element, string message)
        {
            var lineInfo = (IXmlLineInfo)element;
            var line = lineInfo.LineNumber;
            var col = lineInfo.LinePosition;
            throw new FormatException(Resources.FormatError_UrlRewriteParseError(message, line, col));
        }

        private static void ThrowUrlFormatException(XElement element, string message, Exception ex)
        {
            var lineInfo = (IXmlLineInfo)element;
            var line = lineInfo.LineNumber;
            var col = lineInfo.LinePosition;
            throw new FormatException(Resources.FormatError_UrlRewriteParseError(message, line, col), ex);
        }

        private static void ThrowParameterFormatException(XElement element, string message)
        {
            var lineInfo = (IXmlLineInfo)element;
            var line = lineInfo.LineNumber;
            var col = lineInfo.LinePosition;
            throw new FormatException(Resources.FormatError_UrlRewriteParseError(message, line, col));
        }

        private bool ParseBool(XElement element, string rewriteTag, bool defaultValue)
        {
            bool result;
            var attribute = element.Attribute(rewriteTag);
            if (attribute == null)
            {
                return defaultValue;
            }
            else if (!bool.TryParse(attribute.Value, out result))
            {
                ThrowParameterFormatException(element, $"The {rewriteTag} parameter '{attribute.Value}' was not recognized");
            }
            return result;
        }

        private TEnum ParseEnum<TEnum>(XElement element, string rewriteTag, TEnum defaultValue)
            where TEnum : struct
        {
            TEnum enumResult = default(TEnum);
            var attribute = element.Attribute(rewriteTag);
            if (attribute == null)
            {
                return defaultValue;
            }
            else if(!Enum.TryParse(attribute.Value, ignoreCase: true, result: out enumResult))
            {
                ThrowParameterFormatException(element, $"The {rewriteTag} parameter '{attribute.Value}' was not recognized");
            }
            return enumResult;
        }
    }
}
