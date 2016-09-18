// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;
using System.Net;
using Xunit;

namespace Microsoft.AspNetCore.HttpOverrides
{
    public class IPNetworkTest
    {
        [Theory]
        [InlineData("10.1.1.0", 8, "10.1.1.10")]
        [InlineData("174.0.0.0", 7, "175.1.1.10")]
        [InlineData("10.174.0.0", 15, "10.175.1.10")]
        [InlineData("10.168.0.0", 14, "10.171.1.10")]
        public void Contains_Positive(string prefixText, int length, string addressText)
        {
            var network = new IPNetwork(IPAddress.Parse(prefixText), length);
            Assert.True(network.Contains(IPAddress.Parse(addressText)));
        }

        [Theory]
        [InlineData("10.1.0.0", 16, "10.2.1.10")]
        [InlineData("174.0.0.0", 7, "173.1.1.10")]
        [InlineData("10.174.0.0", 15, "10.173.1.10")]
        [InlineData("10.168.0.0", 14, "10.172.1.10")]
        public void Contains_Negative(string prefixText, int length, string addressText)
        {
            var network = new IPNetwork(IPAddress.Parse(prefixText), length);
            Assert.False(network.Contains(IPAddress.Parse(addressText)));
        }

        [Theory]
        [InlineData("10.2.1.10/16", "10.2.1.10", 16)]
        [InlineData("0", "0.0.0.0", 32)]
        [InlineData("0/1", "0.0.0.0", 1)]
        [InlineData("255", "0.0.0.255", 32)]
        [InlineData("256/3", "0.0.1.0", 3)]
        [InlineData("4294967295", "255.255.255.255", 32)]
        [InlineData("4294967295/31", "255.255.255.255", 31)]
        [InlineData("0.0.0.0", "0.0.0.0", 32)]
        [InlineData("10.0.0.0", "10.0.0.0", 32)]
        [InlineData("10.0.0.0/0", "10.0.0.0", 0)]
        [InlineData("10.0.0.0/1", "10.0.0.0", 1)]
        [InlineData("10.0.0.0/8", "10.0.0.0", 8)]
        [InlineData("10.0.0.0/31", "10.0.0.0", 31)]
        [InlineData("10.0.0.0/32", "10.0.0.0", 32)]
        [InlineData("0.0.0.0/0", "0.0.0.0", 0)]
        [InlineData("::1", "::1", 128)]
        [InlineData("::1/64", "::1", 64)]
        [InlineData("::a:b/64", "::0.10.0.11", 64)]
        [InlineData("::a:b", "::0.10.0.11", 128)]
        [InlineData("a:b::/64", "a:b::", 64)]
        [InlineData("a:b::", "a:b::", 128)]
        [InlineData("::/128", "::", 128)]
        [InlineData("::", "::", 128)]
        [InlineData("a:b:0000::c:0000:d/128", "a:b::c:0:d", 128)]
        public void ParseAndTryParse_ParsesCorrectly(string networkString, string expectedPrefix, int expectedLength)
        {
            IPNetwork network;

            // TryParse
            Assert.True(IPNetwork.TryParse(networkString, out network));
            Assert.Equal(expectedPrefix, network.Prefix.ToString());
            Assert.Equal(expectedLength, network.PrefixLength);

            // Parse
            network = IPNetwork.Parse(networkString);
            Assert.Equal(expectedPrefix, network.Prefix.ToString());
            Assert.Equal(expectedLength, network.PrefixLength);
        }

        [Theory]
        [InlineData("foo")]
        // TODO: This test fails on OS X
        // See https://github.com/dotnet/corefx/issues/11843
        [InlineData("4294967296")]
        [InlineData("10.0.0.0/88/99")]
        [InlineData("10a.0.0.0/8")]
        [InlineData("10.0.0.0/33")]
        [InlineData("10.0.0.0/255")]
        [InlineData("10.0.0.0/256")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("a")]
        [InlineData("g")]
        [InlineData("abcde::f")]
        [InlineData("a:b:0000::c:0000:d/129")]
        public void ParseAndTryParse_ShouldNotParse(string networkString)
        {
            IPNetwork network;

            // TryParse
            Assert.False(IPNetwork.TryParse(networkString, out network));
            Assert.Null(network);

            // Parse
            Assert.Throws<FormatException>(()=>IPNetwork.Parse(networkString));
        }
    }
}
