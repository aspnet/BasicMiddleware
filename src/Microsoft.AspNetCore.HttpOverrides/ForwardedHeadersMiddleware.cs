// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.HttpOverrides
{
    public class ForwardedHeadersMiddleware
    {
        private readonly ForwardedHeadersOptions _options;
        private readonly RequestDelegate _next;
        private readonly ForwardedHeadersForwarder _forwardedHeadersForwarder;

        public ForwardedHeadersMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IOptions<ForwardedHeadersOptions> options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _options = options.Value;
            _next = next;
            _forwardedHeadersForwarder = new ForwardedHeadersForwarder(loggerFactory, options);
        }

        public Task Invoke(HttpContext context)
        {
            ApplyForwarders(context);
            return _next(context);
        }

		public void ApplyForwarders(HttpContext context)
        {
            // apply forwareded headers forwarder
            _forwardedHeadersForwarder.ApplyForwarders(context);

            // apply additional fowarders
            if (_options.AdditionalForwarders != null)
                foreach (var forwarder in _options.AdditionalForwarders)
                    forwarder.ApplyForwarders(context);
        }
    }
}
