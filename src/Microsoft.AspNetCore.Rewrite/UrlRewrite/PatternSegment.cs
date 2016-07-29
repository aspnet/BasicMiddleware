using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class PatternSegment
    {
        public Func<HttpContext, Match, Match, string> Evaluate { get; }

        public PatternSegment(Func<HttpContext, Match, Match, string> evaluate)
        {
            Evaluate = evaluate;
        }
    }
}
