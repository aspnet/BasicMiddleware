// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.ResponseCompression
{
    public class GzipResponseCompressionProvider : IResponseCompressionProvider
    {
        private readonly CompressionLevel _level;

        public GzipResponseCompressionProvider(CompressionLevel level)
        {
            _level = level;
        }

        public string EncodingName
        {
            get
            {
                return "gzip";
            }
        }

        public async Task CompressAsync(Stream input, Stream output)
        {
            using (var deflate = new GZipStream(output, _level, true))
            {
                await input.CopyToAsync(deflate);
            }
        }
    }
}
