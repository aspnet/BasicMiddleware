// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite.Internal;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Rewrite.Internal.UrlRewrite.UrlActions
{
    public class RedirectAction : UrlAction
    {
        public int StatusCode { get; }
        public RedirectAction(int statusCode, Pattern pattern)
        {
            StatusCode = statusCode;
            Url = pattern;
        }

        public override RuleResult ApplyAction(HttpContext context, MatchResults ruleMatch, MatchResults condMatch)
        {
            context.Response.StatusCode = StatusCode;
            var pattern = Url.Evaluate(context, ruleMatch, condMatch);

            // url can either contain the full url or the path and query
            // always add to location header.
            // TODO check for false positives
            var split = pattern.IndexOf('?');
            if (split >= 0)
            {
                context.Request.QueryString = context.Request.QueryString.Add(new QueryString(pattern.Substring(split)));
                // not using the response.redirect here because status codes may be 301, 302, 307, 308 
                context.Response.Headers[HeaderNames.Location] = pattern.Substring(0, split) + context.Request.QueryString;
            }
            else
            {
                context.Response.Headers[HeaderNames.Location] = pattern;
            }
            return new RuleResult { Result = RuleTerminiation.ResponseComplete };
        }
    }
}
