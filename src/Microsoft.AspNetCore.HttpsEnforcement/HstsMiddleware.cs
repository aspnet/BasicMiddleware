// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.HttpsEnforcement
{
    /// <summary>
    /// Enables Http Strict Transport Security (HSTS)
    /// See https://tools.ietf.org/html/rfc6797.
    /// </summary>
    public class HstsMiddleware
    {
        private const string IncludeSubDomains = "; includeSubDomains";
        private const string Preload = "; preload";

        private readonly RequestDelegate _next;
        private readonly StringValues _nameValueHeaderValue;

        /// <summary>
        /// Initialize the HSTS middleware.
        /// </summary>
        /// <param name="next"></param>
        public HstsMiddleware(RequestDelegate next, IOptions<HstsOptions> options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _next = next;

            var hstsOptions = options.Value;
            var includeSubdomains = hstsOptions.IncludeSubDomains ? IncludeSubDomains : StringSegment.Empty;
            var preload = hstsOptions.Preload ? Preload : StringSegment.Empty;

            _nameValueHeaderValue = new StringValues($"max-age={hstsOptions.MaxAge}{includeSubdomains}{preload}");
        }

        /// <summary>
        /// Invoke the middleware.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                if (context.Request.Scheme == "https")
                {
                    context.Response.Headers[HeaderNames.StrictTransportSecurity] = _nameValueHeaderValue;
                }
                return Task.CompletedTask;
            });
            await _next(context);
        }
    }
}
