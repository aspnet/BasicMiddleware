// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Rewrite.Internal.PatternSegments
{
    public class ToLowerSegment : PatternSegment
    {
        private readonly Pattern _pattern;

        public ToLowerSegment(Pattern pattern)
        {
            _pattern = pattern;
        }

        public override string Evaluate(RewriteContext context, BackReferenceCollection ruleBackReferences, BackReferenceCollection conditionBackReferences)
        {
            var pattern = _pattern.Evaluate(context, ruleBackReferences, conditionBackReferences);
            return pattern.ToLowerInvariant();
        }
    }
}
