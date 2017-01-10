// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Rewrite.Internal.UrlMatches;

namespace Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite
{
    public class UriMatchCondition : Condition
    {
        private static readonly InputParser _inputParser = new InputParser();

        public UriMatchCondition(string pattern, string input, UriMatchPart uriMatchPart, bool ignoreCase, bool negate)
        {
            var regex = new Regex(
                pattern,
                ignoreCase ? RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase : RegexOptions.CultureInvariant | RegexOptions.Compiled,
                TimeSpan.FromMilliseconds(1));
            Input = _inputParser.ParseInputString(input, uriMatchPart == UriMatchPart.Full);
            Match = new RegexMatch(regex, negate);
        }

        public enum UriMatchPart
        {
            Full,
            Path
        }
    }
}