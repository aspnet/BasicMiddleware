﻿// Copyright (c) .NET Foundation. All rights reserved.
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNetCore.HttpsPolicy.Tests
{
    public class HttpsPolicyTests
    {
        [Theory]
        [InlineData(302, null, 2592000, false, false, "max-age=2592000", "https://localhost/")]
        [InlineData(301, 5050, 2592000, false, false, "max-age=2592000", "https://localhost:5050/")]
        [InlineData(301, 443, 2592000, false, false, "max-age=2592000", "https://localhost/")]
        [InlineData(301, 443, 2592000, true, false, "max-age=2592000; includeSubDomains", "https://localhost/")]
        [InlineData(301, 443, 2592000, false, true, "max-age=2592000; preload", "https://localhost/")]
        [InlineData(301, null, 2592000, true, true, "max-age=2592000; includeSubDomains; preload", "https://localhost/")]
        [InlineData(302, 5050, 2592000, true, true, "max-age=2592000; includeSubDomains; preload", "https://localhost:5050/")]
        public async Task SetsBothHstsAndHttpsRedirection_RedirectOnFirstRequest_HstsOnSecondRequest(int statusCode, int? tlsPort, int maxAge, bool includeSubDomains, bool preload, string expectedHstsHeader, string expectedUrl)
        {

            var builder = new WebHostBuilder()
                .UseUrls("https://*:5050", "http://*:5050")
                .ConfigureServices(services =>
                {
                    services.Configure<HttpsRedirectionOptions>(options =>
                    {
                        options.RedirectStatusCode = statusCode;
                        options.TlsPort = tlsPort;
                    });
                    services.Configure<HstsOptions>(options =>
                    {
                        options.IncludeSubDomains = includeSubDomains;
                        options.MaxAge = maxAge;
                        options.Preload = preload;
                    });
                })
                .Configure(app =>
                {
                    app.UseHttpsRedirection();
                    app.UseHsts();
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
