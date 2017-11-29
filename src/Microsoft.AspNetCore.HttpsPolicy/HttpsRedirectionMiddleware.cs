// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.HttpsPolicy
{
    public class HttpsRedirectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly int? _httpsPort;
        private readonly int _statusCode;
        public HttpsRedirectionMiddleware(RequestDelegate next, IOptions<HttpsRedirectionOptions> options)
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

            var httpsRedirectionOptions = options.Value;

            // The tls port set in options will have priority over the one in configuration.
            var httpsPort = httpsRedirectionOptions.HttpsPort;
            if (httpsPort == null)
            {
                // Only read configuration if there is no httpsPort
                var config = app.ApplicationServices.GetRequiredService<IConfiguration>();
                var configHttpsPort = config["HTTPS_PORT"];
                // If the string isn't empty, try to parse it.
                if (!string.IsNullOrEmpty(configHttpsPort)
                    && int.TryParse(configHttpsPort, out var intHttpsPort))
                {
                    httpsPort = intHttpsPort;
                }
            }

            _httpsPort = httpsRedirectionOptions.HttpsPort;
            _statusCode = httpsRedirectionOptions.RedirectStatusCode;
        }

        public Task Invoke(HttpContext context)
        {
            if (!context.Request.IsHttps)
            {
                var host = context.Request.Host;
                if (_httpsPort.HasValue && _httpsPort.Value > 0)
                {
                    host = new HostString(host.Host, _httpsPort.Value);
                }
                else
                {
                    host = new HostString(host.Host);
                }

                var request = context.Request;
                var newUrl = UriHelper.BuildAbsolute("https://", 
                                                        host,
                                                        request.PathBase,
                                                        request.Path,
                                                        request.QueryString);

                context.Response.StatusCode = _statusCode;
                context.Response.Headers[HeaderNames.Location] = newUrl.ToString();
            }

            return _next(context);
        }
    }
}
