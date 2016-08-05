﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Rewrite.Internal.UrlRewrite.PatternSegments
{
    public class HeaderSegment : PatternSegment
    {
        public string Header { get; set; }
        
        public HeaderSegment(string header)
        {
            Header = header;
        }

        public override string Evaluate(HttpContext context, MatchResults ruleMatch, MatchResults condMatch)
        {
            return context.Request.Headers[Header];
        }
    }
}
