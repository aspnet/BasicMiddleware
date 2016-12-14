// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Rewrite.Internal.PatternSegments
{
    public class ConditionMatchSegment : PatternSegment
    {
        private readonly int _index;

        public ConditionMatchSegment(int index)
        {
            _index = index;
        }

        public override string Evaluate(RewriteContext context, MatchResults ruleMatch, MatchResults condMatch)
        {
            if (condMatch.BackReferences != null)
            {
                //Url Rewrite
                return condMatch.BackReferences.GetBackReferenceAtIndex(_index);
            }
            else
            {
                // For mod_rewrite
                return condMatch.BackReference[_index].Value;
            }
        }
    }
}
