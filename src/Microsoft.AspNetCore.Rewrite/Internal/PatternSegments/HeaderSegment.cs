﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Rewrite.Internal.PatternSegments
{
    public class HeaderSegment : PatternSegment
    {
        private readonly string _header;

        public HeaderSegment(string header)
        {
            _header = header;
        }

        public override string Evaluate(RewriteContext context, BackReferenceCollection ruleBackReferences, BackReferenceCollection conditionBackReferences)
        {
            return context.HttpContext.Request.Headers[_header];
        }
    }
}
