﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class MatchResults
    {
        public ICollection BackReference { get; set; }
        public bool Success { get; set; }
    }
}
