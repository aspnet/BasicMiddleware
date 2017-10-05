// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods for the HttpsPolicy middleware.
    /// </summary>
    public static class HttpsPolicyServiceCollectionExtensions
    {
        /// <summary>
        /// Add HTTPS policy services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> for adding services.</param>
        /// <returns></returns>
        public static IServiceCollection AddHttpsPolicy(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Configure<HttpsPolicyOptions>(options => { });
            return services;
        }

        /// <summary>
        /// Add HTTPS policy services and configure the related options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> for adding services.</param>
        /// <param name="configureOptions">A delegate to configure the <see cref="HttpsPolicyOptions"/>.</param>
        /// <returns></returns>
        public static IServiceCollection AddHttpsPolicy(this IServiceCollection services, Action<HttpsPolicyOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.Configure(configureOptions);
            return services;
        }
    }
}
