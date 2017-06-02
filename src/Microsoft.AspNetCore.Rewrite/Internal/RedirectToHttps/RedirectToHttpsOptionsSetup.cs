// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options.Infrastructure;

namespace Microsoft.AspNetCore.Rewrite
{
    public class RedirectToHttpsOptionsSetup : ConfigureDefaultOptions<RedirectToHttpsOptions>
    {
        public RedirectToHttpsOptionsSetup(IConfiguration configuration)
            : base(options => configuration.GetSection("Microsoft:AspNetCore:RedirectToHttps").Bind(options))
        {
        }
    }
}
