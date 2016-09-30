// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.HttpOverrides
{
    /// <summary>
    /// Default values related to <see cref="ForwardedHeadersMiddleware"/> middleware
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Builder.ForwardedHeadersOptions"/>
    public static class ForwardedHeadersDefaults
    {
        /// <summary>
        /// X-Forwarded-For
        /// </summary>
        public const string XForwardedForHeaderName = "X-Forwarded-For";

        /// <summary>
        /// X-Forwarded-Host
        /// </summary>
        public const string XForwardedHostHeaderName = "X-Forwarded-Host";

        /// <summary>
        /// X-Forwarded-Proto
        /// </summary>
        public const string XForwardedProtoHeaderName = "X-Forwarded-Proto";

        /// <summary>
        /// X-Original-For
        /// </summary>
        public const string XOriginalForHeaderName = "X-Original-For";

        /// <summary>
        /// X-Original-Host
        /// </summary>
        public const string XOriginalHostHeaderName = "X-Original-Host";

        /// <summary>
        /// X-Original-Proto
        /// </summary>
        public const string XOriginalProtoHeaderName = "X-Original-Proto";
    }
}
