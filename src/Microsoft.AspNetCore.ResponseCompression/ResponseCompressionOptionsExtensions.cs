// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.ResponseCompression
{
    /// <summary>
    /// Extension methods for the <see cref="ResponseCompressionOptions"/>.
    /// </summary>
    public static class ResponseCompressionOptionsExtensions
    {
        /// <summary>
        /// Use MIME type filter with default implementation.
        /// </summary>
        /// <param name="responseCompressionOptions">The <see cref="ResponseCompressionOptions"/> to set the MIME type filter for.</param>
        /// <param name="configureFilter">MIME type filter configuration actions.</param>
        /// <returns>The <see cref="ResponseCompressionOptions"/> so that additional calls can be chained.</returns>
        public static ResponseCompressionOptions UseMimeTypeFilter(
            this ResponseCompressionOptions responseCompressionOptions,
            Action<IMimeTypeFilterConfig> configureFilter
        ) =>
            responseCompressionOptions.UseMimeTypeFilter(
                configureFilter,
                new DefaultMimeTypeFilter(new DefaultMimeTypeFilterConfig())
            );

        /// <summary>
        /// Use MIME type filter with the specified implementation.
        /// </summary>
        /// <param name="responseCompressionOptions">The <see cref="ResponseCompressionOptions"/> to set the MIME type filter for.</param>
        /// <param name="configureFilter">MIME type filter configuration actions.</param>
        /// <param name="mimeTypeFilter">MIME type filter implementation.</param>
        /// <returns>The <see cref="ResponseCompressionOptions"/> so that additional calls can be chained.</returns>
        public static ResponseCompressionOptions UseMimeTypeFilter(
            this ResponseCompressionOptions responseCompressionOptions,
            Action<IMimeTypeFilterConfig> configureFilter,
            IMimeTypeFilter mimeTypeFilter
        )
        {
            if (responseCompressionOptions == null)
            {
                throw new ArgumentNullException(nameof(responseCompressionOptions));
            }

            if (configureFilter == null)
            {
                throw new ArgumentNullException(nameof(configureFilter));
            }

            if (mimeTypeFilter == null)
            {
                throw new ArgumentNullException(nameof(mimeTypeFilter));
            }

            configureFilter(mimeTypeFilter.Config);
            responseCompressionOptions.MimeTypeFilter = mimeTypeFilter;
            return responseCompressionOptions;
        }
    }
}
