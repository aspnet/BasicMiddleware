using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.DependencyInjection;

namespace HttpsSample
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpsPolicy(options => {
                options.SetHsts = true;
                options.StatusCode = 302;
                options.TlsPort = 5001;
                options.HstsOptions.MaxAge = 60000;
            });
        }


        public void Configure(IApplicationBuilder app)
        {
            app.UseHttpsPolicy();

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Hello world!");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel(
                options =>
                {
                    options.Listen(new IPEndPoint(IPAddress.Loopback, 5001), listenOptions =>
                    {
                        listenOptions.UseHttps("testCert.pfx", "testPassword");
                    });
                    options.Listen(new IPEndPoint(IPAddress.Loopback, 5000), listenOptions =>
                    {
                    });
                })
                .UseContentRoot(Directory.GetCurrentDirectory()) // for the cert file
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
