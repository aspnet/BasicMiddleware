﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace Microsoft.AspNetCore.Builder
{
    public class ForwardedHeadersOptions
    {
        /// <summary>
        /// Identifies which forwarders should be processed.
        /// </summary>
        public ForwardedHeaders ForwardedHeaders { get; set; }

        /// <summary>
        /// Limits the number of entries in the headers that will be processed. The default value is 1.
        /// Set to null to disable the limit, but this should only be done if
        /// KnownProxies or KnownNetworks are configured.
        /// </summary>
        public int? ForwardLimit { get; set; } = 1;

        /// <summary>
        /// Addresses of known proxies to accept forwarded headers from.
        /// </summary>
        public IList<IPAddress> KnownProxies { get; } = new List<IPAddress>() { IPAddress.IPv6Loopback };

        /// <summary>
        /// Address ranges of known proxies to accept forwarded headers from.
        /// </summary>
        public IList<IPNetwork> KnownNetworks { get; } = new List<IPNetwork>() { new IPNetwork(IPAddress.Loopback, 8) };

        /// <summary>
        /// Require the number of header values to be in sync between the different headers being processed.
        /// The default is 'true'.
        /// </summary>
        public bool RequireHeaderSymmetry { get; set; } = true;
    }
}
