// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.Rewrite.Internal.UrlActions;

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
                // TODO Global rules are currently not treated differently than normal rules, fix.
                // See: https://github.com/aspnet/BasicMiddleware/issues/59
                ParseRules(xmlRoot.Descendants(RewriteTags.GlobalRules).FirstOrDefault(), result);
                ParseRules(xmlRoot.Descendants(RewriteTags.Rules).FirstOrDefault(), result);
                return result;
            }
            return null;
        }

        private void ParseRules(XElement rules, IList<IISUrlRewriteRule> result)
        {
            if (rules == null)
            {
                return;
            }

            if (string.Equals(rules.Name.ToString(), "GlobalRules", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("Support for global rules has not been implemented yet");
            }

            foreach (var rule in rules.Elements(RewriteTags.Rule))
            {
                var builder = new UrlRewriteRuleBuilder();
                ParseRuleAttributes(rule, builder);

                if (builder.Enabled)
                {
                    result.Add(builder.Build());
                }
            }
        }

        private void ParseRuleAttributes(XElement rule, UrlRewriteRuleBuilder builder)
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
                throw new InvalidUrlRewriteFormatException(rule, "Condition must have an associated match");
            }

            var action = rule.Element(RewriteTags.Action);
            if (action == null)
            {
                throw new InvalidUrlRewriteFormatException(rule, "Rule does not have an associated action attribute");
            }

            ParseMatch(match, builder, patternSyntax);
            ParseConditions(rule.Element(RewriteTags.Conditions), builder, patternSyntax);
            ParseUrlAction(action, builder, stopProcessing);
        }

        private void ParseMatch(XElement match, UrlRewriteRuleBuilder builder, PatternSyntax patternSyntax)
        {
            var parsedInputString = match.Attribute(RewriteTags.Url)?.Value;
            if (parsedInputString == null)
            {
                throw new InvalidUrlRewriteFormatException(match, "Match must have Url Attribute");
            }

            var ignoreCase = ParseBool(match, RewriteTags.IgnoreCase, defaultValue: true);
            var negate = ParseBool(match, RewriteTags.Negate, defaultValue: false);
            builder.AddUrlMatch(parsedInputString, ignoreCase, negate, patternSyntax);
        }

        private void ParseConditions(XElement conditions, UrlRewriteRuleBuilder builder, PatternSyntax patternSyntax)
        {
            if (conditions == null)
            {
                return;
            }

            var grouping = ParseEnum(conditions, RewriteTags.LogicalGrouping, LogicalGrouping.MatchAll);
            var trackAllCaptures = ParseBool(conditions, RewriteTags.TrackAllCaptures, defaultValue: false);
            builder.AddUrlConditions(grouping, trackAllCaptures);

            foreach (var cond in conditions.Elements(RewriteTags.Add))
            {
                ParseCondition(cond, builder, patternSyntax, trackAllCaptures);
            }
        }

        private void ParseCondition(XElement condition, UrlRewriteRuleBuilder builder, PatternSyntax patternSyntax, bool trackAllCaptures)
        {
            var ignoreCase = ParseBool(condition, RewriteTags.IgnoreCase, defaultValue: true);
            var negate = ParseBool(condition, RewriteTags.Negate, defaultValue: false);
            var matchType = ParseEnum(condition, RewriteTags.MatchType, MatchType.Pattern);
            var parsedInputString = condition.Attribute(RewriteTags.Input)?.Value;

            if (parsedInputString == null)
            {
                throw new InvalidUrlRewriteFormatException(condition, "Conditions must have an input attribute");
            }

            var parsedPatternString = condition.Attribute(RewriteTags.Pattern)?.Value;
            var input = _inputParser.ParseInputString(parsedInputString);
            builder.AddUrlCondition(input, parsedPatternString, patternSyntax, matchType, ignoreCase, negate, trackAllCaptures);
        }

        private void ParseUrlAction(XElement urlAction, UrlRewriteRuleBuilder builder, bool stopProcessing)
        {
            var actionType = ParseEnum(urlAction, RewriteTags.Type, ActionType.None);
            UrlAction action;
            switch (actionType)
            {
                case ActionType.None:
                    action = new NoneAction(stopProcessing ? RuleResult.SkipRemainingRules : RuleResult.ContinueRules);
                    break;
                case ActionType.Rewrite:
                case ActionType.Redirect:
                    var url = string.Empty;
                    if (urlAction.Attribute(RewriteTags.Url) != null)
                    {
                        url = urlAction.Attribute(RewriteTags.Url).Value;
                        if (string.IsNullOrEmpty(url))
                        {
                            throw new InvalidUrlRewriteFormatException(urlAction, "Url attribute cannot contain an empty string");
                        }
                    }

                    var urlPattern = _inputParser.ParseInputString(url);
                    var appendQuery = ParseBool(urlAction, RewriteTags.AppendQueryString, defaultValue: true);

                    if (actionType == ActionType.Rewrite)
                    {
                        action = new RewriteAction(stopProcessing ? RuleResult.SkipRemainingRules : RuleResult.ContinueRules, urlPattern, appendQuery);
                    }
                    else
                    {
                        var redirectType = ParseEnum(urlAction, RewriteTags.RedirectType, RedirectType.Permanent);

                        action = new RedirectAction((int)redirectType, urlPattern, appendQuery);
                    }
                    break;
                case ActionType.AbortRequest:
                    action = new AbortAction();
                    break;
                case ActionType.CustomResponse:
                    int statusCode;
                    if (!int.TryParse(urlAction.Attribute(RewriteTags.StatusCode)?.Value, out statusCode))
                    {
                        throw new InvalidUrlRewriteFormatException(urlAction, "A valid status code is required");
                    }

                    if (statusCode < 200 || statusCode > 999)
                    {
                        throw new NotSupportedException("Status codes must be between 200 and 999 (inclusive)");
                    }

                    if (!string.IsNullOrEmpty(urlAction.Attribute(RewriteTags.SubStatusCode)?.Value))
                    {
                        throw new NotSupportedException("Substatus codes are not supported");
                    }

                    var statusReason = urlAction.Attribute(RewriteTags.StatusReason)?.Value;
                    var statusDescription = urlAction.Attribute(RewriteTags.StatusDescription)?.Value;

                    action = new CustomResponseAction(statusCode) { StatusReason = statusReason, StatusDescription = statusDescription };
                    break;
                default:
                    throw new NotSupportedException($"The action type {actionType} wasn't recognized");
            }
            builder.AddUrlAction(action);
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
                throw new InvalidUrlRewriteFormatException(element, $"The {rewriteTag} parameter '{attribute.Value}' was not recognized");
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
                throw new InvalidUrlRewriteFormatException(element, $"The {rewriteTag} parameter '{attribute.Value}' was not recognized");
            }
            return enumResult;
        }
    }
}
