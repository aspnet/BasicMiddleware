// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.ResponseCompression
{
    /// <inheritdoc/>
    public class DefaultMimeTypeFilter : IMimeTypeFilter
    {
        /// <inheritdoc/>
        public IMimeTypeFilterConfig Config { get; }

        /// <inheritdoc/>
        public DefaultMimeTypeFilter(IMimeTypeFilterConfig mimeTypeFilterConfig) =>
            Config = mimeTypeFilterConfig ?? throw new ArgumentNullException(nameof(mimeTypeFilterConfig));

        /// <inheritdoc/>
        public virtual bool ShouldCompress(string mimeType) =>
            ShouldCompressExact(mimeType) //check exact match type/subtype
            ?? ShouldCompressPartial(mimeType) //check partial match type/*
            ?? ShouldCompressExact("*/*") //check wildcard */*
            ?? false; //no matches - do not compress

        private bool? ShouldCompressExact(string mimeType)
        {
            //check not compressed first, then compressed
            if (Config.ContainsNotCompressed(mimeType))
            {
                return false;
            }

            if (Config.ContainsCompressed(mimeType))
            {
                return true;
            }

            return null;
        }

        private bool? ShouldCompressPartial(string mimeType)
        {
            int? slashPos = mimeType?.IndexOf('/');

            if (slashPos > -1)
            {
                string partialMimeType = $"{mimeType.Substring(0, slashPos.Value)}/*";
                return ShouldCompressExact(partialMimeType);
            }

            return null;
        }
    }
}
