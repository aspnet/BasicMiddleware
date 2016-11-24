// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Rewrite.Internal
{
    public static class ConditionHelper
    {

        public static MatchResults Evaluate(IEnumerable<Condition> conditions, RewriteContext context, MatchResults ruleMatch, bool trackAllCaptures = false)
        {
            MatchResults prevResult = null;
            var orSucceeded = false;
            foreach (var condition in conditions)
            {
                if (orSucceeded && condition.OrNext)
                {
                    continue;
                }
                else if (orSucceeded)
                {
                    orSucceeded = false;
                    continue;
                }

                var condResult = condition.Evaluate(context, ruleMatch, prevResult);

                if (condition.OrNext)
                {
                    orSucceeded = condResult.Success;
                }
                else if (!condResult.Success)
                {
                    return condResult;
                }

                if (condResult.Success && trackAllCaptures && prevResult?.BackReferences != null)
                {
                    prevResult.BackReferences.Add(condResult.BackReferences);
                    condResult.BackReferences = prevResult.BackReferences;
                }

                prevResult = condResult;
            }
            return prevResult;
        }
    }
}
