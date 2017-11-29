// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Hosting.Server.Features;
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

            // Order for finding the HTTPS port:
            // 1. Set in the HttpsRedirectionOptions
            // 2. HTTPS_PORT environment variable
            // 3. IServerAddressesFeature
            // 4. 443 (or not set.)


            var httpsPort = 0;
            if (options.HttpsPort == null)
            {
                if (TryGetHttpsPortFromEnvironmentVariable(app, out httpsPort)
                    || TryGetHttpsPortFromIServerAddressesFeature(app, out httpsPort))
                {
                    options.HttpsPort = httpsPort;
                }
            }

            app.UseMiddleware<HttpsRedirectionMiddleware>();

            return app;
        }

        private static bool TryGetHttpsPortFromEnvironmentVariable(IApplicationBuilder app, out int httpsPort)
        {
            httpsPort = 0;
            var config = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var configHttpsPort = config["HTTPS_PORT"];
            // If the string isn't empty, try to parse it.
            if (!string.IsNullOrEmpty(configHttpsPort))
            {
                return int.TryParse(configHttpsPort, out httpsPort);
            }
            return false;
        }

        private static bool TryGetHttpsPortFromIServerAddressesFeature(IApplicationBuilder app, out int httpsPort)
        {
            httpsPort = 0;
            var addressFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
            foreach (var address in addressFeature.Addresses)
            {
            }
            return false;
        }
    }
}
