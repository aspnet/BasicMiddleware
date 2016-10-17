// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Rewrite.Internal.UrlActions
{
    public class AbortAction : UrlAction
    {
        public override void ApplyAction(RewriteContext context, MatchResults ruleMatch, MatchResults condMatch)
        {
            context.HttpContext.Abort();
            context.Result = RuleResult.EndResponse;
        }
    }
}
