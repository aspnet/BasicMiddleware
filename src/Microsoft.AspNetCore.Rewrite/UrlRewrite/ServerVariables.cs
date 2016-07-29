using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public static class ServerVariables
    { 
        public static Func<HttpContext, Match, Match, string> FindServerVariable(string serverVariable)
        {
            switch(serverVariable)
            {
                // TODO
                case "CONTENT_LENGTH":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.ContentLength.ToString();
                    };
                case "CONTENT_TYPE":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.ContentLength.ToString();
                    };
                case "QUERY_STRING":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.ContentLength.ToString();
                    };
                case "REMOTE_ADDR":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.ContentLength.ToString();
                    };
                case "REMOTE_HOST":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.ContentLength.ToString();
                    };
                case "REMOTE_PORT":
                    return (ctx, ruleMatch, condMatch) =>
                    {
                        return ctx.Request.ContentLength.ToString();
                    };
                default:
                    throw new FormatException();
            }
        }
    }
}
