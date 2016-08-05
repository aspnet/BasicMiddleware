// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.ResponseCompression
{
    /// <summary>
    /// Stream wrapper that create specific compression stream only if necessary.
    /// </summary>
    internal class BodyWrapperStream : Stream
    {
        private readonly HttpResponse _response;

        private readonly Stream _bodyOriginalStream;

        private readonly HashSet<string> _mimeTypes;

        private bool _compressionChecked = false;

        internal Stream UncompressedStream { get; private set; } = null;

        internal BodyWrapperStream(HttpResponse response, Stream bodyOriginalStream, HashSet<string> mimeTypes)
        {
            _response = response;
            _bodyOriginalStream = bodyOriginalStream;
            _mimeTypes = mimeTypes;
        }

        public override bool CanRead => _bodyOriginalStream.CanRead;

        public override bool CanSeek => _bodyOriginalStream.CanSeek;

        public override bool CanWrite => _bodyOriginalStream.CanWrite;

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public override void Flush()
        {
            if (UncompressedStream != null)
            {
                UncompressedStream.Flush();
            }
            else
            {
                _bodyOriginalStream.Flush();
            }
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            if (UncompressedStream != null)
            {
                return UncompressedStream.FlushAsync(cancellationToken);
            }
            return _bodyOriginalStream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            OnWrite();

            if (UncompressedStream != null)
            {
                UncompressedStream.Write(buffer, offset, count);
            }
            else
            {
                _bodyOriginalStream.Write(buffer, offset, count);
            }
        }

#if NET451
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            OnWrite();

            if (UncompressedStream != null)
            {
                return UncompressedStream.BeginWrite(buffer, offset, count, callback, state);
            }
            return _bodyOriginalStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            OnWrite();

            if (UncompressedStream != null)
            {
                UncompressedStream.EndWrite(asyncResult);
            }
            else
            {
                _bodyOriginalStream.EndWrite(asyncResult);
            }
        }
#endif

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            OnWrite();

            if (UncompressedStream != null)
            {
                return UncompressedStream.WriteAsync(buffer, offset, count, cancellationToken);
            }
            return _bodyOriginalStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        private void OnWrite()
        {
            if (!_compressionChecked)
            {
                if (IsCompressable())
                {
                    UncompressedStream = new MemoryStream();
                }

                _compressionChecked = true;
            }
        }

        private bool IsCompressable()
        {
            return _response.Headers[HeaderNames.ContentRange] == StringValues.Empty &&     // The response is not partial
                _response.Headers[HeaderNames.ContentEncoding] == StringValues.Empty &&    // Not specific encoding already set
                IsMimeTypeCompressable(_response.ContentType);      // MIME type in the authorized list
        }

        private bool IsMimeTypeCompressable(string mimeType)
        {
            var separator = mimeType.IndexOf(';');
            if (separator >= 0)
            {
                // Remove the content-type optional parameters
                mimeType = mimeType.Substring(0, separator);
                mimeType = mimeType.Trim();
            }

            return !string.IsNullOrEmpty(mimeType) && _mimeTypes.Contains(mimeType);
        }
    }
}
