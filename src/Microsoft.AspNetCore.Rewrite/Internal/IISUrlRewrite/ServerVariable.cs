// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite
{
    public class ServerVariable
    {
        public string Name { get; }
        public Pattern Pattern { get; }
        public ServerVariableType Type { get; }

        public ServerVariable(string name, Pattern pattern, ServerVariableType type)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }
            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            Name = name;
            Pattern = pattern;
            Type = type;
        }

        public string Evaluate(RewriteContext context, MatchResults ruleMatch, MatchResults condMatch)
        {
            return Pattern.Evaluate(context, ruleMatch, condMatch);
        }
    }
}