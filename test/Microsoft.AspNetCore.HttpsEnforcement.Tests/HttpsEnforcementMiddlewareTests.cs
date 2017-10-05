// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNetCore.HttpsPolicy.Tests
{
    public class HttpsPolicyMiddlewareTests
    {
        [Fact]
        public async Task SetOptions_DefaultsSetCorrectly()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                })
                .Configure(app =>
                {
                    app.UseHttpsPolicy();
                    app.Run(context =>
                    {
                        return context.Response.WriteAsync("Hello world");
                    });
                });

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "");

            var response = await client.SendAsync(request);

            // By default, we will not redirect if we only use the Hsts Middleware.
            Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
            Assert.Equal("https://localhost/", response.Headers.Location.ToString());
        }

        [Theory]
        [InlineData(301, null, "https://localhost/")]
        [InlineData(302, null, "https://localhost/")]
        [InlineData(307, null, "https://localhost/")]
        [InlineData(308, null, "https://localhost/")]
        [InlineData(301, 5050, "https://localhost:5050/")]
        [InlineData(301, 443, "https://localhost/")]
        public async Task SetOptions_SetStatusCodeTlsPort(int statusCode, int? tlsPort, string expected)
        {
            var httpsEnforcementOptions = new HttpsPolicyOptions
            {
                StatusCode = statusCode,
                TlsPort = tlsPort
            };
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                })
                .Configure(app =>
                {
                    app.UseHttpsPolicy(httpsEnforcementOptions);
                    app.Run(context =>
                    {
                        return context.Response.WriteAsync("Hello world");
                    });
                });

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "");

            var response = await client.SendAsync(request);

            // By default, we will not redirect if we only use the Hsts Middleware.
            Assert.Equal(statusCode, (int)response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }


        [Theory]
        [InlineData(302, null, 0, false, false, "max-age=0", "https://localhost/")]
        [InlineData(301, 5050, 0, false, false, "max-age=0", "https://localhost:5050/")]
        [InlineData(301, 443, 0, false, false, "max-age=0", "https://localhost/")]
        [InlineData(301, 443, 0, true, false, "max-age=0; includeSubDomains", "https://localhost/")]
        [InlineData(301, 443, 0, false, true, "max-age=0; preload", "https://localhost/")]
        [InlineData(301, null, 0, true, true, "max-age=0; includeSubDomains; preload", "https://localhost/")]
        [InlineData(302, 5050, 0, true, true, "max-age=0; includeSubDomains; preload", "https://localhost:5050/")]
        public async Task SetOptions_UseHstsMiddleware(int statusCode, int? tlsPort, int maxAge, bool includeSubDomains, bool preload, string expectedHstsHeader, string expectedUrl)
        {
            var httpsEnforcementOptions = new HttpsPolicyOptions
            {
                StatusCode = statusCode,
                TlsPort = tlsPort
            };
            httpsEnforcementOptions.SetHsts = true;
            httpsEnforcementOptions.HstsOptions.MaxAge = maxAge;
            httpsEnforcementOptions.HstsOptions.IncludeSubDomains = includeSubDomains;
            httpsEnforcementOptions.HstsOptions.Preload = preload;

            var builder = new WebHostBuilder()
                .UseUrls("https://*:5050", "http://*:5050")
                .ConfigureServices(services =>
                {
                })
                .Configure(app =>
                {
                    app.UseHttpsPolicy(httpsEnforcementOptions);
                    app.Run(context =>
                    {
                        return context.Response.WriteAsync("Hello world");
                    });
                });

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "");

            var response = await client.SendAsync(request);

            Assert.Equal(statusCode, (int)response.StatusCode);
            Assert.Equal(expectedUrl, response.Headers.Location.ToString());

            client = server.CreateClient();
            client.BaseAddress = new Uri(response.Headers.Location.ToString());
            request = new HttpRequestMessage(HttpMethod.Get, "");
            response = await client.SendAsync(request);

            Assert.Equal(expectedHstsHeader, response.Headers.GetValues(HeaderNames.StrictTransportSecurity).FirstOrDefault());
        }
    }
}
