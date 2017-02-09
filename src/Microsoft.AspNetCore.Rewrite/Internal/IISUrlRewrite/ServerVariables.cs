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

        /// <summary>
        /// Returns a <see cref="ServerVariable"/>
        /// </summary>
        /// <param name="serverVariable">The server variable</param>
        /// <param name="context">The parser context which is utilized when an exception is thrown</param>
        /// <param name="global">Indicates if the rule being parsed is a global rule</param>
        /// <exception cref="FormatException">Thrown when the server variable is unknown</exception>
        /// <returns>A <see cref="ServerVariable"/></returns>
        public static ServerVariable FindServerVariable(string serverVariable, ParserContext context, bool global)
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
                    return new ServerVariable(serverVariable, new Pattern(new HeaderSegment(HeaderNames.ContentLength)), ServerVariableType.RequestHeader);
                case "CONTENT_TYPE":
                    return new ServerVariable(serverVariable, new Pattern(new HeaderSegment(HeaderNames.ContentType)), ServerVariableType.RequestHeader);
                case "HTTP_ACCEPT":
                    return new ServerVariable(serverVariable, new Pattern(new HeaderSegment(HeaderNames.Accept)), ServerVariableType.RequestHeader);
                case "HTTP_COOKIE":
                    return new ServerVariable(serverVariable, new Pattern(new HeaderSegment(HeaderNames.Cookie)), ServerVariableType.RequestHeader);
                case "HTTP_HOST":
                    return new ServerVariable(serverVariable, new Pattern(new HeaderSegment(HeaderNames.Host)), ServerVariableType.RequestHeader);
                case "HTTP_REFERER":
                    return new ServerVariable(serverVariable, new Pattern(new HeaderSegment(HeaderNames.Referer)), ServerVariableType.RequestHeader);
                case "HTTP_USER_AGENT":
                    return new ServerVariable(serverVariable, new Pattern(new HeaderSegment(HeaderNames.UserAgent)), ServerVariableType.RequestHeader);
                case "HTTP_CONNECTION":
                    return new ServerVariable(serverVariable, new Pattern(new HeaderSegment(HeaderNames.Connection)), ServerVariableType.RequestHeader);
                case "HTTP_URL":
                    patternSegment = global ? (PatternSegment)new GlobalRuleUrlSegment() : (PatternSegment)new UrlSegment();
                    return new ServerVariable(serverVariable, new Pattern(patternSegment), ServerVariableType.RequestHeader);
                case "HTTPS":
                    return new ServerVariable(serverVariable, new Pattern(new IsHttpsUrlSegment()), ServerVariableType.Request);
                case "LOCAL_ADDR":
                    return new ServerVariable(serverVariable, new Pattern(new LocalAddressSegment()), ServerVariableType.Request);
                case "HTTP_PROXY_CONNECTION":
                    throw new NotSupportedException(Resources.FormatError_UnsupportedServerVariable(serverVariable));
                case "QUERY_STRING":
                    return new ServerVariable(serverVariable, new Pattern(new QueryStringSegment()), ServerVariableType.Request);
                case "REMOTE_ADDR":
                    return new ServerVariable(serverVariable, new Pattern(new RemoteAddressSegment()), ServerVariableType.Request);
                case "REMOTE_HOST":
                    throw new NotSupportedException(Resources.FormatError_UnsupportedServerVariable(serverVariable));
                case "REMOTE_PORT":
                    return new ServerVariable(serverVariable, new Pattern(new RemotePortSegment()), ServerVariableType.Request);
                case "REQUEST_FILENAME":
                    return new ServerVariable(serverVariable, new Pattern(new RequestFileNameSegment()), ServerVariableType.Request);
                case "REQUEST_METHOD":
                    return new ServerVariable(serverVariable, new Pattern(new RequestMethodSegment()), ServerVariableType.Request);
                case "REQUEST_SCHEME":
                    return new ServerVariable(serverVariable, new Pattern(new SchemeSegment()), ServerVariableType.Request);
                case "REQUEST_URI":
                    patternSegment = global ? (PatternSegment)new GlobalRuleUrlSegment() : (PatternSegment)new UrlSegment();
                    return new ServerVariable(serverVariable, new Pattern(patternSegment), ServerVariableType.Request);
                default:
                    throw new FormatException(Resources.FormatError_InputParserUnrecognizedParameter(serverVariable, context.Index));
            }
        }

        public static ServerVariable ParseCustomServerVariable(InputParser inputParser, bool global, string name, string value)
        {
            if (name.StartsWith(RequestHeaderPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return new ServerVariable(name.Substring(RequestHeaderPrefix.Length).Replace('_', '-'), inputParser.ParseInputString(value, global), ServerVariableType.RequestHeader);
            }
            if (name.StartsWith(ResponseHeaderPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return new ServerVariable(name.Substring(ResponseHeaderPrefix.Length).Replace('_', '-'), inputParser.ParseInputString(value, global), ServerVariableType.ResponseHeader);
            }
            throw new NotSupportedException($"Custom server variables must start with '{RequestHeaderPrefix}' or '{ResponseHeaderPrefix}'");
        }
    }
}
