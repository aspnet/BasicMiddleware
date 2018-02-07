// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.AspNetCore.Rewrite.Logging;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Rewrite.Internal
{
    public class RedirectToWwwRule : IRule
    {
        public int StatusCode { get; set; }
        public bool AllowLocalhost { get; set; } = true;

        public virtual void ApplyRule(RewriteContext context)
        {
            var req = context.HttpContext.Request;
            if (AllowLocalhost && req.Host.Host.StartsWith("localhost", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = RuleResult.ContinueRules;
                return;
            }

            if (req.Host.Value.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = RuleResult.ContinueRules;
                return;
            }

            var newUrl = new StringBuilder().Append(req.Scheme).Append("://www.").Append(req.Host).Append(req.PathBase).Append(req.Path).Append(req.QueryString);
            var response = context.HttpContext.Response;
            response.StatusCode = StatusCode;
            response.Headers[HeaderNames.Location] = newUrl.ToString();
            context.Result = RuleResult.EndResponse;
            context.Logger?.RedirectedToWww();
        }
    }
}