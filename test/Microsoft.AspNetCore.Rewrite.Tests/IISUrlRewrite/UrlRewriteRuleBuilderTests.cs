// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite;
using Xunit;

namespace Microsoft.AspNetCore.Rewrite.Tests.IISUrlRewrite
{
    public class UrlRewriteRuleBuilderTests
    {
        [Fact]
        public void AddRule_SetsRegexTimeoutToOneSecond()
        {
            var ruleBuilder = new UrlRewriteRuleBuilder();

            ruleBuilder.AddUrlMatch("test");

            Assert.Equal(TimeSpan.FromSeconds(1), ruleBuilder._regexTimeout);
        }


        [Fact]
        public void AddRule_QuirkSet_SetsRegexTimeoutToOneMillisecond()
        {
            var ruleBuilder = new UrlRewriteRuleBuilder();

            ruleBuilder._uselowerRegexTimeouts = true;
            ruleBuilder.AddUrlMatch("test");

            Assert.Equal(TimeSpan.FromMilliseconds(1), ruleBuilder._regexTimeout);
        }
    }
}
