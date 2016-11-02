// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite.Internal;
using Microsoft.AspNetCore.Rewrite.Internal.ApacheModRewrite;
using Microsoft.AspNetCore.Rewrite.Internal.UrlActions;
using Xunit;

namespace Microsoft.AspNetCore.Rewrite.Tests
{
    public class RuleBuilderTest
    {
        [Fact]
        // see https://httpd.apache.org/docs/2.4/rewrite/advanced.html#setenvvars
        public void AddAction_Throws_ChangeEnvNotSupported()
        {
            var builder = new RuleBuilder();
            var flags = new Flags();
            flags.SetFlag(FlagType.Env, "rewritten:1");

            var ex = Assert.Throws<NotSupportedException>(() => builder.AddAction(null, flags));
            Assert.Equal(Resources.Error_ChangeEnvironmentNotSupported, ex.Message);
        }

        [Fact]
        public void AddAction_DefaultRedirectStatusCode()
        {
            var builder = new RuleBuilder();
            var flags = new Flags();
            var pattern = new Pattern(new List<PatternSegment>());
            flags.SetFlag(FlagType.Redirect, string.Empty);

            builder.AddAction(pattern, flags);
            var redirectAction = (RedirectAction)builder._actions[0];

            Assert.Equal(StatusCodes.Status302Found, redirectAction.StatusCode);
        }
    }
}