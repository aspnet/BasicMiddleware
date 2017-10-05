// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.HttpsPolicy
{
    public class HttpsPolicyBuilder
    {
        public HttpsPolicyBuilder(IServiceCollection services)
            => Services = services;
        public virtual IServiceCollection Services { get; }

    }
}