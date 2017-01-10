// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite;

namespace RewriteSample
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var options = new RewriteOptions()
                .AddRedirect("(.*)/$", "$1")
                .AddRewrite(@"app/(\d+)", "app?id=$1", skipRemainingRules: false)
                .AddRedirectToHttps(302, 5001)
                .AddIISUrlRewrite(env.ContentRootFileProvider, "UrlRewrite.xml")
                .AddApacheModRewrite(env.ContentRootFileProvider, "Rewrite.txt")
                .AddIISUrlRewrite(CreateCustomIISRule());

            app.UseRewriter(options);
            app.Run(context => context.Response.WriteAsync($"Rewritten Url: {context.Request.Path + context.Request.QueryString}"));
        }

        /// <summary>
        /// Creates an IIS rewrite rule programmatically that is equivalent to the following xml configuration:
        /// 
        /// <rule name="Test">
        ///     <match url="(.*)" ignoreCase="false" />
        ///     <conditions trackAllCaptures = "true" >
        ///         <add input="{REQUEST_URI}" pattern="^/([a-zA-Z]+)/([0-9]+)$" />
        ///         <add input="{QUERY_STRING}" pattern="p2=([a-z]+)" />
        ///     </conditions>
        ///     <action type="Redirect" url ="blogposts/{C:1}/{C:4}" />
        /// </rule>
        /// </summary>
        /// <returns>An <see cref="IISUrlRewriteRule"/></returns>
        private static IISUrlRewriteRule CreateCustomIISRule()
        {
            // create rule builder
            var ruleBuilder = new UrlRewriteRuleBuilder
            {
                Name = "Test",
                Enabled = true
            };

            // add url match
            ruleBuilder.AddUrlMatch("(.*)", ignoreCase: false);

            // add conditions
            ruleBuilder.ConfigureConditionBehavior(LogicalGrouping.MatchAll, trackAllCaptures: true);
            var requestUriCondition = new UriMatchCondition(
                "{REQUEST_URI}",
                "^/([a-zA-Z]+)/([0-9]+)$",
                UriMatchPart.Path,
                ignoreCase: true,
                negate: false);
            var queryStringCondition = new UriMatchCondition(
                "{QUERY_STRING}",
                "p2=([a-z]+)",
                UriMatchPart.Path,
                ignoreCase: true,
                negate: false);
            ruleBuilder.AddUrlConditions(new[] { requestUriCondition, queryStringCondition });

            // add action
            ruleBuilder.AddUrlAction(new InputParser().ParseInputString("blogposts/{C:1}/{C:4}", UriMatchPart.Path), ActionType.Redirect);

            // create rule
            return ruleBuilder.Build(false);
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, 5000);
                    options.Listen(IPAddress.Loopback, 5001, listenOptions =>
                    {
                        // Configure SSL
                        listenOptions.UseHttps("testCert.pfx", "testPassword");
                    });
                })
                .UseStartup<Startup>()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .Build();

            host.Run();
        }
    }
}
