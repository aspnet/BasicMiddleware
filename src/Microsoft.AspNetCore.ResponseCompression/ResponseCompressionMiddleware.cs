// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.ResponseCompression
{
    public class ResponseCompressionMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly Dictionary<string, IResponseCompressionProvider> _compressionProviders;

        private readonly HashSet<string> _mimeTypes;

        private readonly bool _enableHttps;

        /// <summary>
        /// Initialize the Response Compression middleware.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        public ResponseCompressionMiddleware(RequestDelegate next, IOptions<ResponseCompressionOptions> options)
        {
            if (options.Value.MimeTypes == null)
            {
                throw new ArgumentException($"{nameof(options.Value.MimeTypes)} is not provided in argument {nameof(options)}");
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
            else if (!providers.Any())
            {
                throw new ArgumentException($"{nameof(options.Value.Providers)} cannot be empty in argument {nameof(options)}");
            }

            _compressionProviders = providers.ToDictionary(p => p.EncodingName, StringComparer.OrdinalIgnoreCase);
            _compressionProviders.Add("*", providers.First());
            _compressionProviders.Add("identity", null);

            _enableHttps = options.Value.EnableHttps;
        }

        /// <summary>
        /// Invoke the middleware.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            IResponseCompressionProvider compressionProvider = null;

            if (!context.Request.IsHttps || _enableHttps)
            {
                compressionProvider = SelectProvider(context.Request.GetTypedHeaders());
            }

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

        private IResponseCompressionProvider SelectProvider(RequestHeaders headers)
        {
            var unsorted = headers.AcceptEncoding;

            if (unsorted != null)
            {
                var sorted = unsorted
                    .Where(s => s.Quality.GetValueOrDefault(1) > 0)
                    .OrderByDescending(s => s.Quality.GetValueOrDefault(1));

                foreach (var encoding in sorted)
                {
                    IResponseCompressionProvider provider;

                    if (_compressionProviders.TryGetValue(encoding.Value, out provider))
                    {
                        return provider;
                    }
                }
            }

            return null;
        }
    }
}
