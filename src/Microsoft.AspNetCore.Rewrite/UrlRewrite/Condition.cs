// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class Condition
    {
        public Pattern Input { get; set; } 
        public Func<string, MatchResults> Match { get; set; }
        public bool Negate { get; set; }
        public bool IgnoreCase { get; set; } = true;
        public MatchType MatchType { get; set; } = MatchType.Pattern;
    }
}
