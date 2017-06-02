// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite.Internal.UrlActions;
using Xunit;

namespace Microsoft.AspNetCore.Rewrite.Tests.UrlActions
{
    public class AbortActionTests
    {
        public async Task AbortAction_VerifyEndResponseResult()
        {
            var context = new RewriteContext { HttpContext = new DefaultHttpContext() };
            var action = new AbortAction();

            await action.ApplyActionAsync(context, null, null);

            Assert.Equal(RuleResult.EndResponse, context.Result);
        }
    }
}
