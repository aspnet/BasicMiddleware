using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class ServerVariables
    {
        public string Name { get; set; } // Required, unique key, trimmed
        public string Value { get; set; }
        public bool Replace { get; set; }

    }
}
