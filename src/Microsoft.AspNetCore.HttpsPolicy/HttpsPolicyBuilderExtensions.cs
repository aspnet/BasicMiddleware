// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        /// <remarks>
        /// HTTPS Enforcement interanlly uses the UrlRewrite middleware to redirect HTTP requests to HTTPS
        /// </remarks>
        public static IApplicationBuilder UseHttpsPolicy(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            var options = app.ApplicationServices.GetRequiredService<IOptions<HttpsPolicyOptions>>().Value;

            if (options.SetHsts)
            {
                app.UseHsts();
            }

            var rewriteOptions = new RewriteOptions();
            rewriteOptions.AddRedirectToHttps(
                options.StatusCode,
                options.TlsPort);

            app.UseRewriter(rewriteOptions);

            return app;
        }

        public static IApplicationBuilder UseHttpsPolicy(this IApplicationBuilder app, bool enableHsts)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            var options = app.ApplicationServices.GetRequiredService<IOptions<HttpsPolicyOptions>>().Value;

            if (enableHsts)
            {
                app.UseHsts();
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
