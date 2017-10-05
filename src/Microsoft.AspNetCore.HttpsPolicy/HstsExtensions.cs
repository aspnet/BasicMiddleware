// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.HttpsPolicy
{
    /// <summary>
    /// Extension methods for the HSTS middleware.
    /// </summary>
    public static class HstsExtensions
    {
        /// <summary>
        /// Adds middleware for using HSTS, which adds the Strict-Transport-Security header.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance this method extends.</param>
        public static IApplicationBuilder UseHsts(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseHsts(new HstsOptions());
        }

        /// <summary>
        /// Adds middleware for add HSTS, which adds the Strict-Transport-Security header.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance this method extends.</param>
        /// <param name="options">The <see cref="HstsOptions"/> for specifying the behavior of the middleware.</param>
        public static IApplicationBuilder UseHsts(this IApplicationBuilder app, HstsOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<HstsMiddleware>(Options.Create(options));
        }
    }
}
