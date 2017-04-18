// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Rewrite.Internal
{
    public class Pattern
    {
        public IList<PatternSegment> PatternSegments { get; }
        public Pattern(IList<PatternSegment> patternSegments)
        {
            PatternSegments = patternSegments;
        }

        public string Evaluate(RewriteContext context, BackReferenceCollection ruleBackReferences, BackReferenceCollection conditionBackReferences)
        {
            var pooledBuilder = PooledStringBuilder.Allocate();
            try
            {
                var builder = pooledBuilder.Builder;
                foreach (var pattern in PatternSegments)
                {
                    builder.Append(pattern.Evaluate(context, ruleBackReferences, conditionBackReferences));
                }
                return builder.ToString();
            }
            finally
            {
                pooledBuilder.Free();
            }
        }
    }
}
