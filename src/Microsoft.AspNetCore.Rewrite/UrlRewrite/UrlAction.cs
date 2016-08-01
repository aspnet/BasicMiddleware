// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite.RuleAbstraction;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class UrlAction
    {
        public ActionType Type { get; set; }
        public Pattern Url { get; set; }
        public bool AppendQueryString { get; set; } = true;
        public bool LogRewrittenUrl { get; set; } // Ignoring this flag.
        public RedirectType RedirectType { get; set; } = RedirectType.Permanent;


        public Func<string, HttpContext, RuleResult> Evaluate { get; set; }
        // TODO consider having this in creation, action<context, rulematch, condmatch
        public RuleResult ApplyAction(HttpContext context, MatchResults ruleMatch, MatchResults condMatch)
        {
            var pattern = Url.Evaluate(context, ruleMatch, condMatch);
            return Evaluate(pattern, context);
        }
    }
}
