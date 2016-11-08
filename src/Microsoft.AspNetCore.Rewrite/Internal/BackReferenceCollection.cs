// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Rewrite.Internal
{
    public class BackReferenceCollection
    {
        private List<string> backReferences;

        public BackReferenceCollection()
        {
            backReferences = new List<string>();
        }

        public string GetBackReferenceAtIndex(int index)
        {
            return backReferences[index];
        }

        public void AddStringBackReference(string reference)
        {
            backReferences.Add(reference);
        }

        public void AddEnumerableBackReferences(IEnumerator references)
        {
            do
            {
                backReferences.Add((string)references.Current);
            } while (references.MoveNext());
        }

        public void ReplaceAllBackreferences(IEnumerator references)
        {
            backReferences.Clear();
            AddEnumerableBackReferences(references);
        }

        public void ReplaceAllBackreferences(string references)
        {
            backReferences.Clear();
            AddStringBackReference(references);
        }
    }
}
