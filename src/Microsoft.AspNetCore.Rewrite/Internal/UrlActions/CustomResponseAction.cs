// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Rewrite.Logging;

namespace Microsoft.AspNetCore.Rewrite.Internal.UrlActions
{
    public class CustomResponseAction : UrlAction
    {
        public int StatusCode { get; }

        public CustomResponseAction(int statusCode)
        {
            StatusCode = statusCode;
        }

        public override void ApplyAction(RewriteContext context, BackReferenceCollection ruleBackReferences, BackReferenceCollection conditionBackReferences)
        {
            context.HttpContext.Response.StatusCode = StatusCode;
            context.Result = RuleResult.EndResponse;
            context.Logger?.CustomResponse(context.HttpContext.Request.GetEncodedUrl());
        }
    }
}