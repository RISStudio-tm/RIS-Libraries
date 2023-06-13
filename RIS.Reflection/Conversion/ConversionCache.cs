// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace RIS.Reflection.Conversion
{
    internal class ConversionCache
    {
        private readonly Dictionary<KeyValuePair<Type, Type>, bool> _cache;

        public int CacheSize { get; }

        public ConversionCache(int cacheSize = 5000)
        {
            if (cacheSize <= 0)
                cacheSize = 5000;

            _cache = new Dictionary<KeyValuePair<Type, Type>, bool>(
                cacheSize);

            CacheSize = cacheSize;
        }

        public bool TryGetValue(
            KeyValuePair<Type, Type> key, out bool value)
        {
            lock (((ICollection)_cache).SyncRoot)
            {
                return _cache.TryGetValue(key, out value);
            }
        }

        public void SetValue(
            KeyValuePair<Type, Type> key, bool value)
        {
            lock (((ICollection)_cache).SyncRoot)
            {
                if (_cache.Count >= CacheSize)
                    _cache.Clear();

                _cache[key] = value;
            }
        }
    }
}
