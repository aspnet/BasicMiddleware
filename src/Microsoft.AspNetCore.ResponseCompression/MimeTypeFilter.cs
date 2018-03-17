// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.ResponseCompression
{
    /// <summary>
    /// MIME type filtering for the HTTP response compression middleware.
    /// </summary>
    public class MimeTypeFilter
    {
        private readonly HashSet<string> _compressed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _notCompressed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// True if any of the Add..., Remove... or Clear... methods has been called at least once, otherwise false.
        /// </summary>
        public bool HasBeenPopulated { get; private set; } = false;

        /// <summary>
        /// MIME types to compress responses for.
        /// </summary>
        public IEnumerable<string> CompressedMimeTypes => _compressed;

        /// <summary>
        /// MIME types to not compress responses for.
        /// </summary>
        public IEnumerable<string> NotCompressedMimeTypes => _notCompressed;

        /// <summary>
        /// Adds MIME types to compress responses for.
        /// </summary>
        /// <param name="mimeTypes">MIME types to add. Supported strings are "&lt;type>/&lt;subtype>", "&lt;type>/*" and "*/*".</param>
        /// <returns>This instance of MimeTypeFilter.</returns>
        public MimeTypeFilter AddCompressed(params string[] mimeTypes) =>
            Add(_compressed, mimeTypes);

        /// <summary>
        /// Adds MIME types to not compress responses for.
        /// </summary>
        /// <param name="mimeTypes">MIME types to add. Supported strings are "&lt;type>/&lt;subtype>", "&lt;type>/*" and "*/*".</param>
        /// <returns>This instance of MimeTypeFilter.</returns>
        public MimeTypeFilter AddNotCompressed(params string[] mimeTypes) =>
            Add(_notCompressed, mimeTypes);

        /// <summary>
        /// Removes from the list of MIME types to compress responses for.
        /// </summary>
        /// <param name="mimeTypes">MIME types to remove. Supported strings are "&lt;type>/&lt;subtype>", "&lt;type>/*" and "*/*".</param>
        /// <returns>This instance of MimeTypeFilter.</returns>
        public MimeTypeFilter RemoveCompressed(params string[] mimeTypes) =>
            Remove(_compressed, mimeTypes);

        /// <summary>
        /// Removes from the list of MIME types to not compress responses for.
        /// </summary>
        /// <param name="mimeTypes">MIME types to remove. Supported strings are "&lt;type>/&lt;subtype>", "&lt;type>/*" and "*/*".</param>
        /// <returns>This instance of MimeTypeFilter.</returns>
        public MimeTypeFilter RemoveNotCompressed(params string[] mimeTypes) =>
            Remove(_notCompressed, mimeTypes);

        /// <summary>
        /// Clears the list of MIME types to compress responses for.
        /// </summary>
        /// <returns>This instance of MimeTypeFilter.</returns>
        public MimeTypeFilter ClearCompressed() =>
            Clear(_compressed);

        /// <summary>
        /// Clears the list of MIME types to not compress responses for.
        /// </summary>
        /// <returns>This instance of MimeTypeFilter.</returns>
        public MimeTypeFilter ClearNotCompressed() =>
            Clear(_notCompressed);

        /// <summary>
        /// Clears the lists of MIME types to compress responses for and to not compress responses for.
        /// </summary>
        /// <returns>This instance of MimeTypeFilter.</returns>
        public MimeTypeFilter ClearAll() =>
            ClearCompressed().ClearNotCompressed();

        /// <summary>
        /// Determines if the response with the passed MIME type should be compressed.
        /// </summary>
        /// <param name="mimeType">MIME type to check.</param>
        /// <returns>True if the response with passed MIME type should be compressed, otherwise false.</returns>
        public bool ShouldCompress(string mimeType)
        {
            bool? shouldCompress;

            //check exact match (type/subtype)
            shouldCompress = ShouldCompressExact(mimeType);

            if (shouldCompress.HasValue)
            {
                return shouldCompress.Value;
            }

            //check partial match (type/*)
            int slashPos = mimeType.IndexOf('/');

            if (slashPos > -1)
            {
                string wildcardMimeType = $"{mimeType.Substring(0, slashPos + 1)}*";
                shouldCompress = ShouldCompressExact(wildcardMimeType);

                if (shouldCompress.HasValue)
                {
                    return shouldCompress.Value;
                }
            }

            //check wildcard (*/*)
            shouldCompress = ShouldCompressExact("*/*");

            if (shouldCompress.HasValue)
            {
                return shouldCompress.Value;
            }

            //no matches and no "*/*" entry in either list - do not compress
            return false;
        }

        private MimeTypeFilter Add(ISet<string> set, IEnumerable<string> mimeTypes)
        {
            if (mimeTypes != null)
            {
                foreach (string mimeType in mimeTypes)
                {
                    if (!IsValid(mimeType))
                    {
                        throw new ArgumentException(
                            $@"MIME type ""{mimeType}"" does not match the format ""<type>/<sybtype>"", ""<type>/*"" or ""*/*"".",
                            nameof(mimeTypes)
                        );
                    }

                    set.Add(mimeType);
                }
            }

            HasBeenPopulated = true;
            return this;
        }

        private MimeTypeFilter Remove(ISet<string> set, IEnumerable<string> mimeTypes)
        {
            if (mimeTypes != null)
            {
                set.ExceptWith(mimeTypes);
            }

            HasBeenPopulated = true;
            return this;
        }

        private MimeTypeFilter Clear(ISet<string> set)
        {
            set.Clear();
            HasBeenPopulated = true;
            return this;
        }

        //This is NOT a proper MIME type validation. It only checks conformance to the format
        //"type/sybtype", "type/*" or "*/*", for the purpose of being used in the lists of MIME
        //types that should and should not be compressed.
        private static bool IsValid(string mimeType) =>
            Regex.IsMatch(mimeType, @"^([^\s\*/]+/([^\s\*/]+|\*)|\*/\*)$", RegexOptions.Compiled);

        private bool? ShouldCompressExact(string mimeType)
        {
            //check not compressed first, then compressed
            if (_notCompressed.Contains(mimeType))
            {
                return false;
            }

            if (_compressed.Contains(mimeType))
            {
                return true;
            }

            return null;
        }
    }
}
