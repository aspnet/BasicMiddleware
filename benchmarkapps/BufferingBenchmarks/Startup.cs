// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;

namespace BufferingBenchmarks
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.MapWhen(context => context.Request.Path.StartsWithSegments("buffered", StringComparison.Ordinal), builder =>
            {
                builder.UseResponseBuffering();
                builder.UseJson();
            });

            app.MapWhen(context => context.Request.Path.StartsWithSegments("nonbuffered", StringComparison.Ordinal), builder =>
            {
                builder.UseJson();
            });
        }
    }
}
