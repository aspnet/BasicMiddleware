// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.ResponseCompression
{
    /// <summary>
    /// Configuration for <see cref="IMimeTypeFilter"/>.
    /// </summary>
    public interface IMimeTypeFilterConfig
    {
        /// <summary>
        /// MIME types to compress responses for.
        /// </summary>
        IEnumerable<string> CompressedMimeTypes { get; }

        /// <summary>
        /// MIME types to not compress responses for.
        /// </summary>
        IEnumerable<string> NotCompressedMimeTypes { get; }

        /// <summary>
        /// Adds MIME types to compress responses for.
        /// </summary>
        /// <param name="mimeTypes">MIME types to add.</param>
        /// <returns>The <see cref="IMimeTypeFilterConfig"/> so that additional calls can be chained.</returns>
        IMimeTypeFilterConfig AddCompressed(params string[] mimeTypes);

        /// <inheritdoc cref="AddCompressed(string[])"/>
        IMimeTypeFilterConfig AddCompressed(IEnumerable<string> mimeTypes);

        /// <summary>
        /// Adds MIME types to not compress responses for.
        /// </summary>
        /// <param name="mimeTypes">MIME types to add.</param>
        /// <returns>The <see cref="IMimeTypeFilterConfig"/> so that additional calls can be chained.</returns>
        IMimeTypeFilterConfig AddNotCompressed(params string[] mimeTypes);

        /// <inheritdoc cref="AddNotCompressed(string[])"/>
        IMimeTypeFilterConfig AddNotCompressed(IEnumerable<string> mimeTypes);

        /// <summary>
        /// Removes from the list of MIME types to compress responses for.
        /// </summary>
        /// <param name="mimeTypes">MIME types to remove.</param>
        /// <returns>The <see cref="IMimeTypeFilterConfig"/> so that additional calls can be chained.</returns>
        IMimeTypeFilterConfig RemoveCompressed(params string[] mimeTypes);

        /// <inheritdoc cref="RemoveCompressed(string[])"/>
        IMimeTypeFilterConfig RemoveCompressed(IEnumerable<string> mimeTypes);

        /// <summary>
        /// Removes from the list of MIME types to not compress responses for.
        /// </summary>
        /// <param name="mimeTypes">MIME types to remove.</param>
        /// <returns>The <see cref="IMimeTypeFilterConfig"/> so that additional calls can be chained.</returns>
        IMimeTypeFilterConfig RemoveNotCompressed(params string[] mimeTypes);

        /// <inheritdoc cref="RemoveNotCompressed(string[])"/>
        IMimeTypeFilterConfig RemoveNotCompressed(IEnumerable<string> mimeTypes);

        /// <summary>
        /// Clears the list of MIME types to compress responses for.
        /// </summary>
        /// <returns>The <see cref="IMimeTypeFilterConfig"/> so that additional calls can be chained.</returns>
        IMimeTypeFilterConfig ClearCompressed();

        /// <summary>
        /// Clears the list of MIME types to not compress responses for.
        /// </summary>
        /// <returns>The <see cref="IMimeTypeFilterConfig"/> so that additional calls can be chained.</returns>
        IMimeTypeFilterConfig ClearNotCompressed();

        /// <summary>
        /// Clears the configuration.
        /// </summary>
        /// <returns>The <see cref="IMimeTypeFilterConfig"/> so that additional calls can be chained.</returns>
        IMimeTypeFilterConfig ClearAll();

        /// <summary>
        /// Determines whether the passed MIME type is in the list to compress responses for.
        /// </summary>
        /// <param name="mimeType">MIME type to check.</param>
        /// <returns>True if MIME type is in the list to compress responses for, otherwise false</returns>
        bool ContainsCompressed(string mimeType);

        /// <summary>
        /// Determines whether the passed MIME type is in the list to not compress responses for.
        /// </summary>
        /// <param name="mimeType">MIME type to check.</param>
        /// <returns>True if MIME type is in the list to not compress responses for, otherwise false</returns>
        bool ContainsNotCompressed(string mimeType);
    }
}
