using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public static class UrlRewriteExtensions
    {
        /// <summary>
        /// Imports rules from a mod_rewrite file and adds the rules to current rules. 
        /// </summary>
        /// <param name="options">The UrlRewrite options.</param>
        /// <param name="filePath">The path to the file containing mod_rewrite rules.</param>
        public static UrlRewriteOptions ImportFromUrlRewrite(this UrlRewriteOptions options, string filePath)
        {
            // TODO use IHostingEnvironment as param.
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException(nameof(filePath));
            }
            // TODO IHosting to fix!

            using (var stream = File.OpenRead(filePath))
            {
                options.Rules.AddRange(FileParser.Parse(new StreamReader(stream)));
            };
            return options;
        }
    }
}
