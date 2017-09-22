// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;

namespace Microsoft.AspNetCore.HttpsEnforcement
{
    /// <summary>
    /// Extension methods for the HttpsEnforcement middleware.
    /// </summary>
    public static class HttpsEnforcementExtensions
    {
        /// <summary>
        /// Adds middleware for enforcing HTTPS for all HttpRequests.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance this method extends.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> with HttpsEnforcement.</returns>
        public static IApplicationBuilder UseHttpsEnforcement(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            var httpsEnforcementOptions = new HttpsEnforcementOptions();

            return app.UseHttpsEnforcement(httpsEnforcementOptions);
        }

        /// <summary>
        /// Adds middleware for enforcing HTTPS for all HttpRequests.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance this method extends.</param>
        /// <param name="options">The <see cref="HttpsEnforcementOptions"/> for specifying the behavior of the middleware.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> with HttpsEnforcement.</returns>
        /// <remarks>
        /// HTTPS Enforcement interanlly uses the UrlRewrite middleware to redirect HTTP requests to HTTPS
        /// </remarks>
        public static IApplicationBuilder UseHttpsEnforcement(this IApplicationBuilder app, HttpsEnforcementOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.EnforceHsts)
            {
                app.UseHsts(options.HstsOptions);
            }

            var rewriteOptions = new RewriteOptions();
            rewriteOptions.AddRedirectToHttps(
                options.StatusCode,
                options.TlsPort);

            app.UseRewriter(rewriteOptions);

            return app;
        }
    }
}
