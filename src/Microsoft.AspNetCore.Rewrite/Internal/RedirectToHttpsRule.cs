// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite.Logging;
using Microsoft.Extensions.Internal;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Rewrite.Internal
{
    public class RedirectToHttpsRule : IRule
    {
        public int? SSLPort { get; set; }
        public int StatusCode { get; set; }

        public virtual Task ApplyRuleAsync(RewriteContext context)
        {
            if (!context.HttpContext.Request.IsHttps)
            {
                var host = context.HttpContext.Request.Host;
                if (SSLPort.HasValue && SSLPort.Value > 0)
                {
                    // a specific SSL port is specified
                    host = new HostString(host.Host, SSLPort.Value);
                }
                else
                {
                    // clear the port
                    host = new HostString(host.Host);
                }

                var req = context.HttpContext.Request;
                var newUrl = new StringBuilder().Append("https://").Append(host).Append(req.PathBase).Append(req.Path).Append(req.QueryString);
                var response = context.HttpContext.Response;
                response.StatusCode = StatusCode;
                response.Headers[HeaderNames.Location] = newUrl.ToString();
                context.Result = RuleResult.EndResponse;
                context.Logger?.RedirectedToHttps();
            }
            return TaskCache.CompletedTask;
        }
    }
}