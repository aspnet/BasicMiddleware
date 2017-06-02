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

        /// <summary>
        /// Creates a new instance of <see cref="RedirectToHttpsMiddleware"/>
        /// </summary>
        /// <param name="next">The delegate representing the next middleware in the request pipeline.</param>
        /// <param name="hostingEnvironment">The Hosting Environment.</param>
        /// <param name="loggerFactory">The Logger Factory.</param>
        /// <param name="options">The middleware options for specifing the port and status code of the redirected request.</param>
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

        /// <summary>
        /// Creates a new instance of <see cref="RedirectToHttpsMiddleware"/>
        /// </summary>
        /// <param name="next">The delegate representing the next middleware in the request pipeline.</param>
        /// <param name="hostingEnvironment">The Hosting Environment.</param>
        /// <param name="loggerFactory">The Logger Factory.</param>
        /// <param name="options">The middleware options for specifing the port and status code of the redirected request.</param>
        public RedirectToHttpsMiddleware(
            RequestDelegate next,
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory,
            RedirectToHttpsOptions options)
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

            var rewriteOptions = new RewriteOptions().AddRedirectToHttps(options.StatusCode, options.Port);
            _rewriteMiddleware = new RewriteMiddleware(_next, hostingEnvironment, loggerFactory, rewriteOptions);
        }

        /// <summary>
        /// Executes the RedirectToHttpsMiddleware by invoking the RewriteMiddleware.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <returns>A task that represents the execution of this middleware.</returns>
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
