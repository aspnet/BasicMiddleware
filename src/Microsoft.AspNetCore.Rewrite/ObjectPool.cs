// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;

namespace Microsoft.AspNetCore.Rewrite
{
    public class ObjectPool<T>
    {
        private readonly Func<T> _objectFactory;
        private readonly BlockingCollection<T> _objects;

        public ObjectPool(Func<T> objectFactory, int poolSize)
        {
            _objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
            _objects = new BlockingCollection<T>(new ConcurrentBag<T>(), poolSize);
        }

        public T Allocate()
        {
            T item;
            return _objects.TryTake(out item) ? item : _objectFactory();
        }

        public void Free(T item)
        {
            _objects.TryAdd(item);
        }
    }
}