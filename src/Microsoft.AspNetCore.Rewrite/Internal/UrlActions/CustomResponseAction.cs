﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Rewrite.Logging;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Rewrite.Internal.UrlActions
{
    public class CustomResponseAction : UrlAction
    {
        public int StatusCode { get; }
        public string StatusReason { get; set; }
        public string StatusDescription { get; set; }

        public CustomResponseAction(int statusCode)
        {
            StatusCode = statusCode;
        }

        public override Task ApplyActionAsync(RewriteContext context, BackReferenceCollection ruleBackReferences, BackReferenceCollection conditionBackReferences)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = StatusCode;

            if (!string.IsNullOrEmpty(StatusReason))
            {
                context.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = StatusReason;
            }

            context.Result = RuleResult.EndResponse;
            context.Logger?.CustomResponse(context.HttpContext.Request.GetEncodedUrl());

            if (!string.IsNullOrEmpty(StatusDescription))
            {
                var content = Encoding.UTF8.GetBytes(StatusDescription);
                response.ContentLength = content.Length;
                response.ContentType = "text/plain; charset=utf-8";
                return response.Body.WriteAsync(content, 0, content.Length);
            }

            return TaskCache.CompletedTask;
        }
    }
}