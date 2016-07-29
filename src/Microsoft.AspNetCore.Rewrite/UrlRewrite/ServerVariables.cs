// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public static class ServerVariables
    { 
        public static Func<HttpContext, Match, Match, string> FindServerVariable(string serverVariable)
        {
            switch(serverVariable)
            {
                // TODO Add all server variables here.
                case "ALL_RAW":
                    throw new NotImplementedException();
                case "APP_POOL_ID":
                    throw new NotImplementedException();
                case "CONTENT_LENGTH":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.ContentLength.ToString();
                    };
                case "CONTENT_TYPE":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.ContentType;
                    };
                case "HTTP_ACCEPT":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.Headers[HeaderNames.Accept];
                    };
                case "HTTP_COOKIE":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.Headers[HeaderNames.Cookie];
                    };
                case "HTTP_HOST":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.Headers[HeaderNames.Host];
                    };
                case "HTTP_PROXY_CONNECTION":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.Headers[HeaderNames.ProxyAuthenticate];
                    };
                case "HTTP_REFERER":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.Headers[HeaderNames.Referer];
                    };
                case "HTTP_USER_AGENT":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.Headers[HeaderNames.UserAgent];
                    };
                case "HTTP_CONNECTION":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.Headers[HeaderNames.Connection];
                    };
                case "HTTP_URL":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.Path.ToString();
                    };
                case "HTTPS":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.IsHttps ? "ON" : "OFF";
                    };
                case "LOCAL_ADDR":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Connection.LocalIpAddress.ToString();
                    };
                case "QUERY_STRING":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.QueryString.ToString();
                    };
                case "REMOTE_ADDR":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Connection.RemoteIpAddress?.ToString();
                    };
                case "REMOTE_HOST":
                    throw new NotImplementedException();
                case "REMOTE_PORT":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Connection.RemotePort.ToString(CultureInfo.InvariantCulture);
                    };
                default:
                    throw new FormatException("Unrecognized server variable.");
            }
        }
    }
}
