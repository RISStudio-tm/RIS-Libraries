using System;
using System.Collections;
using System.Collections.Generic;
using RIS.Collections.Trees;

namespace RIS.Collections.Caches
{
    public sealed class MicroCache<TKey, TValue>
    {
        private sealed class ValueHolder
        {
            private int _useCount = 1;

            public TValue Value { get; }
            public int UseCount
            {
                get
                {
                    return _useCount;
                }
            }

            public ValueHolder(TValue value)
            {
                Value = value;
            }

            public void Touch()
            {
                int currentCount = _useCount;

                if (currentCount < int.MaxValue)
                    _useCount = currentCount + 1;
            }

            public void Age()
            {
                _useCount /= 2;
            }
        }
        private sealed class UseCountComparer : IComparer<KeyValuePair<TKey, ValueHolder>>
        {
            public static readonly UseCountComparer Instance = new UseCountComparer();

            int IComparer<KeyValuePair<TKey, ValueHolder>>.Compare(KeyValuePair<TKey, ValueHolder> x, KeyValuePair<TKey, ValueHolder> y)
            {
                return x.Value.UseCount.CompareTo(y.Value.UseCount);
            }
        }
        private sealed class ObjectEqualityComparer : IEqualityComparer
        {
            private readonly IEqualityComparer<TKey> _comparer;

            public ObjectEqualityComparer(IEqualityComparer<TKey> comparer)
            {
                _comparer = comparer;
            }

            public new bool Equals(object x, object y)
            {
                return _comparer.Equals((TKey) x, (TKey) y);
            }

            public int GetHashCode(object obj)
            {
                return _comparer.GetHashCode((TKey) obj);
            }
        }

        private readonly int _maxCount;
        private readonly Hashtable _hashTable;
        private int _remainingColdItems;
        private KeyValuePair<TKey, ValueHolder>[] _quickSelectArray;

        public object SyncRoot;

        public MicroCache(int maxCount, IEqualityComparer<TKey> comparer = null)
        {
            if (maxCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCount), maxCount, "must be positive");
            }

            _maxCount = maxCount;
            _hashTable = new Hashtable(comparer == null ? null : (comparer as IEqualityComparer) ?? new ObjectEqualityComparer(comparer));
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            ValueHolder holder = (ValueHolder)_hashTable[key];

            if (holder != null)
            {
                value = holder.Value;
                holder.Touch();

                return true;
            }

            value = default(TValue);

            return false;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            ValueHolder holder = (ValueHolder)_hashTable[key];

            if (holder != null)
                return false;

            lock (SyncRoot)
            {
                holder = (ValueHolder)_hashTable[key];

                if (holder != null)
                    return false;

                AddNoLock(key, new ValueHolder(value));

                return true;
            }
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (valueFactory == null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }

            if (TryGetValue(key, out TValue existing))
                return existing;

            TValue created = valueFactory(key);

            lock (SyncRoot)
            {
                ValueHolder holder = (ValueHolder)_hashTable[key];

                if (holder != null)
                {
                    holder.Touch();

                    return holder.Value;
                }

                AddNoLock(key, new ValueHolder(created));

                return created;
            }
        }

        private void AddNoLock(TKey key, ValueHolder value)
        {
            if (_hashTable.Count != _maxCount)
                return;

            if (_remainingColdItems == 0)
                IdentifyColdItemsNoLock();

            var indexToRemove = _remainingColdItems + 1;
            //var keyToRemove = _quickSelectArray[indexToRemove].Key;

            _hashTable.Remove(key);

            _quickSelectArray[indexToRemove] = new KeyValuePair<TKey, ValueHolder>(key, value);
            --_remainingColdItems;

            _hashTable.Add(key, value);
        }

        private void IdentifyColdItemsNoLock()
        {
            if (_quickSelectArray == null)
            {
                _quickSelectArray = new KeyValuePair<TKey, ValueHolder>[_maxCount];
                int i = 0;

                foreach (DictionaryEntry entry in _hashTable)
                {
                    _quickSelectArray[i++] = new KeyValuePair<TKey, ValueHolder>((TKey)entry.Key, (ValueHolder)entry.Value);
                }
            }

            int k = (_maxCount / 2) + (_maxCount & 1);

            KDTreeSelector.Select(_quickSelectArray, 0, _maxCount - 1, k, UseCountComparer.Instance);
            _remainingColdItems = k;
        }

        private int GetUseCount(int index)
        {
            return ((ValueHolder) _hashTable[_quickSelectArray[index]]).UseCount;
        }
    }
}
