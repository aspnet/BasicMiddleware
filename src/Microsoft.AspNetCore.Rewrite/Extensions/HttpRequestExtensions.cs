using System;

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Rewrite.Extensions
{
    public static class HttpRequestExtensions
    {
        public static Uri ToUri(this HttpRequest request)
        {
            var uriBuilder = new UriBuilder(request.Scheme, request.Host.Host);
            if (request.Host.Port.HasValue)
            {
                uriBuilder.Port = request.Host.Port.Value;
            }
            uriBuilder.Path = request.PathBase + request.Path;
            if (request.QueryString.HasValue)
            {
                uriBuilder.Query = request.QueryString.Value.Substring(1);
            }
            return uriBuilder.Uri;
        }
    }
}