
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Rewrite.Internal.UrlMatches;

namespace Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite
{
    public class UriMatchCondition : Condition
    {
        private const string UseLowerRegexTimeoutsSwitch = "Switch.Microsoft.AspNetCore.Rewrite.UseLowerRegexTimeouts";
        private static bool UseLowerRegexTimeouts;
        // For testing
        internal bool _uselowerRegexTimeouts = UseLowerRegexTimeouts;
        private TimeSpan _regexTimeout;

        static UriMatchCondition()
        {
            AppContext.TryGetSwitch(UseLowerRegexTimeoutsSwitch, out UseLowerRegexTimeouts);
        }

        public UriMatchCondition(InputParser inputParser, string input, string pattern, UriMatchPart uriMatchPart, bool ignoreCase, bool negate)
        {
            if (_regexTimeout == TimeSpan.Zero)
            {
                _regexTimeout = UseLowerRegexTimeouts ? TimeSpan.FromMilliseconds(1) : TimeSpan.FromSeconds(1);
            }

            var regexOptions = RegexOptions.CultureInvariant | RegexOptions.Compiled;
            regexOptions = ignoreCase ? regexOptions | RegexOptions.IgnoreCase : regexOptions;
            var regex = new Regex(
                pattern,
                regexOptions,
                _regexTimeout
            );
            Input = inputParser.ParseInputString(input, uriMatchPart);
            Match = new RegexMatch(regex, negate);
        }
    }
}