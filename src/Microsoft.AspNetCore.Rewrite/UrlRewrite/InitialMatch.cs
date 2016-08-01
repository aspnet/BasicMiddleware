// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class InitialMatch
    {
        public Func<string, MatchResults> Match { get; set; }
        public bool IgnoreCase { get; set; } = true;
        public bool Negate { get; set; }
    }
}
