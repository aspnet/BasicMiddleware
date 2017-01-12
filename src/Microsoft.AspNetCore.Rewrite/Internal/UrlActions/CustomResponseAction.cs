// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Rewrite.Logging;

namespace Microsoft.AspNetCore.Rewrite.Internal.UrlActions
{
    public class CustomResponseAction : UrlAction
    {
        public int StatusCode { get; }
        public string StatusReason { get; }
        public string StatusDescription { get; }

        public CustomResponseAction(int statusCode, string statusReason = null, string statusDescription = null)
        {
            StatusCode = statusCode;
            StatusReason = statusReason;
            StatusDescription = statusDescription;
        }

        public override void ApplyAction(RewriteContext context, BackReferenceCollection ruleBackReferences, BackReferenceCollection conditionBackReferences)
        {
            context.HttpContext.Response.StatusCode = StatusCode;
            if (!string.IsNullOrEmpty(StatusReason))
            {
                context.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = StatusReason;
            }
            if (!string.IsNullOrEmpty(StatusDescription))
            {
                context.HttpContext.Response.WriteAsync(StatusDescription).Wait();
            }
            context.Result = RuleResult.EndResponse;
            context.Logger?.CustomResponse(context.HttpContext.Request.GetEncodedUrl());
        }
    }
}