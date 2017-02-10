// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Rewrite.Internal
{
    public class DelegateRule : IRule
    {
        private readonly Func<RewriteContext, Task> _onApplyRuleAsync;

        public DelegateRule(Func<RewriteContext, Task> onApplyRuleAsync)
        {
            _onApplyRuleAsync = onApplyRuleAsync;
        }
        public Task ApplyRuleAsync(RewriteContext context) => _onApplyRuleAsync(context);
    }
}