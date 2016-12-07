﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite.Internal.PatternSegments;
using Xunit;

namespace Microsoft.AspNetCore.Rewrite.Tests.PatternSegments
{
    public class UrlSegmentTests
    {
        [Theory]
        [InlineData(false, "http", "localhost", 80, "/foo/bar", "/foo/bar")]
        [InlineData(true, "http", "localhost", 80, "", "http://localhost:80/")]
        [InlineData(true, "http", "localhost", 80, "/foo/bar", "http://localhost:80/foo/bar")]
        [InlineData(true, "http", "localhost", 81, "/foo/bar", "http://localhost:81/foo/bar")]
        [InlineData(true, "https", "localhost", 443, "/foo/bar", "https://localhost:443/foo/bar")]
        public void AssertSegmentIsCorrect(bool globalRule, string scheme, string host, int port, string path, string expectedResult)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = scheme;
            httpContext.Request.Host = new HostString(host, port);
            httpContext.Request.Path = new PathString(path);

            var context = new RewriteContext { HttpContext = httpContext, GlobalRule = globalRule };
            context.HttpContext = httpContext;

            var segment = new UrlSegment();

            // Act
            var results = segment.Evaluate(context, null, null);

            // Assert
            Assert.Equal(expectedResult, results);
        }
    }
}
