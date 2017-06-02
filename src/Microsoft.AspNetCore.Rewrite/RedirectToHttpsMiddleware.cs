// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Rewrite
{
    public class RedirectToHttpsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IFileProvider _fileProvider;
        private RewriteMiddleware _rewriteMiddleware;


        public RedirectToHttpsMiddleware(
            RequestDelegate next,
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory,
            IOptions<RedirectToHttpsOptions> options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _next = next;
            _fileProvider = hostingEnvironment.WebRootFileProvider;

            var rewriteOptions = new RewriteOptions().AddRedirectToHttps(options.Value.StatusCode, options.Value.Port);
            _rewriteMiddleware = new RewriteMiddleware(_next, hostingEnvironment, loggerFactory, rewriteOptions);
        }

        public Task Invoke(HttpContext context)
        {
           if (context == null)
           {
                throw new ArgumentNullException(nameof(context));
           }

           return _rewriteMiddleware.Invoke(context);
        }
    }
}
