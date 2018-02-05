using System;
using System.Threading.Tasks;
using HostFilteringSample;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNetCore.HostFiltering
{
    public class HostFilteringMiddlewareTests
    {
        [Fact]
        public void MissingConfigThrows()
        {
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseMiddleware<HostFilteringMiddleware>();
                });
            Assert.Throws<InvalidOperationException>(() => new TestServer(builder));
        }

        [Fact]
        public async Task AllowsMissingHost()
        {
            var builder = new WebHostBuilder()
                .UseSetting("AllowedHosts", "localhost")
                .Configure(app =>
                {
                    app.Use((ctx, next) =>
                    {
                        ctx.Request.Headers.Remove(HeaderNames.Host);
                        return next();
                    });
                    app.UseMiddleware<HostFilteringMiddleware>();
                    app.Run(c =>
                    {
                        Assert.False(c.Request.Headers.TryGetValue(HeaderNames.Host, out var host));
                        return Task.CompletedTask;
                    });
                });
            var server = new TestServer(builder);
            var response = await server.CreateClient().GetAsync("/");
            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task AllowsEmptyHost()
        {
            var builder = new WebHostBuilder()
                .UseSetting("AllowedHosts", "localhost")
                .Configure(app =>
                {
                    app.Use((ctx, next) =>
                    {
                        ctx.Request.Headers[HeaderNames.Host] = " ";
                        return next();
                    });
                    app.UseMiddleware<HostFilteringMiddleware>();
                    app.Run(c =>
                    {
                        Assert.True(c.Request.Headers.TryGetValue(HeaderNames.Host, out var host));
                        Assert.True(StringValues.Equals(" ", host));
                        return Task.CompletedTask;
                    });
                    app.Run(c => Task.CompletedTask);
                });
            var server = new TestServer(builder);
            var response = await server.CreateClient().GetAsync("/");
            Assert.Equal(200, (int)response.StatusCode);
        }

        [Theory]
        [InlineData("localHost", "localhost")]
        [InlineData("localhost:9090", "example.com;localHost")]
        [InlineData("example.com:443", "example.com;localhost")]
        [InlineData("localHost:80", "localhost;")]
        [InlineData("foo.eXample.com:443", "*.exampLe.com")]
        [InlineData("127.0.0.1", "127.0.0.1")]
        [InlineData("127.0.0.1:443", "127.0.0.1")]
        [InlineData("[::ABC]", "[::aBc]")]
        [InlineData("[::1]:80", "[::1]")]
        public async Task AllowsSpecifiedHost(string host, string allowedHost)
        {
            var builder = new WebHostBuilder()
                .UseSetting("AllowedHosts", allowedHost)
                .Configure(app =>
                {
                    app.Use((ctx, next) =>
                    {
                        // TestHost's ClientHandler doesn't let you set the host header, only the host in the URI
                        // and that would over-normalize some of our test conditions like casing.
                        ctx.Request.Headers[HeaderNames.Host] = host;
                        return next();
                    });
                    app.UseMiddleware<HostFilteringMiddleware>();
                    app.Run(c => Task.CompletedTask);
                });
            var server = new TestServer(builder);
            var response = await server.CreateRequest("/").GetAsync();
            Assert.Equal(200, (int)response.StatusCode);
        }

        [Theory]
        [InlineData("example.com", "localhost")]
        [InlineData("localhost:9090", "example.com;")]
        [InlineData(";", "example.com;localhost")]
        [InlineData(";:80", "example.com;localhost")]
        [InlineData(":80", "localhost")]
        [InlineData(":", "localhost")]
        [InlineData("example.com:443", "*.example.com")]
        [InlineData("foo.com:443", "*.example.com")]
        [InlineData("foo.example.com.bar:443", "*.example.com")]
        [InlineData(".com:443", "*.com")]
        [InlineData("[::1", "[::1]")]
        [InlineData("[::1:80", "[::1]")]
        public async Task RejectsMismatchedHosts(string host, string allowedHost)
        {
            var builder = new WebHostBuilder()
                .UseSetting("AllowedHosts", allowedHost)
                .Configure(app =>
                {
                    app.Use((ctx, next) =>
                    {
                        // TestHost's ClientHandler doesn't let you set the host header, only the host in the URI
                        // and that would reject some of our test conditions.
                        ctx.Request.Headers[HeaderNames.Host] = host;
                        return next();
                    });
                    app.UseMiddleware<HostFilteringMiddleware>();
                    app.Run(c => throw new NotImplementedException("App"));
                });
            var server = new TestServer(builder);
            var response = await server.CreateRequest("/").GetAsync();
            Assert.Equal(400, (int)response.StatusCode);
        }
    }
}
