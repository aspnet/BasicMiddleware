using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite.RuleAbstraction;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite.UrlActions
{
    public class RewriteAction : UrlAction
    {
        public RuleTerminiation Result { get; }

        public RewriteAction(RuleTerminiation result, Pattern pattern)
        {
            Result = result;
            Url = pattern;
        }

        public override RuleResult ApplyAction(HttpContext context, MatchResults ruleMatch, MatchResults condMatch)
        {
            var pattern = Url.Evaluate(context, ruleMatch, condMatch);
            context.Request.Path = new PathString(pattern);
            return new RuleResult { Result = Result };
        }
    }
}
