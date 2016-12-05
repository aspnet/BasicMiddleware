// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Rewrite.Extensions;

namespace Microsoft.AspNetCore.Rewrite.Internal.PatternSegments
{
    public class UrlSegment : PatternSegment
    {
        public override string Evaluate(RewriteContext context, MatchResults ruleMatch, MatchResults condMatch)
        {
            return context.GlobalRule ? context.HttpContext.Request.ToUri().AbsoluteUri : (string)context.HttpContext.Request.Path;
        }
    }
}
