// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;

namespace Microsoft.AspNetCore.Rewrite.Internal.PatternSegments
{
    public class HeaderSegment : PatternSegment
    {
        private readonly string _header;
        private readonly Pattern _pattern;

        public HeaderSegment(string header)
        {
            _header = header;
        }

        public HeaderSegment(Pattern pattern)
        {
            _pattern = pattern;
        }

        public override string Evaluate(RewriteContext context, BackReferenceCollection ruleBackReferences, BackReferenceCollection conditionBackReference)
        {
            string header;
            if (_pattern != null)
            {
                // PERF 
                // Because we need to be able to evaluate multiple nested patterns,
                // we provided a new string builder and evaluate the new pattern,
                // and restore it after evaluation.
                var oldBuilder = context.Builder;
                context.Builder = new StringBuilder(64);
                header = _pattern.Evaluate(context, ruleBackReferences, conditionBackReference);
                context.Builder = oldBuilder;
            }
            else
            {
                header = _header;
            }
            return context.HttpContext.Request.Headers[header];
        }
    }
}