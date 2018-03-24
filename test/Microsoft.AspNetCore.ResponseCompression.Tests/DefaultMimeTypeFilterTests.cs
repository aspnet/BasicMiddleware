// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Moq;
using Xunit;

namespace Microsoft.AspNetCore.ResponseCompression.Tests
{
    public class DefaultMimeTypeFilterTests
    {
        [Theory]
        [InlineData(false, false, false, false, false)]
        [InlineData(false, false, false, true, false)]
        [InlineData(false, false, true, false, false)]
        [InlineData(false, false, true, true, false)]
        [InlineData(false, true, false, false, true)]
        [InlineData(false, true, false, true, false)]
        [InlineData(false, true, true, false, false)]
        [InlineData(false, true, true, true, false)]
        [InlineData(true, false, false, false, true)]
        [InlineData(true, false, false, true, true)]
        [InlineData(true, false, true, false, false)]
        [InlineData(true, false, true, true, false)]
        [InlineData(true, true, false, false, true)]
        [InlineData(true, true, false, true, true)]
        [InlineData(true, true, true, false, false)]
        [InlineData(true, true, true, true, false)]
        public void ShouldCompress_CorrectResult(
            bool containsCompressedExactMatch,
            bool containsCompressedPartialMatch,
            bool containsNotCompressedExactMatch,
            bool containsNotCompressedPartialMatch,
            bool expected
        )
        {
            const string mimeType = "a/b";
            const string mimeTypePartial = "a/*";

            var filterConfigMock = new Mock<IMimeTypeFilterConfig>();
            filterConfigMock.Setup(config => config.ContainsCompressed(mimeType)).Returns(containsCompressedExactMatch);
            filterConfigMock.Setup(config => config.ContainsCompressed(mimeTypePartial)).Returns(containsCompressedPartialMatch);
            filterConfigMock.Setup(config => config.ContainsNotCompressed(mimeType)).Returns(containsNotCompressedExactMatch);
            filterConfigMock.Setup(config => config.ContainsNotCompressed(mimeTypePartial)).Returns(containsNotCompressedPartialMatch);

            var sut = new DefaultMimeTypeFilter(filterConfigMock.Object);
            var actual = sut.ShouldCompress(mimeType);
            Assert.Equal(expected, actual);
        }
    }
}
