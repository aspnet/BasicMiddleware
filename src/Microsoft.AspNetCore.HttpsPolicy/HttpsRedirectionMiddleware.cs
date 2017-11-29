// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.HttpsPolicy
{
    public class HttpsRedirectionMiddleware
    {
        private readonly RequestDelegate _next;
        private int? _httpsPort;
        private readonly int _statusCode;

        private readonly IServerAddressesFeature _serverAddressesFeature;
        private bool _evaluatedServerAddressesFeature;

        /// <summary>
        /// Initializes the HttpsRedirectionMiddleware
        /// </summary>
        /// <param name="next"></param>
        /// <param name="serverAddressesFeature">The</param>
        /// <param name="options"></param>
        public HttpsRedirectionMiddleware(RequestDelegate next, IServerAddressesFeature serverAddressesFeature, IOptions<HttpsRedirectionOptions> options)
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
            _httpsPort = httpsRedirectionOptions.HttpsPort;
            _statusCode = httpsRedirectionOptions.RedirectStatusCode;
            _serverAddressesFeature = serverAddressesFeature;
        }

        /// <summary>
        /// Invokes the HttpsRedirectionMiddleware
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Invoke(HttpContext context)
        {
            if (!context.Request.IsHttps)
            {
                if (!_evaluatedServerAddressesFeature && _httpsPort == null)
                {
                    CheckAddressesFeatureForHttpsPorts();
                }
                _evaluatedServerAddressesFeature = true;

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
                var redirectUrl = UriHelper.BuildAbsolute("https", 
                                                        host,
                                                        request.PathBase,
                                                        request.Path,
                                                        request.QueryString);

                context.Response.StatusCode = _statusCode;
                context.Response.Headers[HeaderNames.Location] = redirectUrl;
                return Task.CompletedTask;
            }

            return _next(context);
        }

        private void CheckAddressesFeatureForHttpsPorts()
        {
            // The IServerAddressesFeature will not be ready until the middleware is Invoked,
            int? httpsPort = null;
            foreach (var address in _serverAddressesFeature.Addresses)
            {
                if (Uri.TryCreate(address, UriKind.Absolute, out var uri))
                {
                    if (uri.Scheme == "https")
                    {
                        // If we find multiple https ports specified, throw
                        if (httpsPort != null)
                        {
                            throw new ArgumentException($"Cannot specify multiple https ports in IServerAddressesFeature. " +
                                $"Conflict found with ports: {httpsPort.Value}, {uri.Port}.");
                        }
                        else
                        {
                            httpsPort = uri.Port;
                        }
                    }
                }
            }
            _httpsPort = httpsPort;
        }
    }
}
