// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Rewrite.RuleAbstraction;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class UrlRewriteRule : Rule
    {
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public PatternSyntax PatternSyntax { get; set; }
        public bool StopProcessing { get; set; }
        public InitialMatch InitialMatch { get; set; }
        public Conditions Conditions { get; set; }
        public UrlAction Action { get; set; }

        public override RuleResult ApplyRule(UrlRewriteContext context)
        {
            // TODO
            if (!Enabled)
            {
                return new RuleResult { Result = RuleTerminiation.Continue };
            }
            // 1. Figure out which section of the string to match for the initial rule.
            var initMatchRes = InitialMatch.Match(context.HttpContext.Request.Path.ToString());

            if (initMatchRes.Success == InitialMatch.Negate)
            {
                return new RuleResult { Result = RuleTerminiation.Continue };
            }
            // done with initial match
            
            // go through conditions now
        }
    }
}
