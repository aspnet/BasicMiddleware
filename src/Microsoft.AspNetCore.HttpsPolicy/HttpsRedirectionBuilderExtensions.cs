// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods for the HttpsRedirection middleware.
    /// </summary>
    public static class HttpsPolicyBuilderExtensions
    {
        /// <summary>
        /// Adds middleware for redirecting HTTP Requests to HTTPS.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance this method extends.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> for HttpsRedirection.</returns>
        /// <remarks>
        /// HTTPS Enforcement interanlly uses the UrlRewrite middleware to redirect HTTP requests to HTTPS.
        /// </remarks>
        public static IApplicationBuilder UseHttpsRedirection(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = app.ApplicationServices.GetRequiredService<IOptions<HttpsRedirectionOptions>>().Value;

            // The tls port set in options will have priority over the one in configuration.
            var tlsPort = options.SSLPort;
            if (tlsPort == null)
            {
                // Only read configuration if there is no tlsPort
                var config = app.ApplicationServices.GetRequiredService<IConfiguration>();
                var configTlsPort = config["SSL_PORT"];
                // If the string isn't empty, try to parse it.
                if (!string.IsNullOrEmpty(configTlsPort)
                    && int.TryParse(configTlsPort, out var intTlsPort))
                {
                    tlsPort = intTlsPort;
                }
            }

            var rewriteOptions = new RewriteOptions();
            rewriteOptions.AddRedirectToHttps(
                options.RedirectStatusCode,
                tlsPort);

            app.UseRewriter(rewriteOptions);

            return app;
        }
    }
}
