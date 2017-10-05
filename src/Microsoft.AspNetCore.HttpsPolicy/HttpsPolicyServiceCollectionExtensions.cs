// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.HttpsPolicy
{
    public static class HttpsPolicyServiceCollectionExtensions
    {
        public static HttpsPolicyBuilder AddHttpsPolicy(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Configure<HttpsPolicyOptions>(options => { });
            return new HttpsPolicyBuilder(services);
        }

        public static HttpsPolicyBuilder AddHttpsPolicy(this IServiceCollection services, Action<HttpsPolicyOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var builder = services.AddHttpsPolicy();
            services.Configure(configureOptions);
            return builder;
        }
    }
}
