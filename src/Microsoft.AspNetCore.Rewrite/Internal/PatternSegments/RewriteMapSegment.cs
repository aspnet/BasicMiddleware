using System;

using Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Rewrite.Internal.PatternSegments
{
    public class RewriteMapSegment : PatternSegment
    {
        private readonly IISRewriteMap _rewriteMap;
        private readonly Pattern _pattern;

        public RewriteMapSegment(IISRewriteMap rewriteMap, Pattern pattern)
        {
            _rewriteMap = rewriteMap;
            _pattern = pattern;
        }

        public override string Evaluate(RewriteContext context, MatchResults ruleMatch, MatchResults condMatch)
        {
            string key = _pattern.Evaluate(context, ruleMatch, condMatch);
            string value;
            if (_rewriteMap.TryGetEntry(key, out value))
            {
                return value;
            }
            var exception = new Exception($"Rewrite map entry not found: '{key}'");
            context.Logger.LogError(context.HttpContext.TraceIdentifier, exception);
            throw exception;
        }
    }
}