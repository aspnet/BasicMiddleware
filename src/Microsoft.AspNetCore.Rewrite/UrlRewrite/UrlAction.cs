using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite.RuleAbstraction;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public abstract class UrlAction
    {
        public Pattern Url { get; set; }

        public abstract RuleResult ApplyAction(HttpContext context, MatchResults ruleMatch, MatchResults condMatch);
    }
}
