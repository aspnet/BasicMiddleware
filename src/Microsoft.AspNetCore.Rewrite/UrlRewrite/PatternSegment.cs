// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class PatternSegment
    {
        public Func<HttpContext, Match, Match, string> Evaluate { get; }

        public PatternSegment(Func<HttpContext, Match, Match, string> evaluate)
        {
            Evaluate = evaluate;
        }
    }
}
