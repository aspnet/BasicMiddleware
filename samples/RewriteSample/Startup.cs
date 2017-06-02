// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Options.Infrastructure;

namespace RewriteSample
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseRedirectToHttps();

            var rewriteOptions = new RewriteOptions()
                .AddRedirect("(.*)/$", "$1")
                .AddRewrite(@"app/(\d+)", "app?id=$1", skipRemainingRules: false);
                //.AddIISUrlRewrite(env.ContentRootFileProvider, "UrlRewrite.xml")
                //.AddApacheModRewrite(env.ContentRootFileProvider, "Rewrite.txt");

            app.UseRewriter(rewriteOptions);
            app.Run(context => context.Response.WriteAsync($"Rewritten Url: {context.Request.Path + context.Request.QueryString}"));
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, 5000);
                    options.Listen(IPAddress.Loopback, 1234, listenOptions =>
                    {
                        // Configure HTTPS
                        listenOptions.UseHttps("testCert.pfx", "testPassword");
                    });
                })
                .ConfigureAppConfiguration((builderCtx, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Microsoft:AspNetCore:RedirectToHttps:Port"] = "1234"
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddTransient<IConfigureOptions<RedirectToHttpsOptions>, ConfigureDefaults<RedirectToHttpsOptions>>();
                    services.AddRedirectToHttps();
                })
                .UseStartup<Startup>()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .Build();

            host.Run();
        }
    }
}
