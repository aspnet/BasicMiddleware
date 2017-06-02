// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods for the <see cref="RedirectToHttpsMiddleware"/>
    /// </summary>
    public static class RedirectToHttpsExtensions
    {
        /// <summary>
        /// Ensures the scheme is Https
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRedirectToHttps(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var marker = app.ApplicationServices.GetService<RedirectToHttpsMarkerService>();
            if (marker == null)
            {
                throw new InvalidOperationException("The RedirectToHttps service needs to be registered");
            }

            return app.UseMiddleware<RedirectToHttpsMiddleware>();
        }

        /// <summary>
        /// Ensures the scheme is Https
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/></param>
        /// <param name="options">Options for the redirect to Https.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseRedirectToHttps(this IApplicationBuilder app, RedirectToHttpsOptions options)
        {
            if(app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var marker = app.ApplicationServices.GetService<RedirectToHttpsMarkerService>();
            if (marker == null)
            {
                throw new InvalidOperationException("The RedirectToHttps service needs to be registered");
            }

            return app.UseMiddleware<RedirectToHttpsMiddleware>(options);
        }
    }
}
