// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Collections.Concurrent
{
    public sealed class AsyncDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDisposable
    {
        private readonly IDictionary<TKey, TValue> _dictionary;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private bool _isDisposed;

        public AsyncDictionary() : this(new Dictionary<TKey, TValue>())
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }
        public AsyncDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>> ContainsKeyFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>>((dictionary, keyValuePair) =>
        {
            return Task.FromResult(dictionary.ContainsKey(keyValuePair.Key));
        });
        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>> ClearFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>>((dictionary, keyValuePair) =>
        {
            dictionary.Clear();

            return Task.FromResult(true);
        });
        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<int>> GetCountFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<int>>((dictionary, keyValuePair) =>
        {
            return Task.FromResult(dictionary.Count);
        });
        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<ICollection<TValue>>> GetValuesFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<ICollection<TValue>>>((dictionary, keyValuePair) =>
        {
            return Task.FromResult<ICollection<TValue>>(dictionary.Values.ToList());
        });
        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<ICollection<TKey>>> GetKeysFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<ICollection<TKey>>>((dictionary, keyValuePair) =>
        {
            return Task.FromResult<ICollection<TKey>>(dictionary.Keys.ToList());
        });
        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>> AddFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>>((dictionary, keyValuePair) =>
        {
            dictionary.Add(keyValuePair);

            return Task.FromResult(true);
        });
        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>> AddOrReplaceFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>>((dictionary, keyValuePair) =>
        {
            if (dictionary.ContainsKey(keyValuePair.Key))
            {
                dictionary[keyValuePair.Key] = keyValuePair.Value;
            }
            else
            {
                dictionary.Add(keyValuePair.Key, keyValuePair.Value);
            }

            return Task.FromResult(true);
        });
        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>> ContainsItemFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>>((dictionary, keyValuePair) =>
        {
            return Task.FromResult(dictionary.Contains(keyValuePair));
        });
        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>> RemoveFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>>((dictionary, keyValuePair) =>
        {
            return Task.FromResult(dictionary.Remove(keyValuePair));
        });
        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>> RemoveByKeyFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>>((dictionary, keyValuePair) =>
        {
            return Task.FromResult(dictionary.Remove(keyValuePair.Key));
        });
        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<TValue>> GetValueFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<TValue>>((dictionary, keyValuePair) =>
        {
            return Task.FromResult(dictionary[keyValuePair.Key]);
        });
        public Task<ICollection<TKey>> GetKeysAsync()
        {
            return CallSynchronizedAsync(GetKeysFunc, default);
        }

        public Task<ICollection<TValue>> GetValuesAsync()
        {
            return CallSynchronizedAsync(GetValuesFunc, default);
        }

        public Task<int> GetCountAsync()
        {
            return CallSynchronizedAsync(GetCountFunc, default);
        }

        public Task AddAsync(TKey key, TValue value)
        {
            return CallSynchronizedAsync(AddFunc, new KeyValuePair<TKey, TValue>(key, value));
        }
        public Task AddAsync(KeyValuePair<TKey, TValue> item)
        {
            return CallSynchronizedAsync(AddFunc, item);
        }

        public Task AddOrReplaceAsync(TKey key, TValue value)
        {
            return CallSynchronizedAsync(AddOrReplaceFunc, new KeyValuePair<TKey, TValue>(key, value));
        }

        public Task ClearAsync()
        {
            return CallSynchronizedAsync(ClearFunc, default);
        }

        public Task<bool> GetContainsAsync(KeyValuePair<TKey, TValue> item)
        {
            return CallSynchronizedAsync(ContainsItemFunc, item);
        }

        public Task<bool> GetContainsKeyAsync(TKey key)
        {
            return CallSynchronizedAsync(ContainsKeyFunc, new KeyValuePair<TKey, TValue>(key, default));
        }

        public Task<bool> RemoveAsync(TKey key)
        {
            return CallSynchronizedAsync(RemoveByKeyFunc, new KeyValuePair<TKey, TValue>(key, default));
        }
        public Task<bool> RemoveAsync(KeyValuePair<TKey, TValue> item)
        {
            return CallSynchronizedAsync(RemoveFunc, item);
        }

        public Task<TValue> GetValueAsync(TKey key)
        {
            return CallSynchronizedAsync(GetValueFunc, new KeyValuePair<TKey, TValue>(key, default));
        }

        private async Task<TReturn> CallSynchronizedAsync<TReturn>(Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<TReturn>> func, KeyValuePair<TKey, TValue> keyValuePair)
        {
            try
            {
                await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

                return await Task.Run(async () =>
                {
                    return await func(_dictionary, keyValuePair).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _dictionary).GetEnumerator();
        }

        public void Dispose()
        {
            Dispose(true);
        }
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _semaphoreSlim.Dispose();
                }

                _isDisposed = true;
            }
        }
    }
}
