// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Rewrite.Internal;

namespace Microsoft.AspNetCore.Rewrite.Internal.UrlRewrite.UrlActions
{
    public class RewriteAction : UrlAction
    {
        public RuleTerminiation Result { get; }

        public RewriteAction(RuleTerminiation result, Pattern pattern)
        {
            Result = result;
            Url = pattern;
        }

        public override RuleResult ApplyAction(HttpContext context, MatchResults ruleMatch, MatchResults condMatch)
        {
            var pattern = Url.Evaluate(context, ruleMatch, condMatch);
            
            if (pattern.IndexOf("://") >= 0)
            {
                string scheme = null;
                var host = new HostString();
                var path = new PathString();
                var query = new QueryString();
                var fragment = new FragmentString();
                UriHelper.FromAbsolute(pattern, out scheme, out host, out path, out query, out fragment);

                // TODO How to split among path and pathbase: globalrules vs rules
                if (!path.StartsWithSegments(context.Request.PathBase))
                {
                    // TODO throw here?
                }
                context.Request.Scheme = scheme;
                context.Request.Host = host;
                context.Request.Path = path;
                context.Request.QueryString = query.Add(context.Request.QueryString);
            }
            else
            {
                // TODO PERF
                var split = pattern.IndexOf('?');
                if (split >= 0)
                {
                    context.Request.Path = new PathString("/" + pattern.Substring(0, split));
                    context.Request.QueryString = context.Request.QueryString.Add(new QueryString(pattern.Substring(split)));
                }
                else
                {
                    context.Request.Path = new PathString("/" + pattern);
                }
            }
            return new RuleResult { Result = Result };
        }
    }
}
