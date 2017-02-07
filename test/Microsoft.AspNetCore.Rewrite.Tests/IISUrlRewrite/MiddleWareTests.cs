// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNetCore.Rewrite.Tests.UrlRewrite
{
    public class MiddlewareTests
    {
        [Fact]
        public async Task Invoke_RedirectPathToPathAndQuery()
        {
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(@"<rewrite>
                <rules>
                <rule name=""Rewrite to article.aspx"">
                <match url = ""^article/([0-9]+)/([_0-9a-z-]+)"" />
                <action type=""Redirect"" url =""article.aspx?id={R:1}&amp;title={R:2}"" />
                </rule>
                </rules>
                </rewrite>"));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                    app.Run(context => context.Response.WriteAsync(context.Response.Headers[HeaderNames.Location]));
                });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("article/10/hey");

            Assert.Equal("/article.aspx?id=10&title=hey", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task Invoke_RewritePathToPathAndQuery()
        {
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(@"<rewrite>
                <rules>
                <rule name=""Rewrite to article.aspx"">
                <match url = ""^article/([0-9]+)/([_0-9a-z-]+)"" />
                <action type=""Rewrite"" url =""article.aspx?id={R:1}&amp;title={R:2}"" />
                </rule>
                </rules>
                </rewrite>"));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                    app.Run(context => context.Response.WriteAsync(context.Request.Path + context.Request.QueryString));
                });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetStringAsync("/article/10/hey");

            Assert.Equal("/article.aspx?id=10&title=hey", response);
        }

        [Fact]
        public async Task Invoke_RewriteBasedOnQueryStringParameters()
        {
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(@"<rewrite>
                <rules>
                <rule name=""Query String Rewrite"">
                <match url=""page\.asp$"" />
                <conditions>
                <add input=""{QUERY_STRING}"" pattern=""p1=(\d+)"" />
                <add input=""##{C:1}##_{QUERY_STRING}"" pattern=""##([^#]+)##_.*p2=(\d+)"" />
                </conditions>
                <action type=""Rewrite"" url=""newpage.aspx?param1={C:1}&amp;param2={C:2}"" appendQueryString=""false""/>
                </rule>
                </rules>
                </rewrite>"));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                    app.Run(context => context.Response.WriteAsync(context.Request.Path + context.Request.QueryString));
                });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetStringAsync("page.asp?p2=321&p1=123");

            Assert.Equal("/newpage.aspx?param1=123&param2=321", response);
        }

        [Fact]
        public async Task Invoke_RedirectToLowerCase()
        {
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(@"<rewrite>
                <rules>
                <rule name=""Convert to lower case"" stopProcessing=""true"">
                <match url="".*[A-Z].*"" ignoreCase=""false"" />
                <action type=""Redirect"" url=""{ToLower:{R:0}}"" redirectType=""Permanent"" />
                </rule>
                </rules>
                </rewrite>"));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                    app.Run(context => context.Response.WriteAsync(context.Response.Headers[HeaderNames.Location]));
                });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("HElLo");

            Assert.Equal("/hello", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task Invoke_RedirectRemoveTrailingSlash()
        {
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(@"<rewrite>
                <rules>
                <rule name=""Remove trailing slash"" stopProcessing=""true"">
                <match url=""(.*)/$"" />
                <conditions>
                <add input=""{REQUEST_FILENAME}"" matchType=""IsFile"" negate=""true"" />
                <add input=""{REQUEST_FILENAME}"" matchType=""IsDirectory"" negate=""true"" />
                </conditions>
                <action type=""Redirect"" redirectType=""Permanent"" url=""{R:1}"" />
                </rule>
                </rules>
                </rewrite>"));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("hey/hello/");

            Assert.Equal("/hey/hello", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task Invoke_RedirectAddTrailingSlash()
        {
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(@"<rewrite>
                <rules>
                <rule name=""Add trailing slash"" stopProcessing=""true"">
                <match url=""(.*[^/])$"" />
                <conditions>
                <add input=""{REQUEST_FILENAME}"" matchType=""IsFile"" negate=""true"" />
                <add input=""{REQUEST_FILENAME}"" matchType=""IsDirectory"" negate=""true"" />
                </conditions>
                <action type=""Redirect"" redirectType=""Permanent"" url=""{R:1}/"" />
                </rule>
                </rules>
                </rewrite>"));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("hey/hello");

            Assert.Equal("/hey/hello/", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task Invoke_RedirectToHttps()
        {
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(@"<rewrite>
                <rules>
                <rule name=""Redirect to HTTPS"" stopProcessing=""true"">
                <match url=""(.*)"" />
                <conditions>
                <add input=""{HTTPS}"" pattern=""^OFF$"" />
                </conditions>
                <action type=""Redirect"" url=""https://{HTTP_HOST}/{R:1}"" redirectType=""Permanent"" />
                </rule>
                </rules>
                </rewrite>"));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync(new Uri("http://example.com"));

            Assert.Equal("https://example.com/", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task Invoke_RewriteToHttps()
        {
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(@"<rewrite>
                <rules>
                <rule name=""Rewrite to HTTPS"" stopProcessing=""true"">
                <match url=""(.*)"" />
                <conditions>
                <add input=""{HTTPS}"" pattern=""^OFF$"" />
                </conditions>
                <action type=""Rewrite"" url=""https://{HTTP_HOST}/{R:1}"" />
                </rule>
                </rules>
                </rewrite>"));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                    app.Run(context => context.Response.WriteAsync(
                        context.Request.Scheme +
                        "://" +
                        context.Request.Host +
                        context.Request.Path +
                        context.Request.QueryString));
                });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetStringAsync(new Uri("http://example.com"));

            Assert.Equal("https://example.com/", response);
        }

        [Fact]
        public async Task Invoke_ReverseProxyToAnotherSite()
        {
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(@"<rewrite>
                <rules>
                <rule name=""Proxy"">
                <match url=""(.*)"" />
                <action type=""Rewrite"" url=""http://internalserver/{R:1}"" />
                </rule>
                </rules>
                </rewrite>"));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                    app.Run(context => context.Response.WriteAsync(
                        context.Request.Scheme +
                        "://" +
                        context.Request.Host +
                        context.Request.Path +
                        context.Request.QueryString));
                });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetStringAsync(new Uri("http://example.com/"));

            Assert.Equal("http://internalserver/", response);
        }

        [Fact]
        public async Task Invoke_CaptureEmptyStringInRegexAssertRedirectLocationHasForwardSlash()
        {
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(@"<rewrite>
                <rules>
                <rule name=""Test"">
                <match url=""(.*)"" />
                <action type=""Redirect"" url=""{R:1}"" />
                </rule>
                </rules>
                </rewrite>"));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                    app.Run(context => context.Response.WriteAsync(
                        context.Request.Scheme +
                        "://" +
                        context.Request.Host +
                        context.Request.Path +
                        context.Request.QueryString));
                });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync(new Uri("http://example.com/"));

            Assert.Equal("/", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task Invoke_CaptureEmptyStringInRegexAssertRewriteLocationHasForwardSlash()
        {
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(@"<rewrite>
                <rules>
                <rule name=""Test"">
                <match url=""(.*)"" />
                <action type=""Rewrite"" url=""{R:1}"" />
                </rule>
                </rules>
                </rewrite>"));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                    app.Run(context => context.Response.WriteAsync(
                        context.Request.Path +
                        context.Request.QueryString));
                });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetStringAsync(new Uri("http://example.com/"));

            Assert.Equal("/", response);
        }

        [Fact]
        public async Task Invoke_CaptureEmptyStringInRegexAssertLocationHeaderContainsPathBase()
        {
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(@"<rewrite>
                <rules>
                <rule name=""Test"">
                <match url=""(.*)"" />
                <action type=""Redirect"" url=""{R:1}"" />
                </rule>
                </rules>
                </rewrite>"));
            var builder = new WebHostBuilder()
            .Configure(app =>
            {
                app.UseRewriter(options);
                app.Run(context => context.Response.WriteAsync(
                        context.Request.Path +
                        context.Request.QueryString));
            });
            var server = new TestServer(builder) { BaseAddress = new Uri("http://localhost:5000/foo") };

            var response = await server.CreateClient().GetAsync("");

            Assert.Equal(response.Headers.Location.OriginalString, "/foo");
        }

        [Theory]
        [InlineData("IsFile")]
        [InlineData("isfile")]
        [InlineData("IsDirectory")]
        [InlineData("isdirectory")]
        public async Task VerifyIsFileAndIsDirectoryParsing(string matchType)
        {
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader($@"<rewrite>
                <rules>
                <rule name=""Test"">
                <match url=""(.*[^/])$"" />
                <conditions>
                <add input=""{{REQUEST_FILENAME}}"" matchType=""{matchType}"" negate=""true""/>
                </conditions>
                <action type=""Redirect"" url=""{{R:1}}/"" />
                </rule>
                </rules>
                </rewrite>"));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("hey/hello");

            Assert.Equal("/hey/hello/", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task VerifyTrackAllCaptures()
        {
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(@"<rewrite>
                <rules>
                <rule name=""Test"">
                <match url=""(.*)"" ignoreCase=""false"" />
                <conditions trackAllCaptures = ""true"" >
                <add input=""{REQUEST_URI}"" pattern=""^/([a-zA-Z]+)/([0-9]+)$"" />
                <add input=""{QUERY_STRING}"" pattern=""p2=([a-z]+)"" />
                </conditions>
                <action type=""Redirect"" url =""blogposts/{C:1}/{C:4}"" />
                <!--rewrite action uses back - references to both conditions -->
                </rule>
                </rules>
                </rewrite>"));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("article/23?p1=123&p2=abc");

            Assert.Equal("/blogposts/article/abc", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task VerifyTrackAllCapturesRuleAndConditionCapture()
        {
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(@"<rewrite>
                <rules>
                <rule name=""Test"">
                <match url=""(.*)"" ignoreCase=""false"" />
                <conditions trackAllCaptures = ""true"" >
                <add input=""{REQUEST_URI}"" pattern=""^/([a-zA-Z]+)/([0-9]+)$"" />
                <add input=""{QUERY_STRING}"" pattern=""p2=([a-z]+)"" />
                </conditions>
                <action type=""Redirect"" url =""blog/{R:0}/{C:4}"" />
                <!--rewrite action uses back - references to both conditions -->
                </rule>
                </rules>
                </rewrite>"));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("article/23?p1=123&p2=abc");

            Assert.Equal("/blog/article/23/abc", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task ThrowIndexOutOfRangeExceptionWithCorrectMessage()
        {
            // Arrange, Act, Assert
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(@"<rewrite>
                <rules>
                <rule name=""Test"">
                <match url=""(.*)"" ignoreCase=""false"" />
                <conditions trackAllCaptures = ""true"" >
                <add input=""{REQUEST_URI}"" pattern=""^/([a-zA-Z]+)/([0-9]+)$"" />
                <add input=""{QUERY_STRING}"" pattern=""p2=([a-z]+)"" />
                </conditions>
                <action type=""Redirect"" url =""blog/{R:0}/{C:9}"" />
                <!--rewrite action uses back - references to both conditions -->
                </rule>
                </rules>
                </rewrite>"));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                });
            var server = new TestServer(builder);

            var ex = await Assert.ThrowsAsync<IndexOutOfRangeException>(() => server.CreateClient().GetAsync("article/23?p1=123&p2=abc"));

            Assert.Equal("Cannot access back reference at index 9. Only 5 back references were captured.", ex.Message);
        }

        [Fact]
        public async Task Invoke_GlobalRuleConditionMatchesAgainstFullUri()
        {
            var xml = @"<rewrite>
                            <globalRules>
                                <rule name=""Test"" patternSyntax=""ECMAScript"" stopProcessing=""true"">
                                    <match url="".*"" />
                                    <conditions logicalGrouping=""MatchAll"" trackAllCaptures=""false"">
                                        <add input=""{REQUEST_URI}"" pattern=""^http://localhost/([0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12})(/.*)"" />
                                    </conditions>
                                    <action type=""Rewrite"" url=""http://www.test.com{C:2}"" />
                                </rule>
                            </globalRules>
                        </rewrite>";
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(xml));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                    app.Run(context => context.Response.WriteAsync(context.Request.GetEncodedUrl()));
                });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetStringAsync($"http://localhost/{Guid.NewGuid()}/foo/bar");

            Assert.Equal("http://www.test.com/foo/bar", response);
        }

        [Theory]
        [InlineData("http://fetch.environment.local/dev/path", "http://1.1.1.1/path")]
        [InlineData("http://fetch.environment.local/qa/path", "http://fetch.environment.local/qa/path")]
        public async Task Invoke_ReverseProxyToAnotherSiteUsingXmlConfiguredRewriteMap(string requestUri, string expectedRewrittenUri)
        {
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(@"
                <rewrite>
                    <rules>
                        <rule name=""Proxy"">
                            <match url=""([^/]*)(/?.*)"" />
                            <conditions>
                                <add input=""{environmentMap:{R:1}}"" pattern=""(.+)"" />
                            </conditions>
                            <action type=""Rewrite"" url=""http://{C:1}{R:2}"" appendQueryString=""true"" />
                        </rule>
                    </rules>
                    <rewriteMaps>
                        <rewriteMap name=""environmentMap"">
                            <add key=""dev"" value=""1.1.1.1"" />
                        </rewriteMap>
                    </rewriteMaps>
                </rewrite>"));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                    app.Run(context => context.Response.WriteAsync(context.Request.GetEncodedUrl()));
                });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetStringAsync(new Uri(requestUri));

            Assert.Equal(expectedRewrittenUri, response);
        }

        [Fact]
        public async Task Invoke_RewriteShouldSupportCustomRequestHeadersUsingStandardServerVariables()
        {
            var actionType = ActionType.None;
            var xml = $@"<rewrite>
                            <rules>
                                <rule name=""clientIpAddress"" patternSyntax=""ECMAScript"" stopProcessing=""false"">
                                    <match url="".*"" />
                                    <serverVariables>
                                        <set name=""HTTP_Custom_ClientIpAddress"" value=""{{REMOTE_ADDR}}"" />
                                    </serverVariables>
                                    <action type=""{actionType}"" />
                                </rule>
                            </rules>
                        </rewrite>";
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(xml));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Use((context, next) =>
                    {
                        // TestServer doesn't set RemoteIpAddress so do it inline
                        context.Connection.RemoteIpAddress = IPAddress.Loopback;
                        return next();
                    });
                    app.UseRewriter(options);
                    app.Run(context =>
                    {
                        StringValues values;
                        context.Response.HttpContext.Request.Headers.TryGetValue("Custom-ClientIpAddress", out values);
                        return context.Response.WriteAsync(values);
                    });
                });
            var server = new TestServer(builder);
            var response = await server.CreateClient().GetStringAsync("/article/10/hey");
            Assert.Equal(IPAddress.Loopback, IPAddress.Parse(response));
        }

        [Fact]
        public async Task Invoke_RewriteShouldSupportCustomRequestHeadersUsingRuleBackReference()
        {
            var actionType = ActionType.Rewrite;
            var xml = $@"<rewrite>
                            <rules>
                                <rule name=""tenantId"" patternSyntax=""ECMAScript"" stopProcessing=""false"">
                                    <match url=""(.*)"" />
                                    <serverVariables>
                                        <set name=""HTTP_OriginalUri"" value=""{{R:0}}"" />
                                    </serverVariables>
                                    <action type=""{actionType}"" url=""http://{{HTTP_HOST}}{{REQUEST_URI}}"" />
                                </rule>
                            </rules>
                        </rewrite>";
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(xml));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                    app.Run(context =>
                    {
                        /*
                            client will not see added inbound request headers
                            need to assert header existence by returning the header value as the response string
                        */
                        var headerValue = context.Response.HttpContext.Request.Headers["OriginalUri"];
                        return context.Response.WriteAsync(headerValue);
                    });
                });
            var server = new TestServer(builder);
            const string requestPath = "article/10/hey";
            var response = await server.CreateClient().GetStringAsync(requestPath);
            Assert.Equal(requestPath, response);
        }

        [Fact]
        public async Task Invoke_RewriteShouldSupportCustomRequestHeadersUsingConditionBackReference()
        {
            var actionType = ActionType.Rewrite;
            var xml = $@"<rewrite>
                            <rules>
                                <rule name=""tenantId"" patternSyntax=""ECMAScript"" stopProcessing=""false"">
                                    <match url="".*"" />
                                        <conditions logicalGrouping=""MatchAll"" trackAllCaptures=""false"">
                                            <add input=""{{REQUEST_URI}}"" pattern=""^/([0-9a-fA-F]{{8}}\-[0-9a-fA-F]{{4}}\-[0-9a-fA-F]{{4}}\-[0-9a-fA-F]{{4}}\-[0-9a-fA-F]{{12}})(/.*)$"" />
                                        </conditions>
                                        <serverVariables>
                                            <set name=""HTTP_TenantId"" value=""{{C:1}}"" />
                                        </serverVariables>
                                    <action type=""{actionType}"" url=""http://{{HTTP_HOST}}/{{C:2}}"" />
                                </rule>
                            </rules>
                        </rewrite>";
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(xml));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                    app.Run(context =>
                    {
                        /*
                            client will not see added inbound request headers
                            need to assert header existence by returning the header value as the response string
                        */
                        var headerValue = context.Response.HttpContext.Request.Headers["TenantId"];
                        return context.Response.WriteAsync(headerValue);
                    });
                });
            var server = new TestServer(builder);
            var tenantId = Guid.NewGuid();
            var response = await server.CreateClient().GetStringAsync($"{tenantId}/article/10/hey");
            Assert.Equal(tenantId.ToString(), response);
        }

        [Fact]
        public async Task Invoke_RewriteShouldSupportCustomRequestHeadersUsingLiteral()
        {
            var actionType = ActionType.Rewrite;
            var customValue = "customValue";
            var xml = $@"<rewrite>
                            <rules>
                                <rule name=""customValue"" patternSyntax=""ECMAScript"" stopProcessing=""false"">
                                    <match url="".*"" />
                                    <serverVariables>
                                        <set name=""HTTP_CustomValue"" value=""{customValue}"" />
                                    </serverVariables>
                                    <action type=""{actionType}"" url=""http://{{HTTP_HOST}}{{REQUEST_URI}}"" />
                                </rule>
                            </rules>
                        </rewrite>";
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(xml));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                    app.Run(context =>
                    {
                        /*
                            client will not see added inbound request headers
                            need to assert header existence by returning the header value as the response string
                        */
                        var headerValue = context.Response.HttpContext.Request.Headers["CustomValue"];
                        return context.Response.WriteAsync(headerValue);
                    });
                });
            var server = new TestServer(builder);
            var response = await server.CreateClient().GetStringAsync("article/10/hey");
            Assert.Equal(customValue, response);
        }

        [Fact]
        public async Task Invoke_RewriteShouldSupportCustomResponseHeadersUsingStandardServerVariables()
        {
            var actionType = ActionType.None;
            var xml = $@"<rewrite>
                            <rules>
                                <rule name=""clientIpAddress"" patternSyntax=""ECMAScript"" stopProcessing=""false"">
                                    <match url="".*"" />
                                    <serverVariables>
                                        <set name=""RESPONSE_Custom_ClientIpAddress"" value=""{{REMOTE_ADDR}}"" />
                                    </serverVariables>
                                    <action type=""{actionType}"" />
                                </rule>
                            </rules>
                        </rewrite>";
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(xml));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Use((context, next) =>
                    {
                        // TestServer doesn't set RemoteIpAddress so do it inline
                        context.Connection.RemoteIpAddress = IPAddress.Loopback;
                        return next();
                    });
                    app.UseRewriter(options);
                    app.Run(context => context.Response.WriteAsync(context.Request.GetEncodedUrl()));
                });
            var server = new TestServer(builder);
            var response = await server.CreateClient().GetAsync("/article/10/hey");
            IEnumerable<string> headerValues;
            response.Headers.TryGetValues("Custom-ClientIpAddress", out headerValues);
            Assert.Equal(IPAddress.Loopback, IPAddress.Parse(headerValues.Single()));
        }

        [Fact]
        public async Task Invoke_RewriteShouldSupportCustomResponseHeadersUsingRuleBackReference()
        {
            var actionType = ActionType.Rewrite;
            var xml = $@"<rewrite>
                            <rules>
                                <rule name=""tenantId"" patternSyntax=""ECMAScript"" stopProcessing=""false"">
                                    <match url=""(.*)"" />
                                    <serverVariables>
                                        <set name=""RESPONSE_OriginalUri"" value=""{{R:0}}"" />
                                    </serverVariables>
                                    <action type=""{actionType}"" url=""http://{{HTTP_HOST}}{{REQUEST_URI}}"" />
                                </rule>
                            </rules>
                        </rewrite>";
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(xml));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                    app.Run(context => context.Response.WriteAsync(context.Request.GetEncodedUrl()));
                });
            var server = new TestServer(builder);
            const string requestPath = "article/10/hey";
            var response = await server.CreateClient().GetAsync(requestPath);
            IEnumerable<string> headerValues;
            var headerExists = response.Headers.TryGetValues("OriginalUri", out headerValues);
            Assert.True(headerExists);
            Assert.Equal(requestPath, headerValues.Single());
        }

        [Fact]
        public async Task Invoke_RewriteShouldSupportCustomResponseHeadersUsingConditionBackReference()
        {
            var actionType = ActionType.Rewrite;
            var xml = $@"<rewrite>
                            <rules>
                                <rule name=""tenantId"" patternSyntax=""ECMAScript"" stopProcessing=""false"">
                                    <match url="".*"" />
                                        <conditions logicalGrouping=""MatchAll"" trackAllCaptures=""false"">
                                            <add input=""{{REQUEST_URI}}"" pattern=""^/([0-9a-fA-F]{{8}}\-[0-9a-fA-F]{{4}}\-[0-9a-fA-F]{{4}}\-[0-9a-fA-F]{{4}}\-[0-9a-fA-F]{{12}})(/.*)$"" />
                                        </conditions>
                                        <serverVariables>
                                            <set name=""RESPONSE_TenantId"" value=""{{C:1}}"" />
                                        </serverVariables>
                                    <action type=""{actionType}"" url=""http://{{HTTP_HOST}}/{{C:2}}"" />
                                </rule>
                            </rules>
                        </rewrite>";
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(xml));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                    app.Run(context => context.Response.WriteAsync(context.Request.GetEncodedUrl()));
                });
            var server = new TestServer(builder);
            var tenantId = Guid.NewGuid();
            var response = await server.CreateClient().GetAsync($"{tenantId}/article/10/hey");
            IEnumerable<string> headerValues;
            var headerExists = response.Headers.TryGetValues("TenantId", out headerValues);
            Assert.True(headerExists);
            Assert.Equal(tenantId.ToString(), headerValues.Single());
        }

        [Fact]
        public async Task Invoke_RewriteShouldSupportCustomResponseHeadersUsingLiteral()
        {
            var actionType = ActionType.Rewrite;
            var customValue = "customValue";
            var xml = $@"<rewrite>
                            <rules>
                                <rule name=""customValue"" patternSyntax=""ECMAScript"" stopProcessing=""false"">
                                    <match url="".*"" />
                                    <serverVariables>
                                        <set name=""RESPONSE_CustomValue"" value=""{customValue}"" />
                                    </serverVariables>
                                    <action type=""{actionType}"" url=""http://{{HTTP_HOST}}{{REQUEST_URI}}"" />
                                </rule>
                            </rules>
                        </rewrite>";
            var options = new RewriteOptions().AddIISUrlRewrite(new StringReader(xml));
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseRewriter(options);
                    app.Run(context => context.Response.WriteAsync(context.Request.GetEncodedUrl()));
                });
            var server = new TestServer(builder);
            var response = await server.CreateClient().GetAsync("article/10/hey");
            IEnumerable<string> headerValues;
            var headerExists = response.Headers.TryGetValues("CustomValue", out headerValues);
            Assert.True(headerExists);
            Assert.Equal(customValue, headerValues.Single());
        }
    }
}
