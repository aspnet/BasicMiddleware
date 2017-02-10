// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Rewrite.Internal.UrlActions
{
    public class NoneAction : UrlAction
    {
        public RuleResult Result { get; }

        public NoneAction(RuleResult result)
        {
            Result = result;
        }
        // Explicitly say that nothing happens
        public override Task ApplyActionAsync(RewriteContext context, BackReferenceCollection ruleBackReferences, BackReferenceCollection conditionBackReferences)
        {
            context.Result = Result;
            return TaskCache.CompletedTask;
        }
    }
}
