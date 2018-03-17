// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.ResponseCompression.Tests
{
    public class MimeTypeFilterTests
    {
        [Fact]
        public void NotPopulated_HasBeenPopulated_False()
        {
            var sut = new MimeTypeFilter();
            Assert.False(sut.HasBeenPopulated);
        }

        [Fact]
        public void AddCompressed_HasBeenPopulated_True()
        {
            var sut = new MimeTypeFilter();
            sut.AddCompressed("a/b");
            Assert.True(sut.HasBeenPopulated);
        }

        [Fact]
        public void AddNotCompressed_HasBeenPopulated_True()
        {
            var sut = new MimeTypeFilter();
            sut.AddNotCompressed("a/b");
            Assert.True(sut.HasBeenPopulated);
        }

        [Fact]
        public void RemoveCompressed_HasBeenPopulated_True()
        {
            var sut = new MimeTypeFilter();
            sut.RemoveCompressed("a/b");
            Assert.True(sut.HasBeenPopulated);
        }

        [Fact]
        public void RemoveNotCompressed_HasBeenPopulated_True()
        {
            var sut = new MimeTypeFilter();
            sut.RemoveNotCompressed("a/b");
            Assert.True(sut.HasBeenPopulated);
        }

        [Fact]
        public void ClearCompressed_HasBeenPopulated_True()
        {
            var sut = new MimeTypeFilter();
            sut.ClearCompressed();
            Assert.True(sut.HasBeenPopulated);
        }

        [Fact]
        public void ClearNotCompressed_HasBeenPopulated_True()
        {
            var sut = new MimeTypeFilter();
            sut.ClearNotCompressed();
            Assert.True(sut.HasBeenPopulated);
        }

        [Fact]
        public void ClearAll_HasBeenPopulated_True()
        {
            var sut = new MimeTypeFilter();
            sut.ClearAll();
            Assert.True(sut.HasBeenPopulated);
        }

        [Fact]
        public void AddCompressed_CorrectCompressedMimeTypes()
        {
            var sut = new MimeTypeFilter();
            sut.AddCompressed("a/b", "x/y", "A/B");

            var expected = (IEnumerable<string>)new[] { "a/b", "x/y" };
            var actual = sut.CompressedMimeTypes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddNotCompressed_CorrectNotCompressedMimeTypes()
        {
            var sut = new MimeTypeFilter();
            sut.AddNotCompressed("a/b", "x/y", "A/B");

            var expected = (IEnumerable<string>)new[] { "a/b", "x/y" };
            var actual = sut.NotCompressedMimeTypes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RemoveCompressed_CorrectCompressedMimeTypes()
        {
            var sut = new MimeTypeFilter();
            sut.AddCompressed("a/b", "c/d", "e/f", "g/h");
            sut.RemoveCompressed("A/B", "e/f");

            var expected = (IEnumerable<string>)new[] { "c/d", "g/h" };
            var actual = sut.CompressedMimeTypes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RemoveNotCompressed_CorrectNotCompressedMimeTypes()
        {
            var sut = new MimeTypeFilter();
            sut.AddNotCompressed("a/b", "c/d", "e/f", "g/h");
            sut.RemoveNotCompressed("A/B", "e/f");

            var expected = (IEnumerable<string>)new[] { "c/d", "g/h" };
            var actual = sut.NotCompressedMimeTypes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ClearCompressed_NoCompressedMimeTypes()
        {
            var sut = new MimeTypeFilter();
            sut.AddCompressed("a/b", "c/d");
            sut.AddNotCompressed("e/f", "g/h");
            sut.ClearCompressed();

            var expected = Enumerable.Empty<string>();
            var actual = sut.CompressedMimeTypes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ClearNotCompressed_NoNotCompressedMimeTypes()
        {
            var sut = new MimeTypeFilter();
            sut.AddCompressed("a/b", "c/d");
            sut.AddNotCompressed("e/f", "g/h");
            sut.ClearNotCompressed();

            var expected = Enumerable.Empty<string>();
            var actual = sut.NotCompressedMimeTypes;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ClearAll_NoCompressedMimeTypes_NoNotCompressedMimeTypes()
        {
            var sut = new MimeTypeFilter();
            sut.AddCompressed("a/b", "c/d");
            sut.AddNotCompressed("e/f", "g/h");
            sut.ClearAll();

            var expected = new string[0];
            var actualCompressed = sut.CompressedMimeTypes;
            var actualNotCompressed = sut.NotCompressedMimeTypes;

            Assert.Equal(expected, actualCompressed);
            Assert.Equal(expected, actualNotCompressed);
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("a/")]
        [InlineData("/b")]
        [InlineData("*/b")]
        [InlineData("a/b*")]
        [InlineData("a/*b")]
        [InlineData("a*/b")]
        [InlineData("*a/b")]
        [InlineData("a a/b")]
        [InlineData("a/b b")]
        public void InvalidMimeType_Exception(params string[] mimeTypes)
        {
            var sut = new MimeTypeFilter();
            Assert.Throws<ArgumentException>("mimeTypes", () => sut.AddCompressed(mimeTypes));
        }

        [Theory]
        [InlineData("a/b")]
        [InlineData("a/*")]
        [InlineData("*/*")]
        public void ValidMimeType_NoException(params string[] mimeTypes)
        {
            var sut = new MimeTypeFilter();
            var ex = Record.Exception(() => sut.AddCompressed(mimeTypes));
            Assert.Null(ex);
        }

        [Theory]
        [InlineData(null, null, false)]

        [InlineData(new string[0], new string[0], false)]
        [InlineData(new string[0], new[] { "a/b" }, false)]
        [InlineData(new string[0], new[] { "a/*" }, false)]
        [InlineData(new string[0], new[] { "*/*" }, false)]
        [InlineData(new string[0], new[] { "c/d" }, false)]

        [InlineData(new[] { "a/b" }, new string[0], true)]
        [InlineData(new[] { "a/b" }, new[] { "a/b" }, false)]
        [InlineData(new[] { "a/b" }, new[] { "a/*" }, true)]
        [InlineData(new[] { "a/b" }, new[] { "*/*" }, true)]
        [InlineData(new[] { "a/b" }, new[] { "c/d" }, true)]

        [InlineData(new[] { "a/*" }, new string[0], true)]
        [InlineData(new[] { "a/*" }, new[] { "a/b" }, false)]
        [InlineData(new[] { "a/*" }, new[] { "a/*" }, false)]
        [InlineData(new[] { "a/*" }, new[] { "*/*" }, true)]
        [InlineData(new[] { "a/*" }, new[] { "c/d" }, true)]

        [InlineData(new[] { "*/*" }, new string[0], true)]
        [InlineData(new[] { "*/*" }, new[] { "a/b" }, false)]
        [InlineData(new[] { "*/*" }, new[] { "a/*" }, false)]
        [InlineData(new[] { "*/*" }, new[] { "*/*" }, false)]
        [InlineData(new[] { "*/*" }, new[] { "c/d" }, true)]

        [InlineData(new[] { "c/d" }, new string[0], false)]
        [InlineData(new[] { "c/d" }, new[] { "a/b" }, false)]
        [InlineData(new[] { "c/d" }, new[] { "a/*" }, false)]
        [InlineData(new[] { "c/d" }, new[] { "*/*" }, false)]
        [InlineData(new[] { "c/d" }, new[] { "c/d" }, false)]
        public void ShouldCompress_CorrectResult(string[] compressed, string[] notCompressed, bool expected)
        {
            var sut = new MimeTypeFilter();
            sut.AddCompressed(compressed);
            sut.AddNotCompressed(notCompressed);

            var actual = sut.ShouldCompress("a/b");

            Assert.Equal(expected, actual);
        }
    }
}
