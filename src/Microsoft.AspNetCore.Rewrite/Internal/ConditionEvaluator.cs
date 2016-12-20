// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Rewrite.Internal
{
    public static class ConditionHelper
    {
        public static BackReferenceCollection Evaluate(IEnumerable<Condition> conditions, RewriteContext context, BackReferenceCollection backReferences)
        {
            return Evaluate(conditions, context, backReferences, trackAllCaptures: false);
        }
        public static BackReferenceCollection Evaluate(IEnumerable<Condition> conditions, RewriteContext context, BackReferenceCollection backReferences, bool trackAllCaptures)
        {
            BackReferenceCollection prevBackReferences = null;
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

                var condBackReferences = condition.Evaluate(context, backReferences, prevBackReferences);

                if (condition.OrNext)
                {
                    orSucceeded = condBackReferences != null;
                }
                else if (condBackReferences == null)
                {
                    return condBackReferences;
                }

                if (condBackReferences != null && trackAllCaptures && prevBackReferences!= null)
                {
                    prevBackReferences.Add(condBackReferences);
                    condBackReferences = prevBackReferences;
                }

                prevBackReferences = condBackReferences;
            }
            return prevBackReferences;
        }
    }
}
