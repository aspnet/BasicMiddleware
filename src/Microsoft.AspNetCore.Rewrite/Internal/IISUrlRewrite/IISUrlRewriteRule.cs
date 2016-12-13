﻿// Copyright (c) .NET Foundation. All rights reserved.
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

            if (ServerVariables != null)
            {
                foreach (ServerVariable serverVariable in ServerVariables)
                {
                    var name = serverVariable.Name;
                    var value = serverVariable.Evaluate(context, initMatchResults, condMatchRes);

                    IHeaderDictionary headerDictionary;
                    switch (serverVariable.Type)
                    {
                        case ServerVariableType.RequestHeader:
                            headerDictionary = context.HttpContext.Request.Headers;
                            context.Logger?.RequestHeaderAdded(name, value);
                            break;
                        case ServerVariableType.ResponseHeader:
                            headerDictionary = context.HttpContext.Response.Headers;
                            context.Logger?.ResponseHeaderAdded(name, value);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"Unexpected server variable: `{serverVariable.Type}`");
                    }

                    headerDictionary.Append(name, value);
                }
            }

            context.Logger?.UrlRewriteMatchedRule(Name);
            // at this point we know the rule passed, evaluate the replacement.
            Action.ApplyAction(context, initMatchResults, condMatchRes);
        }
    }
}