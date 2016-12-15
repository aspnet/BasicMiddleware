﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Rewrite.Internal.PatternSegments
{
    public class LocalAddressSegment : PatternSegment
    {
        public override string Evaluate(RewriteContext context, MatchResults ruleMatch, BackReferenceCollection backReferences)
        {
            return context.HttpContext.Connection.LocalIpAddress?.ToString();
        }
    }
}
