// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite.RuleAbstraction;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite.UrlActions
{
    public class RewriteClearQueryAction : UrlAction
    {
        public RuleTerminiation Result { get; }

        public RewriteClearQueryAction(RuleTerminiation result, Pattern pattern)
        {
            Result = result;
            Url = pattern;
        }

        public override RuleResult ApplyAction(HttpContext context, MatchResults ruleMatch, MatchResults condMatch)
        {
            var pattern = Url.Evaluate(context, ruleMatch, condMatch, true);
            var split = pattern.IndexOf('?');
            context.Request.QueryString = new QueryString();

            if (split >= 0)
            {
                context.Request.Path = new PathString(pattern.Substring(0, split));
                context.Request.QueryString = context.Request.QueryString.Add(new QueryString(pattern.Substring(split)));
            }
            else 
            {
                context.Request.Path = new PathString(pattern);
            }
            return new RuleResult { Result = Result };
        }
    }
}
