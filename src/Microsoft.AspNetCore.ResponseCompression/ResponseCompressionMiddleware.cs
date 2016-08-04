// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.ResponseCompression
{
    public class ResponseCompressionMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IResponseCompressionProvider[] _compressionProviders;

        private readonly HashSet<string> _mimeTypes;

        private readonly int _minimumSize;

        /// <summary>
        /// Initialize the Response Compression middleware.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        public ResponseCompressionMiddleware(RequestDelegate next, IOptions<ResponseCompressionOptions> options)
        {
            if (options.Value.MinimumSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.Value.MinimumSize));
            }
            _minimumSize = options.Value.MinimumSize;

            if (options.Value.MimeTypes == null)
            {
                throw new ArgumentNullException(nameof(options.Value.MimeTypes));
            }
            _mimeTypes = new HashSet<string>(options.Value.MimeTypes);

            _next = next;

            var providers = options.Value.Providers;
            if (providers == null)
            {
                providers = new IResponseCompressionProvider[]
                {
                    new GzipResponseCompressionProvider(CompressionLevel.Fastest)
                };
            }
            _compressionProviders = providers.ToArray();
        }

        /// <summary>
        /// Invoke the middleware.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            IResponseCompressionProvider compressionProvider = SelectProvider(context.Request.Headers[HeaderNames.AcceptEncoding]);

            if (compressionProvider == null)
            {
                await _next(context);
                return;
            }

            var bodyStream = context.Response.Body;

            using (var uncompressedStream = new MemoryStream())
            {
                context.Response.Body = uncompressedStream;

                try
                {
                    await _next(context);
                }
                finally
                {
                    context.Response.Body = bodyStream;
                }

                uncompressedStream.Seek(0, SeekOrigin.Begin);

                if (uncompressedStream.Length < _minimumSize ||                 // The response is too small
                    context.Response.Headers[HeaderNames.ContentRange] != StringValues.Empty ||     // The response is partial
                    context.Response.Headers[HeaderNames.ContentEncoding] != StringValues.Empty ||    // Already a specific encoding
                    !IsMimeTypeCompressable(context.Response.ContentType))      // MIME type not in the authorized list
                {
                    await uncompressedStream.CopyToAsync(bodyStream);
                }
                else
                {
                    using (var compressedStream = new MemoryStream())
                    {
                        await compressionProvider.CompressAsync(uncompressedStream, compressedStream);

                        if (compressedStream.Length >= uncompressedStream.Length)
                        {
                            uncompressedStream.Seek(0, SeekOrigin.Begin);
                            await uncompressedStream.CopyToAsync(bodyStream);
                        }
                        else
                        {
                            context.Response.Headers[HeaderNames.ContentEncoding] = compressionProvider.EncodingName;
                            context.Response.Headers[HeaderNames.ContentMD5] = StringValues.Empty;      // Reset the MD5 because the content changed.
                            context.Response.Headers[HeaderNames.ContentLength] = compressedStream.Length.ToString();
                            context.Response.ContentLength = compressedStream.Length;

                            compressedStream.Seek(0, SeekOrigin.Begin);
                            await compressedStream.CopyToAsync(bodyStream);
                        }
                    }
                }
            }
        }

        private IResponseCompressionProvider SelectProvider(StringValues encodings)
        {
            for (int i = 0; i < encodings.Count; i++)
            {
                var encoding = encodings[i];

                for (int j = 0; j < _compressionProviders.Length; j++)
                {
                    if (encoding.IndexOf(_compressionProviders[j].EncodingName) >= 0)
                    {
                        return _compressionProviders[j];
                    }
                }
            }

            return null;
        }

        private bool IsMimeTypeCompressable(string mimeType)
        {
            return !string.IsNullOrEmpty(mimeType) && _mimeTypes.Contains(mimeType);
        }
    }
}
