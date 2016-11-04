using System.Collections.Generic;
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
            string xml = $@"<rewrite>
                                <rewriteMaps>
                                    <rewriteMap name=""{expectedMapName}"">
                                        <add key=""{expectedKey}"" value=""{expectedValue}"" />
                                    </rewriteMap>
                                </rewriteMaps>
                            </rewrite>";

            // act
            XDocument xmlDoc = XDocument.Load(new StringReader(xml), LoadOptions.SetLineInfo);
            XElement xmlRoot = xmlDoc.Descendants(RewriteTags.Rewrite).FirstOrDefault();
            IDictionary<string, IISRewriteMap> actualMaps = new RewriteMapParser().Parse(xmlRoot);

            // assert
            Assert.Equal(1, actualMaps.Count);

            IISRewriteMap actualMap;
            Assert.True(actualMaps.TryGetValue(expectedMapName, out actualMap));
            Assert.NotNull(actualMap);
            Assert.Equal(expectedMapName, actualMap.Name);

            string actualValue;
            Assert.True(actualMap.TryGetEntry(expectedKey, out actualValue));
            Assert.Equal(expectedValue, actualValue);
        }
    }
}