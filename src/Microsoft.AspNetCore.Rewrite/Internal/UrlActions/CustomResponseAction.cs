// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
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

        public CustomResponseAction(int statusCode, string statusReason, string statusDescription)
        {
            if (string.IsNullOrEmpty(statusReason))
            {
                throw new ArgumentException(nameof(statusReason));
            }

            if (string.IsNullOrEmpty(statusDescription))
            {
                throw new ArgumentException(nameof(statusDescription));
            }

            StatusCode = statusCode;
            StatusReason = statusReason;
            StatusDescription = statusDescription;
        }

        public override void ApplyAction(RewriteContext context, BackReferenceCollection ruleBackReferences, BackReferenceCollection conditionBackReferences)
        {
            context.HttpContext.Response.StatusCode = StatusCode;

            context.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = StatusReason;

            var content = Encoding.UTF8.GetBytes(StatusDescription);
            context.HttpContext.Response.ContentLength = content.Length;
            context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
            context.HttpContext.Response.Body.Write(content, 0, content.Length);

            context.Result = RuleResult.EndResponse;

            context.Logger?.CustomResponse(context.HttpContext.Request.GetEncodedUrl());
        }
    }
}