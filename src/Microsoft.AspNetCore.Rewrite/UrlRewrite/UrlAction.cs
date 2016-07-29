using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class UrlAction
    {
        public ActionType Type { get; set; }
        public Pattern Url { get; set; }
        public bool AppendQueryString { get; set; }
        public bool LogRewrittenUrl { get; set; }
        public RedirectType RedirectType { get; set; } = RedirectType.Permanent;
    }

    public enum RedirectType
    {
        Permanent = 301,
        Found = 302,
        SeeOther = 303,
        Temporary = 307
    }

    public enum ActionType
    {
        None,
        Rewrite,
        Redirect,
        CustomResponse,
        AbortRequest
    }
}
