﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;

namespace Microsoft.AspNetCore.Rewrite.Internal.PatternSegments
{
    public class LocalPortSegment : PatternSegment
    {
        public override string Evaluate(RewriteContext context, BackReferenceCollection ruleBackReferences, BackReferenceCollection conditionBackReferences)
        {
            return context.HttpContext.Connection.LocalPort.ToString(CultureInfo.InvariantCulture);
        }
    }
}
