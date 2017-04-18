// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;

namespace Microsoft.AspNetCore.Rewrite
{
    public class PooledStringBuilder
    {
        private static readonly ObjectPool<PooledStringBuilder> _sGlobalPool = CreatePool(Environment.ProcessorCount * 2);

        public readonly StringBuilder Builder = new StringBuilder();
        private readonly ObjectPool<PooledStringBuilder> _pool;

        private PooledStringBuilder(ObjectPool<PooledStringBuilder> pool)
        {
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        }

        public int Length => Builder.Length;

        public void Free()
        {
            StringBuilder builder = Builder;

            // do not store builders that are too large.
            if (builder.Capacity > 1024)
            {
                return;
            }

            builder.Clear();
            _pool.Free(this);
        }

        [Obsolete("Consider calling " + nameof(ToStringAndFree) + " instead.")]
        public new string ToString()
        {
            return Builder.ToString();
        }

        public string ToStringAndFree()
        {
            string result = Builder.ToString();
            Free();

            return result;
        }

        public string ToStringAndFree(int startIndex, int length)
        {
            string result = Builder.ToString(startIndex, length);
            Free();

            return result;
        }

        public static ObjectPool<PooledStringBuilder> CreatePool(int size = 32)
        {
            ObjectPool<PooledStringBuilder> pool = null;
            pool = new ObjectPool<PooledStringBuilder>(() => new PooledStringBuilder(pool), size);
            return pool;
        }

        public static PooledStringBuilder Allocate()
        {
            PooledStringBuilder builder = _sGlobalPool.Allocate();
            if (builder.Builder.Length != 0)
            {
                throw new InvalidOperationException("Builder in use");
            }
            return builder;
        }

        public static implicit operator StringBuilder(PooledStringBuilder obj)
        {
            return obj.Builder;
        }
    }
}