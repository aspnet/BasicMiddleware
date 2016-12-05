// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite.Logging;

namespace Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite
{
    public class IISUrlRewriteRule : IRule
    {
        public string Name { get; }
        public UrlMatch InitialMatch { get; }
        public IList<Condition> Conditions { get; }
        public IEnumerable<ServerVariable> ServerVariables { get; }
        public UrlAction Action { get; }

        public IISUrlRewriteRule(string name,
            UrlMatch initialMatch,
            IList<Condition> conditions,
            IEnumerable<ServerVariable> serverVariables,
            UrlAction action)
        {
            Name = name;
            InitialMatch = initialMatch;
            Conditions = conditions;
            ServerVariables = serverVariables;
            Action = action;
        }

        public virtual void ApplyRule(RewriteContext context)
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
                return;
            }

            MatchResults condMatchRes = null;
            if (Conditions != null)
            {
                condMatchRes = ConditionHelper.Evaluate(Conditions, context, initMatchResults);
                if (!condMatchRes.Success)
                {
                    context.Logger?.UrlRewriteDidNotMatchRule(Name);
                    return;
                }
            }

            foreach (ServerVariable serverVariable in ServerVariables)
            {
                if (serverVariable.Name.StartsWith(ServerVariableConstants.RequestHeaderPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    context.HttpContext.Request.Headers.Append(
                        serverVariable.Name.Substring(ServerVariableConstants.RequestHeaderPrefix.Length).Replace('_', '-'),
                        serverVariable.Evaluate(context, initMatchResults, condMatchRes));
                }
                else if (serverVariable.Name.StartsWith(ServerVariableConstants.ResponseHeaderPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    context.HttpContext.Response.Headers.Append(
                        serverVariable.Name.Substring(ServerVariableConstants.ResponseHeaderPrefix.Length).Replace('_', '-'),
                        serverVariable.Evaluate(context, initMatchResults, condMatchRes));
                }
            }

            context.Logger?.UrlRewriteMatchedRule(Name);
            // at this point we know the rule passed, evaluate the replacement.
            Action.ApplyAction(context, initMatchResults, condMatchRes);
        }
    }
}