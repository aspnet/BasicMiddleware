// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite;

namespace Microsoft.AspNetCore.Rewrite.Internal.PatternSegments
{
    public class UrlSegment : PatternSegment
    {
        private readonly UriMatchCondition.UriMatchPart _uriMatchPart;

        public UrlSegment(UriMatchCondition.UriMatchPart uriMatchPart = UriMatchCondition.UriMatchPart.Path)
        {
            _uriMatchPart = uriMatchPart;
        }

        public override string Evaluate(RewriteContext context, BackReferenceCollection ruleBackReferences, BackReferenceCollection conditionBackReferences)
        {
            return _uriMatchPart == UriMatchCondition.UriMatchPart.Full ? context.HttpContext.Request.GetEncodedUrl() : (string)context.HttpContext.Request.Path;
        }
    }
}