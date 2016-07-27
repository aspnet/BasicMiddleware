using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class Match
    {
        // TODO can convert this to be Regex.
        public Regex Url { get; set; } // must be a non-empty string
        public bool IgnoreCase { get; set; } = true;
        public bool Negate { get; set; } = false;
    }
}
