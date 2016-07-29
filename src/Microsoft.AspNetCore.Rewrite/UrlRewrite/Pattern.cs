using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class Pattern
    {
        public List<PatternSegment> PatternSegments { get; }

        public Pattern(List<PatternSegment> patternSegments)
        {
            PatternSegments = patternSegments;

        }
        public string Evaluate(HttpContext context, Match ruleMatch, Match condMatch)
        {
            var strBuilder = new StringBuilder();
            foreach (var pattern in PatternSegments)
            {
                strBuilder.Append(pattern.Evaluate(context, ruleMatch, condMatch));
            }
            return strBuilder.ToString();
        }

    }
}
