using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Rewrite.Internal
{
    public class RegexTimeoutSwitchUtility
    {
        internal static readonly string UseLowerRegexTimeoutsSwitch = "Switch.Microsoft.AspNetCore.Rewrite.UseLowerRegexTimeouts";
        // For testing
        public static bool UseLowerRegexTimeouts;

        static RegexTimeoutSwitchUtility()
        {
            AppContext.TryGetSwitch(UseLowerRegexTimeoutsSwitch, out UseLowerRegexTimeouts);
        }
    }
}
