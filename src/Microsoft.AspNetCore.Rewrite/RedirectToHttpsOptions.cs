// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Rewrite
{
    /// <summary>
    /// Options for the <see cref="RedirectToHttpsMiddleware"/>
    /// </summary>
    public class RedirectToHttpsOptions
    {
        public int Port { get; set; } = 443;

        public int StatusCode { get; set; } = StatusCodes.Status302Found;
    }
}
