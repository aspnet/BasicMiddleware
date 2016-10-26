﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite;
using Xunit;

namespace Microsoft.AspNetCore.Rewrite.Tests.UrlRewrite
{
    public class FormatExceptionHandlingTests
    {
        [Theory]
        [InlineData(
@"<rewrite>
    <rules>
        <rule name=""Rewrite to article.aspx"">
        </rule>
    </rules>
</rewrite>",
            "Could not parse the UrlRewrite file. Message: 'Cannot have rule without match'. Line number '3': '10'.")]
        [InlineData(
@"<rewrite>
    <rules>
        <rule name=""Rewrite to article.aspx"">
            <match url = ""(.*)"" />
            <action type=""Rewrite"" url =""{"" />
        </rule>
    </rules>
</rewrite>",
            "Could not parse the UrlRewrite file. Message: 'Missing close brace for parameter at string index: '1''. Line number '5': '14'.")]
        [InlineData(
@"<rewrite>
    <rules>
        <rule name=""Rewrite to article.aspx"">
            <match />
            <action type=""Rewrite"" url=""foo"" />
        </rule>
    </rules>
</rewrite>",
            "Could not parse the UrlRewrite file. Message: 'Match must have Url Attribute'. Line number '4': '14'.")]
        [InlineData(
@"<rewrite>
    <rules>
        <rule name=""Rewrite to article.aspx"">
            <match url = ""(.*)"" />
            <conditions>
                <add input=""{HTTPS"" pattern=""^OFF$"" />
            </conditions>
            <action type=""Rewrite"" url =""foo"" />
        </rule>
    </rules>
</rewrite>",
            "Could not parse the UrlRewrite file. Message: 'Missing close brace for parameter at string index: '6''. Line number '6': '18'.")]
        [InlineData(
@"<rewrite>
    <rules>
        <rule name=""Rewrite to article.aspx"">
            <match url = ""(.*)"" />
            <conditions>
                <add pattern=""^OFF$"" />
            </conditions>
            <action type=""Rewrite"" url =""foo"" />
        </rule>
    </rules>
</rewrite>",
            "Could not parse the UrlRewrite file. Message: 'Conditions must have an input attribute'. Line number '6': '18'.")]
        [InlineData(
@"<rewrite>
    <rules>
        <rule name=""Rewrite to article.aspx"">
            <match url = ""(.*)"" />
            <conditions>
                <add input=""{HTTPS}"" />
            </conditions>
            <action type=""Rewrite"" url =""foo"" />
        </rule>
    </rules>
</rewrite>",
            "Could not parse the UrlRewrite file. Message: 'Match does not have an associated pattern attribute in condition'. Line number '6': '18'.")]
        [InlineData(
@"<rewrite>
    <rules>
        <rule name=""Rewrite to article.aspx"">
            <match url = ""(.*)"" />
            <conditions>
                <add input=""{HTTPS}"" patternSyntax=""ExactMatch""/>
            </conditions>
            <action type=""Rewrite"" url =""foo"" />
        </rule>
    </rules>
</rewrite>",
            "Could not parse the UrlRewrite file. Message: 'Match does not have an associated pattern attribute in condition'. Line number '6': '18'.")]
        [InlineData(
@"<rewrite>
    <rules>
        <rule name=""Rewrite to article.aspx"">
            <match url = ""(.*)"" />
            <action type=""Rewrite"" url ="""" />
        </rule>
    </rules>
</rewrite>",
            "Could not parse the UrlRewrite file. Message: 'Url attribute cannot contain an empty string'. Line number '5': '14'.")]
        [InlineData(
@"<rewrite>
    <rules>
        <rule name=""Remove trailing slash"">
            <match url = ""(.*)/$"" />
            <action type=""Redirect"" redirectType=""foo"" url =""{R:1}"" />
        </rule>
    </rules>
</rewrite>",
            "Could not parse the UrlRewrite file. Message: 'The redirectType parameter was unrecognized'. Line number '5': '14'.")]
        public void ThrowFormatExceptionWithCorrectMessage(string input, string expected)
        {
            // Arrange, Act, Assert
            var ex = Assert.Throws<FormatException>(() => new UrlRewriteFileParser().Parse(new StringReader(input)));
            Assert.Equal(expected, ex.Message);
        }
    }
}
