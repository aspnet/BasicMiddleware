// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.HttpsPolicy
{
    /// <summary>
    /// Options for the HttpsPolicyMiddleware middleware
    /// </summary>
    public class HttpsPolicyOptions
    {
        /// <summary>
        /// Whether to use HTTP Strict-Transport-Security (HSTS) on all HTTPS responses.
        /// </summary>
        public bool SetHsts { get; set; }

        /// <summary>
        /// The status code to redirect the response to.
        /// </summary>
        public int RedirectStatusCode { get; set; } = StatusCodes.Status301MovedPermanently;

        /// <summary>
        /// The TLS port to be added to the redirected URL.
        /// </summary>
        /// <remarks>
        /// Defaults to 443 if not provided.
        /// </remarks>
        public int? TlsPort { get; set; }
    }
}
