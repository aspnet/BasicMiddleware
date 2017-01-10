﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite.Internal;
using Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNetCore.Rewrite.Tests.UrlRewrite
{
    public class ServerVariableTests
    {
        [Theory]
        [InlineData("CONTENT_LENGTH", "10", UriMatchCondition.UriMatchPart.Path)]
        [InlineData("CONTENT_TYPE", "json", UriMatchCondition.UriMatchPart.Path)]
        [InlineData("HTTP_ACCEPT", "accept", UriMatchCondition.UriMatchPart.Path)]
        [InlineData("HTTP_COOKIE", "cookie", UriMatchCondition.UriMatchPart.Path)]
        [InlineData("HTTP_HOST", "example.com", UriMatchCondition.UriMatchPart.Path)]
        [InlineData("HTTP_REFERER", "referer", UriMatchCondition.UriMatchPart.Path)]
        [InlineData("HTTP_USER_AGENT", "useragent", UriMatchCondition.UriMatchPart.Path)]
        [InlineData("HTTP_CONNECTION", "connection", UriMatchCondition.UriMatchPart.Path)]
        [InlineData("HTTP_URL", "/foo", UriMatchCondition.UriMatchPart.Path)]
        [InlineData("HTTP_URL", "http://example.com/foo?bar=1", UriMatchCondition.UriMatchPart.Full)]
        [InlineData("QUERY_STRING", "bar=1", UriMatchCondition.UriMatchPart.Path)]
        [InlineData("REQUEST_FILENAME", "/foo", UriMatchCondition.UriMatchPart.Path)]
        [InlineData("REQUEST_URI", "/foo", UriMatchCondition.UriMatchPart.Path)]
        [InlineData("REQUEST_URI", "http://example.com/foo?bar=1", UriMatchCondition.UriMatchPart.Full)]
        public void CheckServerVariableParsingAndApplication(string variable, string expected, UriMatchCondition.UriMatchPart uriMatchPart)
        {
            // Arrange and Act
            var testParserContext = new ParserContext("test");
            var serverVar = ServerVariables.FindServerVariable(variable, testParserContext, uriMatchPart);
            var lookup = serverVar.Evaluate(CreateTestHttpContext(), CreateTestRuleMatch().BackReferences, CreateTestCondMatch().BackReferences);
            // Assert
            Assert.Equal(expected, lookup);
        }

        private RewriteContext CreateTestHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Scheme = "http";
            context.Request.Host = new HostString("example.com");
            context.Request.Path = PathString.FromUriComponent("/foo");
            context.Request.QueryString = QueryString.FromUriComponent("?bar=1");
            context.Request.ContentLength = 10;
            context.Request.ContentType = "json";
            context.Request.Headers[HeaderNames.Accept] = "accept";
            context.Request.Headers[HeaderNames.Cookie] = "cookie";
            context.Request.Headers[HeaderNames.Referer] = "referer";
            context.Request.Headers[HeaderNames.UserAgent] = "useragent";
            context.Request.Headers[HeaderNames.Connection] = "connection";
            return new RewriteContext { HttpContext = context };
        }

        private MatchResults CreateTestRuleMatch()
        {
            var match = Regex.Match("foo/bar/baz", "(.*)/(.*)/(.*)");
            return new MatchResults { BackReferences = new BackReferenceCollection(match.Groups), Success = match.Success };
        }

        private MatchResults CreateTestCondMatch()
        {
            var match = Regex.Match("foo/bar/baz", "(.*)/(.*)/(.*)");
            return new MatchResults { BackReferences = new BackReferenceCollection(match.Groups), Success = match.Success };
        }

        [Fact]
        private void EmptyQueryStringCheck()
        {
            var context = new DefaultHttpContext();
            var rewriteContext = new RewriteContext { HttpContext = context };
            var testParserContext = new ParserContext("test");
            var serverVar = ServerVariables.FindServerVariable("QUERY_STRING", testParserContext, UriMatchCondition.UriMatchPart.Path);
            var lookup = serverVar.Evaluate(rewriteContext, CreateTestRuleMatch().BackReferences, CreateTestCondMatch().BackReferences);

            Assert.Equal(string.Empty, lookup);
        }
    }
}
