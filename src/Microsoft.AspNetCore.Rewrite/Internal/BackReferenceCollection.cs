// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Rewrite.Internal
{
    public class BackReferenceCollection
    {
        private List<string> backReferences = new List<string>();

        public string GetBackReferenceAtIndex(int index)
        {
            return backReferences[index];
        }

        public void AddBackReferences(MatchResults matchResults, bool trackAllCaptures = false)
        {
            if (!trackAllCaptures)
            {
                backReferences.Clear();
            }

            var currentBackReference = matchResults.BackReference;
            if (currentBackReference != null)
            {
                for (var i = 0; i < currentBackReference.Count; i++)
                {
                    backReferences.Add(currentBackReference[i].Value);
                }
            }
            else if (!string.IsNullOrEmpty(matchResults.ExactBackReference))
            {
                backReferences.Add(matchResults.ExactBackReference);
            }
        }
    }
}
