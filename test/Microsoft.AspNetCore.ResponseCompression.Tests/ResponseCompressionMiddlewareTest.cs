// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNetCore.ResponseCompression.Tests
{
    public class ResponseCompressionMiddlewareTest
    {
        private const string TextPlain = "text/plain";

        [Fact]
        public void Options_NullMimesTypes()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new ResponseCompressionMiddleware(null, Options.Create(new ResponseCompressionOptions()
                {
                    MimeTypes = null
                }));
            });
        }

        [Fact]
        public void Options_HttpsDisabledByDefault()
        {
            var options = new ResponseCompressionOptions();

            Assert.False(options.EnableHttps);
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
                MimeTypes = new string[] { TextPlain },
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

            Assert.Null(httpContext.Response.ContentLength);
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

        [Theory]
        [InlineData(false, 100)]
        [InlineData(true, 24)]
        public async Task Request_Https(bool enableHttps, int expectedLength)
        {
            var options = new ResponseCompressionOptions()
            {
                MimeTypes = new string[] { TextPlain },
                Providers = new IResponseCompressionProvider[]
                {
                    new GzipResponseCompressionProvider(CompressionLevel.Optimal)
                },
                EnableHttps = enableHttps
            };

            var middleware = new ResponseCompressionMiddleware(async context =>
            {
                context.Response.ContentType = TextPlain;
                await context.Response.WriteAsync(new string('a', 100));
            }, Options.Create(options));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderNames.AcceptEncoding] = "gzip";
            httpContext.Request.IsHttps = true;

            httpContext.Response.Body = new MemoryStream();

            await middleware.Invoke(httpContext);

            Assert.Equal(expectedLength, httpContext.Response.Body.Length);
        }

        private async Task<HttpResponse> InvokeMiddleware(int uncompressedBodyLength, string requestAcceptEncoding, string responseType, Action<HttpResponse> addResponseAction = null)
        {
            var options = new ResponseCompressionOptions()
            {
                MimeTypes = new string[] { TextPlain },
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
                Assert.Null(response.ContentLength);
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

        [Fact]
        public async Task Request_FullStack()
        {
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseResponseCompression(new ResponseCompressionOptions()
                    {
                        MimeTypes = new string[] { TextPlain }
                    });
                    app.Run(context =>
                    {
                        context.Response.Headers[HeaderNames.ContentType] = TextPlain;
                        return context.Response.WriteAsync(new string('a', 100));
                    });
                });

            var server = new TestServer(builder);
            var client = server.CreateClient();

            // Temp: very basic performance test code
            //var chrono = new System.Diagnostics.Stopwatch();
            //chrono.Start();
            //for (int i = 0; i < 10000; i++)
            //{
                var request = new HttpRequestMessage(HttpMethod.Get, "");
                request.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));

                var response = await client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                Assert.Equal(24, responseContent.Length);
            //}
            //chrono.Stop();
            //Assert.Equal(5, chrono.ElapsedMilliseconds);
        }
    }
}
