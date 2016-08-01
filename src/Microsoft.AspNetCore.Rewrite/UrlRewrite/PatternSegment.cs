// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class PatternSegment
    {
        // Given the httpcontext, the rule backreference, and the condition backreference
        // Create a new string. Based on the definer of the func.
        //                       Rule  Condition
        public Func<HttpContext, MatchResults, MatchResults, string> Evaluate { get; }

        public PatternSegment(Func<HttpContext, MatchResults, MatchResults, string> evaluate)
        {
            Evaluate = evaluate;
        }
    }
}
