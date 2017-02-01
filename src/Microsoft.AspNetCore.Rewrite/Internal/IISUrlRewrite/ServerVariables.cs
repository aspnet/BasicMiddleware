// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Rewrite.Internal.PatternSegments;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite
{
    public static class ServerVariables
    {
        public const string RequestHeaderPrefix = "HTTP_";
        public const string ResponseHeaderPrefix = "RESPONSE_";

        public static ServerVariable FindServerVariable(string serverVariable, ParserContext context)
        {
            PatternSegment patternSegment;

            switch (serverVariable)
            {
                // TODO Add all server variables here.
                case "ALL_RAW":
                    throw new NotSupportedException(Resources.FormatError_UnsupportedServerVariable(serverVariable));
                case "APP_POOL_ID":
                    throw new NotSupportedException(Resources.FormatError_UnsupportedServerVariable(serverVariable));
                case "CONTENT_LENGTH":
                    patternSegment = new HeaderSegment(HeaderNames.ContentLength);
                    break;
                case "CONTENT_TYPE":
                    patternSegment = new HeaderSegment(HeaderNames.ContentType);
                    break;
                case "HTTP_ACCEPT":
                    patternSegment = new HeaderSegment(HeaderNames.Accept);
                    break;
                case "HTTP_COOKIE":
                    patternSegment = new HeaderSegment(HeaderNames.Cookie);
                    break;
                case "HTTP_HOST":
                    patternSegment = new HeaderSegment(HeaderNames.Host);
                    break;
                case "HTTP_REFERER":
                    patternSegment = new HeaderSegment(HeaderNames.Referer);
                    break;
                case "HTTP_USER_AGENT":
                    patternSegment = new HeaderSegment(HeaderNames.UserAgent);
                    break;
                case "HTTP_CONNECTION":
                    patternSegment = new HeaderSegment(HeaderNames.Connection);
                    break;
                case "HTTP_URL":
                    patternSegment = new UrlSegment();
                    break;
                case "HTTPS":
                    patternSegment = new IsHttpsUrlSegment();
                    break;
                case "LOCAL_ADDR":
                    patternSegment = new LocalAddressSegment();
                    break;
                case "HTTP_PROXY_CONNECTION":
                    throw new NotSupportedException(Resources.FormatError_UnsupportedServerVariable(serverVariable));
                case "QUERY_STRING":
                    patternSegment = new QueryStringSegment();
                    break;
                case "REMOTE_ADDR":
                    patternSegment = new RemoteAddressSegment();
                    break;
                case "REMOTE_HOST":
                    throw new NotSupportedException(Resources.FormatError_UnsupportedServerVariable(serverVariable));
                case "REMOTE_PORT":
                    patternSegment = new RemotePortSegment();
                    break;
                case "REQUEST_FILENAME":
                    patternSegment = new RequestFileNameSegment();
                    break;
                case "REQUEST_METHOD":
                    patternSegment = new RequestMethodSegment();
                    break;
                case "REQUEST_SCHEME":
                    patternSegment = new SchemeSegment();
                    break;
                case "REQUEST_URI":
                    patternSegment = new UrlSegment();
                    break;
                default:
                    throw new FormatException(Resources.FormatError_InputParserUnrecognizedParameter(serverVariable, context.Index));
            }

            return new ServerVariable(serverVariable, new Pattern(patternSegment), ServerVariableType.RequestHeader);
        }

        public static ServerVariable ParseCustomServerVariable(InputParser inputParser, string name, string value)
        {
            if (name.StartsWith(RequestHeaderPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return new ServerVariable(name.Substring(RequestHeaderPrefix.Length).Replace('_', '-'), inputParser.ParseInputString(value), ServerVariableType.RequestHeader);
            }
            if (name.StartsWith(ResponseHeaderPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return new ServerVariable(name.Substring(ResponseHeaderPrefix.Length).Replace('_', '-'), inputParser.ParseInputString(value), ServerVariableType.ResponseHeader);
            }
            throw new NotSupportedException($"Custom server variables must start with '{RequestHeaderPrefix}' or '{ResponseHeaderPrefix}'");
        }
    }
}