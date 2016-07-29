// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class Conditions
    {
        public List<Condition> ConditionList { get; set; } = new List<Condition>();
        public LogicalGrouping MatchType { get; set; } // default is 0
        public bool TrackingAllCaptures { get; set; } // default is false
    }
}
