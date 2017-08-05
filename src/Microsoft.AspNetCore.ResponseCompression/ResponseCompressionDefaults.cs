// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.ResponseCompression
{
    /// <summary>
    /// Defaults for the ResponseCompressionMiddleware
    /// </summary>
    public class ResponseCompressionDefaults
    {
        /// <summary>
        /// Default MIME types to compress responses for.
        /// </summary>
        // This list is not intended to be exhaustive, it's a baseline for the 90% case.
        public static readonly IEnumerable<string> MimeTypes = new[]
        {
            // General
            "text/plain",
            "text/css",
            "application/javascript",
            "text/html",
            "application/xml",
            "text/xml",
            "application/rss+xml",
            "application/atom+xml",
            "application/json",
            "text/json",
            // Images
            "image/svg+xml",
            "image/x-icon",
            // Fonts
            "application/vnd.ms-fontobject",
            "application/x-font-ttf",
            "font/otf"
        };
    }
}
