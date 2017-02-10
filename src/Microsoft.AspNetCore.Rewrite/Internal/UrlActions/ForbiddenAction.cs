// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Rewrite.Internal.UrlActions
{
    public class ForbiddenAction : UrlAction
    {
        public override Task ApplyActionAsync(RewriteContext context, BackReferenceCollection ruleBackReferences, BackReferenceCollection conditionBackReferences)
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Result = RuleResult.EndResponse;
            return TaskCache.CompletedTask;
        }
    }
}
