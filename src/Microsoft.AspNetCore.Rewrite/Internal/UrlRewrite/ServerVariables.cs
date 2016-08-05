﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Rewrite.Internal.UrlRewrite.PatternSegments;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Rewrite.Internal.UrlRewrite
{
    public static class ServerVariables
    { 
        public static PatternSegment FindServerVariable(string serverVariable)
        {
            switch(serverVariable)
            {
                // TODO Add all server variables here.
                case "ALL_RAW":
                    throw new NotImplementedException();
                case "APP_POOL_ID":
                    throw new NotImplementedException();
                case "CONTENT_LENGTH":
                    return new HeaderSegment(HeaderNames.ContentLength);
                case "CONTENT_TYPE":
                    return new HeaderSegment(HeaderNames.ContentType);
                case "HTTP_ACCEPT":
                    return new HeaderSegment(HeaderNames.Accept);
                case "HTTP_COOKIE":
                    return new HeaderSegment(HeaderNames.Cookie);
                case "HTTP_HOST":
                    return new HeaderSegment(HeaderNames.Host);
                case "HTTP_PROXY_CONNECTION":
                    return new HeaderSegment(HeaderNames.ProxyAuthenticate);
                case "HTTP_REFERER":
                    return new HeaderSegment(HeaderNames.Referer);
                case "HTTP_USER_AGENT":
                    return new HeaderSegment(HeaderNames.UserAgent);
                case "HTTP_CONNECTION":
                    return new HeaderSegment(HeaderNames.Connection);
                case "HTTP_URL":
                    return new UrlSegment();
                case "HTTPS":
                    return new IsHttpsSegment();
                case "LOCAL_ADDR":
                    return new LocalAddressSegment();
                case "QUERY_STRING":
                    return new QueryStringSegment();
                case "REMOTE_ADDR":
                    return new RemoteAddressSegment();
                case "REMOTE_HOST":
                    throw new NotImplementedException();
                case "REMOTE_PORT":
                    return new RemotePortSegment();
                case "REQUEST_FILENAME":
                    return new RequestFileNameSegment();
                default:
                    throw new FormatException("Unrecognized server variable.");
            }
        }
    }
}
