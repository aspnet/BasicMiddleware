// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.ResponseCompression
{
    /// <summary>
    /// MIME type filtering for the HTTP response compression middleware.
    /// </summary>
    public interface IMimeTypeFilter
    {
        /// <summary>
        /// MIME type filter configuration.
        /// </summary>
        IMimeTypeFilterConfig Config { get; }

        /// <summary>
        /// Should the response with the passed MIME type be compressed.
        /// </summary>
        /// <param name="mimeType">MIME type to check.</param>
        /// <returns>True if the response should be compressed, otherwise false.</returns>
        bool ShouldCompress(string mimeType);
    }
}
