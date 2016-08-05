// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.ResponseCompression.Tests
{
    public class ResponseCompressionMiddlewareTest
    {
        private const string TextPlain = "text/plain";

        private const int MinimumSize = 10;

        [Fact]
        public void Options_IncorrectMinimumSize()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
           {
               new ResponseCompressionMiddleware(null, Options.Create(new ResponseCompressionOptions()
               {
                   MimeTypes = new string[] { TextPlain },
                   MinimumSize = 0
               }));
           });
        }

        [Fact]
        public void Options_NullMimesTypes()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new ResponseCompressionMiddleware(null, Options.Create(new ResponseCompressionOptions()
                {
                    MimeTypes = null,
                    MinimumSize = 10
                }));
            });
        }

        [Fact]
        public async Task Request_Uncompressed()
        {
            var response = await InvokeMiddleware(100, requestAcceptEncoding: null, responseType: TextPlain);

            CheckResponseCompressed(response, expectCompressed: false, expectedBodyLength: 100);
        }

        [Fact]
        public async Task Request_CompressGzip()
        {
            var response = await InvokeMiddleware(100, requestAcceptEncoding: "gzip, deflate", responseType: TextPlain);

            CheckResponseCompressed(response, expectCompressed: true, expectedBodyLength: 24);
        }

        [Theory]
        [InlineData("text/plain; charset=ISO-8859-4")]
        [InlineData("text/plain ; charset=ISO-8859-4")]
        public async Task Request_CompressGzipWithMimeTypeWithCharset(string responseContentType)
        {
            var options = new ResponseCompressionOptions()
            {
                MimeTypes = new string[] { "text/plain" },
                MinimumSize = MinimumSize,
                Providers = new IResponseCompressionProvider[]
                {
                    new GzipResponseCompressionProvider(CompressionLevel.Optimal)
                }
            };

            var middleware = new ResponseCompressionMiddleware(async context =>
            {
                context.Response.ContentType = responseContentType;
                await context.Response.WriteAsync(new string('a', 100));
            }, Options.Create(options));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderNames.AcceptEncoding] = "gzip";

            httpContext.Response.Body = new MemoryStream();

            await middleware.Invoke(httpContext);

            Assert.Equal(24, httpContext.Response.ContentLength);
            Assert.Equal(24, httpContext.Response.Body.Length);
        }

        [Fact]
        public async Task Request_CompressUnknown()
        {
            var response = await InvokeMiddleware(100, requestAcceptEncoding: "unknown", responseType: TextPlain);

            CheckResponseCompressed(response, expectCompressed: false, expectedBodyLength: 100);
        }

        [Fact]
        public async Task Request_UnauthorizedMimeType()
        {
            var response = await InvokeMiddleware(100, requestAcceptEncoding: "gzip", responseType: "text/html");

            CheckResponseCompressed(response, expectCompressed: false, expectedBodyLength: 100);
        }

        [Fact]
        public async Task Request_UnderMinimumSize()
        {
            var response = await InvokeMiddleware(MinimumSize - 1, requestAcceptEncoding: "gzip", responseType: TextPlain);

            CheckResponseCompressed(response, expectCompressed: false, expectedBodyLength: MinimumSize - 1);
        }

        [Fact]
        public async Task Request_ResponseWithContentRange()
        {
            var response = await InvokeMiddleware(50, requestAcceptEncoding: "gzip", responseType: TextPlain, addResponseAction: (r) =>
            {
                r.Headers[HeaderNames.ContentRange] = "1-2/*";
            });

            CheckResponseCompressed(response, expectCompressed: false, expectedBodyLength: 50);
        }

        [Fact]
        public async Task Request_ResponseWithContentEncodingAlreadySet()
        {
            var otherContentEncoding = "something";

            var response = await InvokeMiddleware(50, requestAcceptEncoding: "gzip", responseType: TextPlain, addResponseAction: (r) =>
            {
                r.Headers[HeaderNames.ContentEncoding] = otherContentEncoding;
            });

            Assert.Equal("MD5", response.Headers[HeaderNames.ContentMD5]);
            Assert.Equal(otherContentEncoding, response.Headers[HeaderNames.ContentEncoding]);
            Assert.Null(response.ContentLength);
            Assert.Equal(50, response.Body.Length);
        }

        private async Task<HttpResponse> InvokeMiddleware(int uncompressedBodyLength, string requestAcceptEncoding, string responseType, Action<HttpResponse> addResponseAction = null)
        {
            var options = new ResponseCompressionOptions()
            {
                MimeTypes = new string[] { TextPlain },
                MinimumSize = MinimumSize,
                Providers = new IResponseCompressionProvider[]
                {
                    new GzipResponseCompressionProvider(CompressionLevel.Optimal)
                }
            };

            var middleware = new ResponseCompressionMiddleware(async context =>
            {
                context.Response.Headers[HeaderNames.ContentMD5] = "MD5";
                context.Response.ContentType = responseType;
                if (addResponseAction != null)
                {
                    addResponseAction(context.Response);
                }
                await context.Response.WriteAsync(new string('a', uncompressedBodyLength));
            }, Options.Create(options));

            var httpContext = new DefaultHttpContext();

            if (requestAcceptEncoding != null)
            {
                httpContext.Request.Headers[HeaderNames.AcceptEncoding] = requestAcceptEncoding;
            }

            httpContext.Response.Body = new MemoryStream();

            await middleware.Invoke(httpContext);

            return httpContext.Response;
        }

        private void CheckResponseCompressed(HttpResponse response, bool expectCompressed, int expectedBodyLength)
        {
            if (expectCompressed)
            {
                Assert.Equal(StringValues.Empty, response.Headers[HeaderNames.ContentMD5]);
                Assert.Equal("gzip", response.Headers[HeaderNames.ContentEncoding]);
                Assert.Equal(expectedBodyLength, response.ContentLength);
                Assert.Equal(expectedBodyLength, response.Body.Length);
            }
            else
            {
                Assert.Equal("MD5", response.Headers[HeaderNames.ContentMD5]);
                Assert.Equal(StringValues.Empty, response.Headers[HeaderNames.ContentEncoding]);
                Assert.Null(response.ContentLength);
                Assert.Equal(expectedBodyLength, response.Body.Length);
            }
        }
    }
}
