using System;

namespace Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite
{
    public class ServerVariable
    {
        public string Name { get; }
        public Pattern Pattern { get; }

        public ServerVariable(string name, Pattern pattern)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }
            if (pattern == null)
            {
                throw new ArgumentException(nameof(pattern));
            }

            Name = name;
            Pattern = pattern;
        }

        public string Evaluate(RewriteContext context, MatchResults ruleMatch, MatchResults condMatch)
        {
            return Pattern.Evaluate(context, ruleMatch, condMatch);
        }
    }
}