﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.HttpsPolicy
{
    /// <summary>
    /// Enables HTTP Strict Transport Security (HSTS)
    /// See https://tools.ietf.org/html/rfc6797.
    /// </summary>
    public class HstsMiddleware
    {
        private const string IncludeSubDomains = "; includeSubDomains";
        private const string Preload = "; preload";
        private static readonly string[] LocalhostStrings =
        {
            "localhost",
            "127.0.0.1", // ipv4
            "[::1]" // ipv6
        };

        private readonly RequestDelegate _next;
        private readonly StringValues _strictTransportSecurityValue;
        private readonly List<string> _excludedDomains;
        /// <summary>
        /// Initialize the HSTS middleware.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
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
            var maxAge = Convert.ToInt64(Math.Floor(hstsOptions.MaxAge.TotalSeconds))
                            .ToString(CultureInfo.InvariantCulture);
            var includeSubdomains = hstsOptions.IncludeSubDomains ? IncludeSubDomains : StringSegment.Empty;
            var preload = hstsOptions.Preload ? Preload : StringSegment.Empty;
            _strictTransportSecurityValue = new StringValues($"max-age={maxAge}{includeSubdomains}{preload}");

            _excludedDomains = hstsOptions.AddHstsHeaderToLocahostRequests ? new List<string>() : new List<string>(LocalhostStrings);
            _excludedDomains.AddRange(hstsOptions.ExcludedDomains);
        }

        /// <summary>
        /// Invoke the middleware.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <returns></returns>
        public Task Invoke(HttpContext context)
        {
            if (context.Request.IsHttps)
            {
                if (!IsDomainBlocked(context.Request.Host.Host))
                {
                    context.Response.Headers[HeaderNames.StrictTransportSecurity] = _strictTransportSecurityValue;
                }
            }

            return  _next(context);
        }

        private bool IsDomainBlocked(string host)
        {
            for (var i = 0; i < _excludedDomains.Count; i++)
            {
                if (host.Equals(_excludedDomains[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
