// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Rewrite.Internal
{
    public class BackReferenceCollection
    {
        private List<string> backReferences = new List<string>();

        public BackReferenceCollection(GroupCollection references)
        {
            if (references != null)
            {
                for (var i = 0; i < references.Count; i++)
                {
                    backReferences.Add(references[i].Value);
                }
            }
        }

        public BackReferenceCollection(string reference)
        {
            backReferences.Add(reference);
        }

        public string this[int index]
        {
            // TODO: index out of range error handling?
            get { return backReferences[index]; }
        }

        public void Add(BackReferenceCollection references)
        {
            if (references != null)
            {
                backReferences.AddRange(references.backReferences);
            }
        }
    }
}
