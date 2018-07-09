﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.ResponseCompression
{
    /// <inheritdoc />
    public class ResponseCompressionProvider : IResponseCompressionProvider
    {
        private readonly ICompressionProvider[] _providers;
        private readonly HashSet<string> _mimeTypes;
        private readonly HashSet<string> _excludedMimeTypes;
        private readonly bool _enableForHttps;

        /// <summary>
        /// If no compression providers are specified then GZip is used by default.
        /// </summary>
        /// <param name="services">Services to use when instantiating compression providers.</param>
        /// <param name="options"></param>
        public ResponseCompressionProvider(IServiceProvider services, IOptions<ResponseCompressionOptions> options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var responseCompressionOptions = options.Value;

            _providers = responseCompressionOptions.Providers.ToArray();
            if (_providers.Length == 0)
            {
                // Use the factory so it can resolve IOptions<GzipCompressionProviderOptions> from DI.
                _providers = new ICompressionProvider[] { new CompressionProviderFactory(typeof(GzipCompressionProvider)) };
            }
            for (var i = 0; i < _providers.Length; i++)
            {
                var factory = _providers[i] as CompressionProviderFactory;
                if (factory != null)
                {
                    _providers[i] = factory.CreateInstance(services);
                }
            }

            var mimeTypes = responseCompressionOptions.MimeTypes;
            if (mimeTypes == null || !mimeTypes.Any())
            {
                mimeTypes = ResponseCompressionDefaults.MimeTypes;
            }
            _mimeTypes = new HashSet<string>(mimeTypes, StringComparer.OrdinalIgnoreCase);

            _excludedMimeTypes = new HashSet<string>(
                responseCompressionOptions.ExcludedMimeTypes ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase
            );

            _enableForHttps = responseCompressionOptions.EnableForHttps;
        }

        /// <inheritdoc />
        public virtual ICompressionProvider GetCompressionProvider(HttpContext context)
        {
            IList<StringWithQualityHeaderValue> unsorted;

            // e.g. Accept-Encoding: gzip, deflate, sdch
            var accept = context.Request.Headers[HeaderNames.AcceptEncoding];
            if (!StringValues.IsNullOrEmpty(accept)
                && StringWithQualityHeaderValue.TryParseList(accept, out unsorted)
                && unsorted != null && unsorted.Count > 0)
            {
                // TODO PERF: clients don't usually include quality values so this sort will not have any effect. Fast-path?
                var sorted = unsorted
                    .Where(s => s.Quality.GetValueOrDefault(1) > 0)
                    .OrderByDescending(s => s.Quality.GetValueOrDefault(1));

                foreach (var encoding in sorted)
                {
                    // There will rarely be more than three providers, and there's only one by default
                    foreach (var provider in _providers)
                    {
                        if (StringSegment.Equals(provider.EncodingName, encoding.Value, StringComparison.OrdinalIgnoreCase))
                        {
                            return provider;
                        }
                    }

                    // Uncommon but valid options
                    if (StringSegment.Equals("*", encoding.Value, StringComparison.Ordinal))
                    {
                        // Any
                        return _providers[0];
                    }
                    if (StringSegment.Equals("identity", encoding.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        // No compression
                        return null;
                    }
                }
            }

            return null;
        }

        /// <inheritdoc />
        public virtual bool ShouldCompressResponse(HttpContext context)
        {
            if (context.Response.Headers.ContainsKey(HeaderNames.ContentRange))
            {
                return false;
            }

            var mimeType = context.Response.ContentType;

            if (string.IsNullOrEmpty(mimeType))
            {
                return false;
            }

            var separator = mimeType.IndexOf(';');
            if (separator >= 0)
            {
                // Remove the content-type optional parameters
                mimeType = mimeType.Substring(0, separator);
                mimeType = mimeType.Trim();
            }

            return ShouldCompressExact(mimeType) //check exact match type/subtype
                ?? ShouldCompressPartial(mimeType) //check partial match type/*
                ?? _mimeTypes.Contains("*/*"); //check wildcard */*
        }

        /// <inheritdoc />
        public bool CheckRequestAcceptsCompression(HttpContext context)
        {
            if (context.Request.IsHttps && !_enableForHttps)
            {
                return false;
            }
            return !string.IsNullOrEmpty(context.Request.Headers[HeaderNames.AcceptEncoding]);
        }

        private bool? ShouldCompressExact(string mimeType)
        {
            //Check excluded MIME types first, then included
            if (_excludedMimeTypes.Contains(mimeType))
            {
                return false;
            }

            if (_mimeTypes.Contains(mimeType))
            {
                return true;
            }

            return null;
        }

        private bool? ShouldCompressPartial(string mimeType)
        {
            int? slashPos = mimeType?.IndexOf('/');

            if (slashPos >= 0)
            {
                string partialMimeType = mimeType.Substring(0, slashPos.Value) + "/*";
                return ShouldCompressExact(partialMimeType);
            }

            return null;
        }
    }
}
