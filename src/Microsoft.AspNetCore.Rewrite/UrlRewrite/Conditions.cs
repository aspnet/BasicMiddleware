using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class Conditions
    {
        public List<Condition> ConditionList { get; set; } = new List<Condition>();
        public LogicalGrouping MatchType { get; set; } // default is 0
        public bool TrackingAllCaptures { get; set; } // default is false
    }

    public enum LogicalGrouping
    {
        MatchAll,
        MatchAny
    }
}
