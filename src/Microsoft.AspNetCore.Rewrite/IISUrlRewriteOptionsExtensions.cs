// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Rewrite
{
    /// <summary>
    /// Extensions for adding IIS Url Rewrite rules to <see cref="RewriteOptions"/>
    /// </summary>
    public static class IISUrlRewriteOptionsExtensions
    {
		/// <summary>
		/// Add rules from a IIS config file containing Url Rewrite rules
		/// </summary>
		/// <param name="options">The <see cref="RewriteOptions"/></param>
		/// <param name="fileProvider">The <see cref="IFileProvider"/> </param>
		/// <param name="filePath">The path to the file containing UrlRewrite rules.</param>
		/// <param name="rewriteMaps">An optional set of rewrite maps to uztilize when parsing rules.
		/// Rewrite maps in this collection will overwrite (supercede) duplicate named entries in the XML.</param>
		public static RewriteOptions AddIISUrlRewrite(this RewriteOptions options, IFileProvider fileProvider, string filePath, IEnumerable<IISRewriteMap> rewriteMaps = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (fileProvider == null)
            {
                throw new ArgumentNullException(nameof(fileProvider));
            }

            var file = fileProvider.GetFileInfo(filePath);

            using (var stream = file.CreateReadStream())
            {
                return AddIISUrlRewrite(options, new StreamReader(stream), rewriteMaps);
            }
        }

		/// <summary>
		/// Add rules from a IIS config file containing Url Rewrite rules
		/// </summary>
		/// <param name="options">The <see cref="RewriteOptions"/></param>
		/// <param name="reader">The text reader stream.</param>
		/// <param name="rewriteMaps">An optional set of rewrite maps to uztilize when parsing rules.
		/// Rewrite maps in this collection will overwrite (supercede) duplicate named entries in the XML.</param>
		public static RewriteOptions AddIISUrlRewrite(this RewriteOptions options, TextReader reader, IEnumerable<IISRewriteMap> rewriteMaps = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (reader == null)
            {
                throw new ArgumentException(nameof(reader));
            }

            var rules = new UrlRewriteFileParser().Parse(reader, rewriteMaps);

            foreach (var rule in rules)
            {
                options.Rules.Add(rule);
            }

            return options;
        }
    }
}