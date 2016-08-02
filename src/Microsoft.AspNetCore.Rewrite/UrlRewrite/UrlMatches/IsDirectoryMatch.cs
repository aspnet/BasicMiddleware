// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite.UrlMatches
{
    public class IsDirectoryMatch : UrlMatch
    {
        public IsDirectoryMatch( bool negate)
        { 
            Negate = negate;
        }

        public override MatchResults Evaluate(string pattern, UrlRewriteContext context)
        {
            var res = context.FileProvider.GetFileInfo(pattern).IsDirectory;
            return new MatchResults { Success = (res != Negate) };
        }
    }
}
