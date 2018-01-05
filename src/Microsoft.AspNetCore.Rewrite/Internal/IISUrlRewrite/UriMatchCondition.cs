﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Rewrite.Internal.UrlMatches;

namespace Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite
{
    public class UriMatchCondition : Condition
    {
        internal TimeSpan _regexTimeout = RegexTimeoutSwitchUtility.UseLowerRegexTimeouts 
            ? TimeSpan.FromMilliseconds(1) 
            : TimeSpan.FromSeconds(1);

        public UriMatchCondition(InputParser inputParser, string input, string pattern, UriMatchPart uriMatchPart, bool ignoreCase, bool negate)
        {
            var regexOptions = RegexOptions.CultureInvariant | RegexOptions.Compiled;
            regexOptions = ignoreCase ? regexOptions | RegexOptions.IgnoreCase : regexOptions;
            var regex = new Regex(
                pattern,
                regexOptions,
                _regexTimeout);
            Input = inputParser.ParseInputString(input, uriMatchPart);
            Match = new RegexMatch(regex, negate);
        }
    }
}