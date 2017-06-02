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
        /// <summary>
        /// The Port that the incoming request will be redirected to.
        /// </summary>
        public int Port { get; set; } = 443;

        /// <summary>
        /// The status code for the redirected request.
        /// </summary>
        public int StatusCode { get; set; } = StatusCodes.Status302Found;
    }
}
