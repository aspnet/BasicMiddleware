// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite
{
    public class UrlRewriteParseException : FormatException
    {
        public UrlRewriteParseException(XElement element, string message)
            : base(FormatMessage(element, message))
        {
        }

        public UrlRewriteParseException(XElement element, string message, Exception innerException)
            : base(FormatMessage(element, message), innerException)
        {
        }

        private static string FormatMessage(XElement element, string message)
        {
            var lineInfo = (IXmlLineInfo)element;
            var line = lineInfo.LineNumber;
            var col = lineInfo.LinePosition;
            return Resources.FormatError_UrlRewriteParseError(message, line, col);
        }
    }
}