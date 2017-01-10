// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite.Internal.UrlActions;
using Microsoft.AspNetCore.Rewrite.Internal.UrlMatches;

namespace Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite
{
    public class UrlRewriteRuleBuilder
    {
        private readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(1);

        public string Name { get; set; }
        public bool Enabled { get; set; }

        private UrlMatch _initialMatch;
        private ConditionCollection _conditions;
        private UrlAction _action;

        public IISUrlRewriteRule Build(bool global)
        {
            if (_initialMatch == null || _action == null)
            {
                throw new InvalidOperationException("Cannot create UrlRewriteRule without action and match");
            }

            return new IISUrlRewriteRule(Name, _initialMatch, _conditions, _action, global);
        }

        public void AddUrlAction(
            Pattern url,
            ActionType actionType = ActionType.None,
            bool appendQueryString = true,
            bool stopProcessing = false,
            int statusCode = StatusCodes.Status301MovedPermanently)
        {
            switch (actionType)
            {
                case ActionType.None:
                    _action = new VoidAction(stopProcessing ? RuleResult.SkipRemainingRules : RuleResult.ContinueRules);
                    break;
                case ActionType.Rewrite:
                    _action = new RewriteAction(stopProcessing ? RuleResult.SkipRemainingRules : RuleResult.ContinueRules,
                        url, appendQueryString);
                    break;
                case ActionType.Redirect:
                    _action = new RedirectAction(statusCode, url, appendQueryString);
                    break;
                case ActionType.AbortRequest:
                    _action = new AbortAction();
                    break;
                case ActionType.CustomResponse:
                    throw new NotImplementedException("Custom Responses are not implemented");
            }
        }

        public void AddUrlMatch(string input, bool ignoreCase = true, bool negate = false, PatternSyntax patternSyntax = PatternSyntax.ECMAScript)
        {
            switch (patternSyntax)
            {
                case PatternSyntax.ECMAScript:
                    {
                        if (ignoreCase)
                        {
                            var regex = new Regex(input, RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexTimeout);
                            _initialMatch = new RegexMatch(regex, negate);
                        }
                        else
                        {
                            var regex = new Regex(input, RegexOptions.CultureInvariant | RegexOptions.Compiled, RegexTimeout);
                            _initialMatch = new RegexMatch(regex, negate);
                        }
                        break;
                    }
                case PatternSyntax.Wildcard:
                    throw new NotSupportedException("Wildcard syntax is not supported");
                case PatternSyntax.ExactMatch:
                    _initialMatch = new ExactMatch(ignoreCase, input, negate);
                    break;
            }
        }

        public void AddUrlCondition(Condition condition, bool trackAllCaptures)
        {
            // If there are no conditions specified
            if (_conditions == null)
            {
                AddUrlConditions(LogicalGrouping.MatchAll, trackAllCaptures);
            }
			_conditions.Add(condition);
        }

        public void AddUrlConditions(LogicalGrouping logicalGrouping, bool trackAllCaptures)
        {
            _conditions = new ConditionCollection(logicalGrouping == LogicalGrouping.MatchAny ? ConditionCollection.ConditionGrouping.Or : ConditionCollection.ConditionGrouping.And, trackAllCaptures);
        }
    }
}