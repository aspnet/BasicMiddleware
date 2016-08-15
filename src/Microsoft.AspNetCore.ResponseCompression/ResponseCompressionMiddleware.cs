// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Initialize the Response Compression middleware.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        public ResponseCompressionMiddleware(RequestDelegate next, IOptions<ResponseCompressionOptions> options)
        {
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

            using (var bodyWrapperStream = new BodyWrapperStream(context.Response, bodyStream, _mimeTypes, compressionProvider))
            {
                context.Response.Body = bodyWrapperStream;

                try
                {
                    await _next(context);
                }
                finally
                {
                    context.Response.Body = bodyStream;
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
    }
}
