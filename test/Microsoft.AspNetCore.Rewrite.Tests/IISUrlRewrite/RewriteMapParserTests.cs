﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite;

using Xunit;

namespace Microsoft.AspNetCore.Rewrite.Tests.IISUrlRewrite
{
    public class RewriteMapParserTests
    {
        [Fact]
        public void Should_parse_rewrite_map()
        {
            // arrange
            const string expectedMapName = "apiMap";
            const string expectedKey = "api.test.com";
            const string expectedValue = "test.com/api";
            var xml = $@"<rewrite>
                                <rewriteMaps>
                                    <rewriteMap name=""{expectedMapName}"">
                                        <add key=""{expectedKey}"" value=""{expectedValue}"" />
                                    </rewriteMap>
                                </rewriteMaps>
                            </rewrite>";

            // act
            var xmlDoc = XDocument.Load(new StringReader(xml), LoadOptions.SetLineInfo);
            var xmlRoot = xmlDoc.Descendants(RewriteTags.Rewrite).FirstOrDefault();
            var actualMaps = RewriteMapParser.Parse(xmlRoot);

            // assert
            Assert.Equal(1, actualMaps.Count);

            IISRewriteMap actualMap = actualMaps[expectedMapName];
            Assert.NotNull(actualMap);
            Assert.Equal(expectedMapName, actualMap.Name);
            Assert.Equal(expectedValue, actualMap[expectedKey]);
        }
    }
}