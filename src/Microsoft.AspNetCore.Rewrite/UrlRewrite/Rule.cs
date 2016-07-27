using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class Rule
    {
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public PatternSyntax PatternSyntax { get; set; }
        public bool StopProcessing { get; set; }
        public Match Match { get; set; }
        public Conditions Conditions { get; set; }
        public ServerVariables ServerVariables { get; set; }
        public UrlAction Action { get; set; }
    }

    public enum PatternSyntax
    {
        ECMAScript,
        WildCard,
        ExactMatch
    }
}
