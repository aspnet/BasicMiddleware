// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.ResponseCompression
{
    public class ResponseCompressionOptions
    {
        public IEnumerable<string> MimeTypes { get; set; }

        public int MinimumSize { get; set; }

        /// <summary>
        /// The compression providers.
        /// </summary>
        public IEnumerable<IResponseCompressionProvider> Providers { get; set; }
    }
}
