// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Rewrite.RuleAbstraction;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    public class UrlRewriteRule : Rule
    {
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public PatternSyntax PatternSyntax { get; set; }
        public bool StopProcessing { get; set; }
        public UrlMatch InitialMatch { get; set; }
        public Conditions Conditions { get; set; }
        public UrlAction Action { get; set; }

        public override RuleResult ApplyRule(UrlRewriteContext context)
        {
            if (!Enabled)
            {
                return new RuleResult { Result = RuleTerminiation.Continue };
            }
            // Due to the path string always having a leading slash,
            // remove it from the path before regex comparison
            var initMatchRes = InitialMatch.Evaluate(context.HttpContext.Request.Path.ToString().Substring(1), context);

            if (!initMatchRes.Success)
            {
                return new RuleResult { Result = RuleTerminiation.Continue };
            }

            var condMatchRes = Conditions.Evaluate(context, initMatchRes);
            if (!condMatchRes.Success)
            {
                return new RuleResult { Result = RuleTerminiation.Continue };
            }

            // at this point we know the rule passed, evaluate the replacement.
            return Action.ApplyAction(context.HttpContext, initMatchRes, condMatchRes);
        }
    }
}
