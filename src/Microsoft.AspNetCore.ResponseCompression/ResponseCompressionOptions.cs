// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.ResponseCompression
{
    public class ResponseCompressionOptions
    {
        /// <summary>
        /// The accepted MIME types. Other MIME type bodies are not compressed.
        /// </summary>
        public IEnumerable<string> MimeTypes { get; set; }

        /// <summary>
        /// The compression providers. If 'null', the GZIP provider is set as default.
        /// </summary>
        public IEnumerable<IResponseCompressionProvider> Providers { get; set; }
    }
}
