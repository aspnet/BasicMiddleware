using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Rewrite.UrlRewrite;
using Xunit;
namespace Microsoft.AspNetCore.Rewrite.Tests.UrlRewrite
{
    public class FileParserTests
    {
        [Fact]
        public void RuleParse_ParseTypicalRule()
        {
            var xml = @"<rewrite>
                            <rules>
                                <rule name=""Rewrite to article.aspx"">
                                    <match url = ""^article/([0-9]+)/([_0-9a-z-]+)"" />
                                    <action type=""Rewrite"" url =""article.aspx?id={R:1}&amp;title={R:2}"" />
                                </rule>
                            </rules>
                        </rewrite>";
            var res = FileParser.Parse(new StringReader(xml));

        }
    }
}
