﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Sockets;

namespace Microsoft.AspNetCore.Rewrite.Internal.PatternSegments
{

    public class IsIPV6Segment : PatternSegment
    {
        public override string Evaluate(RewriteContext context, MatchResults ruleMatch, BackReferenceCollection backReferences)
        {
            if (context.HttpContext.Connection.RemoteIpAddress == null)
            {
                return "off";
            }
            return context.HttpContext.Connection.RemoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6 ? "on" : "off";
        }
    }
}
