using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class Condition
    {
        public Pattern Input { get; set; } // This will eventually be similar to the pattern in mod_rewrite
        public Regex MatchPattern { get; set; } // TODO maybe need to have a default here.
        public bool Negate { get; set; } // default is false
        public bool IgnoreCase { get; set; } = true;
        public MatchType MatchType { get; set; } = MatchType.Pattern;
    }

    public enum MatchType
    {
        Pattern,
        IsFile,
        IsDirectory
    }
}
