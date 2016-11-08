using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Rewrite.Internal.IISUrlRewrite
{
    public class IISRewriteMap
    {
        private readonly Dictionary<string, string> _map = new Dictionary<string, string>();
        public string Name { get; }

        public IISRewriteMap(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }
            Name = name;
        }

        public void AddOrUpdateEntry(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(nameof(key));
            }
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(nameof(value));
            }
            _map[key] = value;
        }

        public bool TryGetEntry(string key, out string value)
        {
            return _map.TryGetValue(key, out value);
        }
    }
}