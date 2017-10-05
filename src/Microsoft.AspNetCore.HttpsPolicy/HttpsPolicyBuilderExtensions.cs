// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Rewrite;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods for the HttpsPolicy middleware.
    /// </summary>
    public static class HttpsPolicyBuilderExtensions
    {
        /// <summary>
        /// Adds middleware for enforcing HTTPS for all HTTP Requests, including redirecting HTTP to HTTPS
        /// and adding the HTTP Strict-Transport-Header.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance this method extends.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> with HttpsPolicy.</returns>
        public static IApplicationBuilder UseHttpsPolicy(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            var httpsEnforcementOptions = new HttpsPolicyOptions();

            return app.UseHttpsPolicy(httpsEnforcementOptions);
        }

        /// <summary>
        /// Adds middleware for enforcing HTTPS for all HTTP Requests, including redirecting HTTP to HTTPS
        /// and adding the HTTP Strict-Transport-Header.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance this method extends.</param>
        /// <param name="options">The <see cref="HttpsPolicyOptions"/> for specifying the behavior of the middleware.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> with HttpsPolicy.</returns>
        /// <remarks>
        /// HTTPS Enforcement interanlly uses the UrlRewrite middleware to redirect HTTP requests to HTTPS
        /// </remarks>
        public static IApplicationBuilder UseHttpsPolicy(this IApplicationBuilder app, HttpsPolicyOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.SetHsts)
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
