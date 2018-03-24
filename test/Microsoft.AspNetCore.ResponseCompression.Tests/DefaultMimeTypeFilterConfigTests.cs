// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.ResponseCompression.Tests
{
    public class DefaultMimeTypeFilterConfigTests
    {
        [Fact]
        public void AddCompressed_Array_CorrectCompressedMimeTypes()
        {
            var sut = new DefaultMimeTypeFilterConfig().AddCompressed(null, "", "a/b", "A/B");

            var expected = (IEnumerable<string>)new[] { null, "", "a/b" };
            var actual = sut.CompressedMimeTypes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddNotCompressed_Array_CorrectNotCompressedMimeTypes()
        {
            var sut = new DefaultMimeTypeFilterConfig().AddNotCompressed(null, "", "a/b", "A/B");

            var expected = (IEnumerable<string>)new[] { null, "", "a/b" };
            var actual = sut.NotCompressedMimeTypes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RemoveCompressed_Array_CorrectCompressedMimeTypes()
        {
            var sut = new DefaultMimeTypeFilterConfig()
                .AddCompressed(null, "", "a/b", "c/d", "e/f")
                .RemoveCompressed(null, "", "A/B", "e/f");

            var expected = (IEnumerable<string>)new[] { "c/d" };
            var actual = sut.CompressedMimeTypes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RemoveNotCompressed_Array_CorrectNotCompressedMimeTypes()
        {
            var sut = new DefaultMimeTypeFilterConfig()
                .AddNotCompressed(null, "", "a/b", "c/d", "e/f")
                .RemoveNotCompressed(null, "", "A/B", "e/f");

            var expected = (IEnumerable<string>)new[] { "c/d" };
            var actual = sut.NotCompressedMimeTypes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddCompressed_IEnumerable_CorrectCompressedMimeTypes()
        {
            var sut = new DefaultMimeTypeFilterConfig().AddCompressed((IEnumerable<string>)new[] { null, "", "a/b", "A/B" });

            var expected = (IEnumerable<string>)new[] { null, "", "a/b" };
            var actual = sut.CompressedMimeTypes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddNotCompressed_IEnumerable_CorrectNotCompressedMimeTypes()
        {
            var sut = new DefaultMimeTypeFilterConfig().AddNotCompressed((IEnumerable<string>)new[] { null, "", "a/b", "A/B" });

            var expected = (IEnumerable<string>)new[] { null, "", "a/b" };
            var actual = sut.NotCompressedMimeTypes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RemoveCompressed_IEnumerable_CorrectCompressedMimeTypes()
        {
            var sut = new DefaultMimeTypeFilterConfig()
                .AddCompressed((IEnumerable<string>)new[] { null, "", "a/b", "c/d", "e/f" })
                .RemoveCompressed((IEnumerable<string>)new[] { null, "", "A/B", "e/f" });

            var expected = (IEnumerable<string>)new[] { "c/d" };
            var actual = sut.CompressedMimeTypes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RemoveNotCompressed_IEnumerable_CorrectNotCompressedMimeTypes()
        {
            var sut = new DefaultMimeTypeFilterConfig()
                .AddNotCompressed((IEnumerable<string>)new[] { null, "", "a/b", "c/d", "e/f" })
                .RemoveNotCompressed((IEnumerable<string>)new[] { null, "", "A/B", "e/f" });

            var expected = (IEnumerable<string>)new[] { "c/d" };
            var actual = sut.NotCompressedMimeTypes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ClearCompressed_NoCompressedMimeTypes()
        {
            var sut = new DefaultMimeTypeFilterConfig()
                .AddCompressed(null, "", "a/b")
                .AddNotCompressed(null, "", "c/d")
                .ClearCompressed();

            var expected = Enumerable.Empty<string>();
            var actual = sut.CompressedMimeTypes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ClearNotCompressed_NoNotCompressedMimeTypes()
        {
            var sut = new DefaultMimeTypeFilterConfig()
                .AddCompressed(null, "", "a/b")
                .AddNotCompressed(null, "", "c/d")
                .ClearNotCompressed();

            var expected = Enumerable.Empty<string>();
            var actual = sut.NotCompressedMimeTypes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ClearAll_NoCompressedMimeTypes_NoNotCompressedMimeTypes()
        {
            var sut = new DefaultMimeTypeFilterConfig()
                .AddCompressed(null, "", "a/b")
                .AddNotCompressed(null, "", "c/d")
                .ClearAll();

            var expected = Enumerable.Empty<string>();
            var actualCompressed = sut.CompressedMimeTypes;
            var actualNotCompressed = sut.NotCompressedMimeTypes;

            Assert.Equal(expected, actualCompressed);
            Assert.Equal(expected, actualNotCompressed);
        }

        [Theory]
        [InlineData(null, null, null, false)]
        [InlineData(null, null, new[] { null, "" }, false)]
        [InlineData(null, new[] { null, "" }, new string[0], true)]

        [InlineData("", null, null, false)]
        [InlineData("", null, new[] { null, "" }, false)]
        [InlineData("", new[] { null, "" }, new string[0], true)]

        [InlineData("a/b", null, null, false)]
        [InlineData("a/b", null, new[] { null, "", "a/b" }, false)]
        [InlineData("a/b", new[] { null, "", "a/b" }, new string[0], true)]
        public void ContainsCompressed_CorrectResult(string mimeType, string[] compressed, string[] notCompressed, bool expected)
        {
            var sut = new DefaultMimeTypeFilterConfig()
                .AddCompressed(compressed)
                .AddNotCompressed(notCompressed);

            var actual = sut.ContainsCompressed(mimeType);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, null, null, false)]
        [InlineData(null, null, new[] { null, "" }, true)]
        [InlineData(null, new[] { null, "" }, new string[0], false)]

        [InlineData("", null, null, false)]
        [InlineData("", null, new[] { null, "" }, true)]
        [InlineData("", new[] { null, "" }, new string[0], false)]

        [InlineData("a/b", null, null, false)]
        [InlineData("a/b", null, new[] { null, "", "a/b" }, true)]
        [InlineData("a/b", new[] { null, "", "a/b" }, new string[0], false)]
        public void ContainsNotCompressed_CorrectResult(string mimeType, string[] compressed, string[] notCompressed, bool expected)
        {
            var sut = new DefaultMimeTypeFilterConfig()
                .AddCompressed(compressed)
                .AddNotCompressed(notCompressed);

            var actual = sut.ContainsNotCompressed(mimeType);

            Assert.Equal(expected, actual);
        }
    }
}
