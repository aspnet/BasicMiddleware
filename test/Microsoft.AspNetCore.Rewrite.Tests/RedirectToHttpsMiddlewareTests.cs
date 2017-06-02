// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Options.Infrastructure;
using Xunit;

namespace Microsoft.AspNetCore.Rewrite.Tests.RedirectToHttps
{
    public class RedirectToHttpsMiddlewareTests
    {
        [Fact]
        public async Task EnsureRequestIsRedirectedToHttpsDefaultPortAndStatusCode()
        {
            var builder = new WebHostBuilder()
            .Configure(app =>
            {
                app.UseRedirectToHttps();
            })

            .ConfigureServices(services =>
            {
                services.AddTransient<IConfigureOptions<RedirectToHttpsOptions>, ConfigureDefaults<RedirectToHttpsOptions>>();
                services.AddRedirectToHttps();
            });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync(new Uri("http://example.com:1234"));

            Assert.Equal("https://example.com:443/", response.Headers.Location.OriginalString);
            Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        }

        [Fact]
        public async Task EnsureRequestIsRedirectedToHttpsReadPortFromConfig()
        {
            var builder = new WebHostBuilder()
            .Configure(app =>
            {
                app.UseRedirectToHttps();
            })
            .ConfigureAppConfiguration((builderCtx, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Microsoft:AspNetCore:RedirectToHttps:Port"] = "5001"
                });
            })
            .ConfigureServices(services =>
            {
                services.AddTransient<IConfigureOptions<RedirectToHttpsOptions>, ConfigureDefaults<RedirectToHttpsOptions>>();
                services.AddRedirectToHttps();
            });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync(new Uri("http://example.com:1234"));

            Assert.Equal("https://example.com:5001/", response.Headers.Location.OriginalString);
            Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        }

        [Fact]
        public async Task EnsureRequestIsRedirectedToHttpsReadStatusCodeFromOptions()
        {
            var builder = new WebHostBuilder()
            .Configure(app =>
            {
                var options = new RedirectToHttpsOptions();
                options.Port = 5001;
                options.StatusCode = StatusCodes.Status307TemporaryRedirect;
                app.UseRedirectToHttps(options);
            })
            .ConfigureServices(services =>
            {
                services.AddTransient<IConfigureOptions<RedirectToHttpsOptions>, ConfigureDefaults<RedirectToHttpsOptions>>();
                services.AddRedirectToHttps();
            });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync(new Uri("http://example.com:1234"));

            Assert.Equal("https://example.com:5001/", response.Headers.Location.OriginalString);
            Assert.Equal(StatusCodes.Status307TemporaryRedirect, (int)response.StatusCode);
        }

        [Fact]
        public async Task EnsureRequestIsRedirectedToHttpsReadStatusCodeFromConfig()
        {
            var builder = new WebHostBuilder()
            .Configure(app =>
            {
                app.UseRedirectToHttps();
            })
            .ConfigureAppConfiguration((builderCtx, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Microsoft:AspNetCore:RedirectToHttps:StatusCode"] = "301"
                });
            })
            .ConfigureServices(services =>
            {
                services.AddTransient<IConfigureOptions<RedirectToHttpsOptions>, ConfigureDefaults<RedirectToHttpsOptions>>();
                services.AddRedirectToHttps();
            });
            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync(new Uri("http://example.com:1234"));

            Assert.Equal("https://example.com:443/", response.Headers.Location.OriginalString);
            Assert.Equal(StatusCodes.Status301MovedPermanently, (int)response.StatusCode);
        }


    }
}
