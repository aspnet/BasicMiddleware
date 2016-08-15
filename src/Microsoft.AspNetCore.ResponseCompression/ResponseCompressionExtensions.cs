// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.AspNetCore.ResponseCompression
{
    public static class ResponseCompressionExtensions
    {
        /// <summary>
        /// Allows to compress HTTP Responses.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/> instance this method extends.</param>
        /// <param name="options">The <see cref="ResponseCompressionOptions"/>.</param>
        public static IApplicationBuilder UseResponseCompression(this IApplicationBuilder builder, ResponseCompressionOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return builder.UseMiddleware<ResponseCompressionMiddleware>(Options.Create(options));
        }
    }
}
