// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.AspNetCore.ResponseCompression
{
    /// <inheritdoc/>
    public class DefaultMimeTypeFilterConfig : IMimeTypeFilterConfig
    {
        private readonly HashSet<string> _compressed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _notCompressed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public IEnumerable<string> CompressedMimeTypes => _compressed;

        /// <inheritdoc/>
        public IEnumerable<string> NotCompressedMimeTypes => _notCompressed;

        /// <inheritdoc/>
        public IMimeTypeFilterConfig AddCompressed(params string[] mimeTypes) =>
            AddCompressed((IEnumerable<string>)mimeTypes);

        /// <inheritdoc/>
        public IMimeTypeFilterConfig AddCompressed(IEnumerable<string> mimeTypes) =>
            PerformSetOperation(_compressed, SetOperation.Add, mimeTypes);

        /// <inheritdoc/>
        public IMimeTypeFilterConfig AddNotCompressed(params string[] mimeTypes) =>
            AddNotCompressed((IEnumerable<string>)mimeTypes);

        /// <inheritdoc/>
        public IMimeTypeFilterConfig AddNotCompressed(IEnumerable<string> mimeTypes) =>
            PerformSetOperation(_notCompressed, SetOperation.Add, mimeTypes);

        /// <inheritdoc/>
        public IMimeTypeFilterConfig RemoveCompressed(params string[] mimeTypes) =>
            RemoveCompressed((IEnumerable<string>)mimeTypes);

        /// <inheritdoc/>
        public IMimeTypeFilterConfig RemoveCompressed(IEnumerable<string> mimeTypes) =>
            PerformSetOperation(_compressed, SetOperation.Remove, mimeTypes);

        /// <inheritdoc/>
        public IMimeTypeFilterConfig RemoveNotCompressed(params string[] mimeTypes) =>
            RemoveNotCompressed((IEnumerable<string>)mimeTypes);

        /// <inheritdoc/>
        public IMimeTypeFilterConfig RemoveNotCompressed(IEnumerable<string> mimeTypes) =>
            PerformSetOperation(_notCompressed, SetOperation.Remove, mimeTypes);

        /// <inheritdoc/>
        public IMimeTypeFilterConfig ClearCompressed() =>
            PerformSetOperation(_compressed, SetOperation.Clear);

        /// <inheritdoc/>
        public IMimeTypeFilterConfig ClearNotCompressed() =>
            PerformSetOperation(_notCompressed, SetOperation.Clear);

        /// <inheritdoc/>
        public IMimeTypeFilterConfig ClearAll() =>
            ClearCompressed().ClearNotCompressed();

        /// <inheritdoc/>
        public bool ContainsCompressed(string mimeType) =>
            _compressed.Contains(mimeType);

        /// <inheritdoc/>
        public bool ContainsNotCompressed(string mimeType) =>
            _notCompressed.Contains(mimeType);

        private IMimeTypeFilterConfig PerformSetOperation(
            ISet<string> set,
            SetOperation setOperation,
            IEnumerable<string> mimeTypes = null
        )
        {
            switch (setOperation)
            {
                case SetOperation.Add:
                    if (mimeTypes != null)
                    {
                        set.UnionWith(mimeTypes);
                    }

                    break;
                case SetOperation.Remove:
                    if (mimeTypes != null)
                    {
                        set.ExceptWith(mimeTypes);
                    }

                    break;
                case SetOperation.Clear:
                    set.Clear();
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(setOperation), (int)setOperation, typeof(SetOperation));
            }

            return this;
        }

        private enum SetOperation
        {
            Add,
            Remove,
            Clear
        }
    }
}
