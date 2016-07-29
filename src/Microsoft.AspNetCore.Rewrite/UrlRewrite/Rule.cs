using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Rewrite.RuleAbstraction;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class UrlRewriteRule : Rule
    {
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public PatternSyntax PatternSyntax { get; set; }
        public bool StopProcessing { get; set; }
        public InitialMatch Match { get; set; }
        public Conditions Conditions { get; set; }
        public UrlAction Action { get; set; }

        public override RuleResult ApplyRule(UrlRewriteContext context)
        {
            throw new NotImplementedException();
        }
    }

    public enum PatternSyntax
    {
        ECMAScript,
        WildCard,
        ExactMatch
    }
}
