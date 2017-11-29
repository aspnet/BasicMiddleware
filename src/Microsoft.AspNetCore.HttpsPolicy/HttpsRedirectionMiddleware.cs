// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
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
        public HttpsRedirectionMiddleware(RequestDelegate next, IOptions<HttpsRedirectionOptions> options, IServerAddressesFeature serverAddressesFeature)
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

        public Task Invoke(HttpContext context)
        {
            if (!context.Request.IsHttps)
            {
                if (!_evaluatedServerAddressesFeature)
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
                var newUrl = UriHelper.BuildAbsolute("https", 
                                                        host,
                                                        request.PathBase,
                                                        request.Path,
                                                        request.QueryString);

                context.Response.StatusCode = _statusCode;
                context.Response.Headers[HeaderNames.Location] = newUrl;
                return Task.CompletedTask;
            }

            return _next(context);
        }

        private void CheckAddressesFeatureForHttpsPorts()
        {
            if (_serverAddressesFeature == null || _serverAddressesFeature.Addresses == null)
            {
                return;
            }
            foreach (var address in _serverAddressesFeature.Addresses)
            {
                if (Uri.TryCreate(address, UriKind.Absolute, out var uri))
                {
                    if (uri.Scheme == "https")
                    {
                        _httpsPort = uri.Port;
                        return;
                    }
                }
            }
        }
    }
}
