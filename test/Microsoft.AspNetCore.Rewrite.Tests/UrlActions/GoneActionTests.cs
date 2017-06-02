// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite.Internal.UrlActions;
using Xunit;

namespace Microsoft.AspNetCore.Rewrite.Tests.UrlActions
{
    public class GoneActionTests
    {
        [Fact]
        public async Task Gone_Verify410IsInStatusCode()
        {
            var context = new RewriteContext { HttpContext = new DefaultHttpContext() };
            var action = new GoneAction();

            await action.ApplyActionAsync(context, null, null);

            Assert.Equal(RuleResult.EndResponse, context.Result);
            Assert.Equal(StatusCodes.Status410Gone, context.HttpContext.Response.StatusCode);
        }
    }
}
