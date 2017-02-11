// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite.Logging;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite
{
    public class IISUrlRewriteRule : IRule
    {
        public string Name { get; }
        public UrlMatch InitialMatch { get; }
        public IList<Condition> Conditions { get; }
        public UrlAction Action { get; }
        public bool TrackAllCaptures { get; }
        public bool Global { get; }

        public IISUrlRewriteRule(string name,
            UrlMatch initialMatch,
            IList<Condition> conditions,
            UrlAction action,
            bool trackAllCaptures)
            : this(name, initialMatch, conditions, action, trackAllCaptures, false)
        {
        }

        public IISUrlRewriteRule(string name,
            UrlMatch initialMatch,
            IList<Condition> conditions,
            UrlAction action,
            bool trackAllCaptures,
            bool global)
        {
            Name = name;
            InitialMatch = initialMatch;
            Conditions = conditions;
            Action = action;
            TrackAllCaptures = trackAllCaptures;
            Global = global;
        }

        public virtual Task ApplyRuleAsync(RewriteContext context)
        {
            // Due to the path string always having a leading slash,
            // remove it from the path before regex comparison
            var path = context.HttpContext.Request.Path;
            MatchResults initMatchResults;
            if (path == PathString.Empty)
            {
                initMatchResults = InitialMatch.Evaluate(path.ToString(), context);
            }
            else
            {
                initMatchResults = InitialMatch.Evaluate(path.ToString().Substring(1), context);
            }

            if (!initMatchResults.Success)
            {
                context.Logger?.UrlRewriteDidNotMatchRule(Name);
                return TaskCache.CompletedTask;
            }

            MatchResults condResult = null;
            if (Conditions != null)
            {
                condResult = ConditionHelper.Evaluate(Conditions, context, initMatchResults.BackReferences, TrackAllCaptures);
                if (!condResult.Success)
                {
                    context.Logger?.UrlRewriteDidNotMatchRule(Name);
                    return TaskCache.CompletedTask;
                }
            }

            context.Logger?.UrlRewriteMatchedRule(Name);
            // at this point we know the rule passed, evaluate the replacement.
            return Action.ApplyActionAsync(context, initMatchResults?.BackReferences, condResult?.BackReferences);
        }
    }
}