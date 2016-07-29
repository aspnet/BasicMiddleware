// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public static class UrlRewriteExtensions
    {
        /// <summary>
        /// Imports rules from a mod_rewrite file and adds the rules to current rules. 
        /// </summary>
        /// <param name="options">The UrlRewrite options.</param>
        /// <param name="filePath">The path to the file containing urlrewrite rules.</param>
        public static UrlRewriteOptions ImportFromUrlRewrite(this UrlRewriteOptions options, string filePath)
        {
            if (options == null)
            {
                throw new ArgumentNullException("UrlRewriteOptions is null");
            }
            // TODO use IHostingEnvironment as param.
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException(nameof(filePath));
            }

            using (var stream = File.OpenRead(filePath))
            {
                options.Rules.AddRange(XMLFileParser.Parse(new StreamReader(stream)));
            };
            return options;
        }
    }
}
