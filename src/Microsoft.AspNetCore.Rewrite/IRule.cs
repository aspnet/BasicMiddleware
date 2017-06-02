// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Rewrite
{
    /// <summary>
    /// Represents a rule.
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// Applies the rule.
        /// Implementations of ApplyRule should set the value for <see cref="RewriteContext.Result"/>
        /// (defaults to RuleResult.ContinueRules)
        /// </summary>
        /// <param name="context"></param>
        Task ApplyRuleAsync(RewriteContext context);
    }
}