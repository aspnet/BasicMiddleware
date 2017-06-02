// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options.Infrastructure;

namespace Microsoft.AspNetCore.Rewrite
{
    public static class RedirectToHttpsServiceCollectionExtensions
    {
        public static IServiceCollection AddRedirectToHttps(this IServiceCollection services)
        {
            services.TryAddSingleton<RedirectToHttpsMarkerService>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ConfigureDefaultOptions<RedirectToHttpsOptions>,RedirectToHttpsOptionsSetup>());
            return services;
        }
    }
}
