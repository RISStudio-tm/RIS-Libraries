// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RIS.Localization.Entities
{
    public sealed class LocalizationDictionary : ILocalizationDictionary
    {
        private class LocalizationDictionaryEnumerator : IDictionaryEnumerator
        {
            private readonly LocalizationDictionary _owner;
            private readonly IEnumerator _keysEnumerator;



            object IEnumerator.Current
            {
                get
                {
                    return ((IDictionaryEnumerator)this).Entry;
                }
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    var key = _keysEnumerator.Current;

                    return key != null
                        ? new DictionaryEntry(key, _owner[key])
                        : default;
                }
            }
            object IDictionaryEnumerator.Key
            {
                get
                {
                    return _keysEnumerator.Current;
                }
            }
            object IDictionaryEnumerator.Value
            {
                get
                {
                    var key = _keysEnumerator.Current;

                    return key != null
                        ? _owner[key]
                        : null;
                }
            }



            internal LocalizationDictionaryEnumerator(LocalizationDictionary owner)
            {
                _owner = owner;
                _keysEnumerator = _owner.Keys
                    .GetEnumerator();
            }



            bool IEnumerator.MoveNext()
            {
                return _keysEnumerator.MoveNext();
            }

            void IEnumerator.Reset()
            {
                _keysEnumerator.Reset();
            }
        }



        public object this[object key]
        {
            get
            {
                return GetValue(key);
            }
            set
            {
                SetValue(key, value);
            }
        }



        public Dictionary<object, object> Source { get; }


        private readonly List<ILocalizationDictionary> _mergedDictionaries;
        public ReadOnlyCollection<ILocalizationDictionary> MergedDictionaries
        {
            get
            {
                return new ReadOnlyCollection<ILocalizationDictionary>(
                    _mergedDictionaries);
            }
        }


        public ICollection<object> Keys
        {
            get
            {
                return Source.Keys;
            }
        }
        public ICollection<object> Values
        {
            get
            {
                return Source.Values;
            }
        }

        public object SyncRoot
        {
            get
            {
                return ((ICollection)Source).SyncRoot;
            }
        }
        public int Count
        {
            get
            {
                return Source.Count;
            }
        }



        bool ICollection<KeyValuePair<object, object>>.IsReadOnly
        {
            get
            {
                return ((IDictionary)Source).IsReadOnly;
            }
        }



        private LocalizationDictionary()
        {
            _mergedDictionaries = new List<ILocalizationDictionary>();
        }
        private LocalizationDictionary(Dictionary<object, object> dictionary)
            : this()
        {
            Source = dictionary;
        }



        public bool AddMergedDictionary(ILocalizationDictionary dictionary)
        {
            if (dictionary == null)
                return false;
            if (GetType() != dictionary.GetType())
                return false;

            return AddMergedDictionary((LocalizationDictionary)dictionary);
        }
        public bool AddMergedDictionary(LocalizationDictionary dictionary)
        {
            if (dictionary == null)
                return false;

            _mergedDictionaries.Add(
                dictionary);

            return true;
        }

        public bool InsertMergedDictionary(int index, ILocalizationDictionary dictionary)
        {
            if (dictionary == null)
                return false;
            if (GetType() != dictionary.GetType())
                return false;

            return InsertMergedDictionary(index, (LocalizationDictionary)dictionary);
        }
        public bool InsertMergedDictionary(int index, LocalizationDictionary dictionary)
        {
            if (dictionary == null)
                return false;
            if (index < 0 || index > _mergedDictionaries.Count)
                return false;

            _mergedDictionaries.Insert(
                index, dictionary);

            return true;
        }

        public bool RemoveMergedDictionary(ILocalizationDictionary dictionary)
        {
            if (dictionary == null)
                return false;
            if (GetType() != dictionary.GetType())
                return false;

            return RemoveMergedDictionary((LocalizationDictionary)dictionary);
        }
        public bool RemoveMergedDictionary(LocalizationDictionary dictionary)
        {
            if (dictionary == null)
                return false;

            return _mergedDictionaries.Remove(
                dictionary);
        }

        public bool RemoveAtMergedDictionary(int index)
        {
            if (index < 0)
                return false;

            _mergedDictionaries.RemoveAt(
                index);

            return true;
        }


        public object GetValue(object key)
        {
            if (Source.TryGetValue(key, out var value))
                return value;
            if (_mergedDictionaries == null)
                return null;

            for (int i = _mergedDictionaries.Count - 1; i > -1; --i)
            {
                var mergedDictionary = _mergedDictionaries[i];

                if (mergedDictionary == null)
                    continue;
                if (!mergedDictionary.TryGetValue(key, out value))
                    continue;

                break;
            }

            return value;
        }

        public void SetValue(object key, object value)
        {
            Source[key] = value;
        }


        public void Add(object key, object value)
        {
            Source.Add(key, value);
        }

        public bool Remove(object key)
        {
            return Source.Remove(key);
        }

        public bool ContainsKey(object key)
        {
            if (Source.ContainsKey(key))
                return true;
            if (_mergedDictionaries == null)
                return false;

            for (int i = _mergedDictionaries.Count - 1; i > -1; --i)
            {
                var mergedDictionary = _mergedDictionaries[i];

                if (mergedDictionary == null)
                    continue;
                if (!mergedDictionary.ContainsKey(key))
                    continue;

                return true;
            }

            return false;
        }

        public bool TryGetValue(object key, out object value)
        {
            if (Source.TryGetValue(key, out value))
                return true;
            if (_mergedDictionaries == null)
                return false;

            for (int i = _mergedDictionaries.Count - 1; i > -1; --i)
            {
                var mergedDictionary = _mergedDictionaries[i];

                if (mergedDictionary == null)
                    continue;
                if (!mergedDictionary.TryGetValue(key, out value))
                    continue;

                return true;
            }

            return false;
        }

        public void Clear()
        {
            Source.Clear();
        }


        public IDictionaryEnumerator GetEnumerator()
        {
            return (IDictionaryEnumerator)((IEnumerable)this).GetEnumerator();
        }



        void ICollection<KeyValuePair<object, object>>.Add(KeyValuePair<object, object> element)
        {
            Add(element.Key, element.Value);
        }

        bool ICollection<KeyValuePair<object, object>>.Remove(KeyValuePair<object, object> element)
        {
            return Remove(element.Key);
        }

        bool ICollection<KeyValuePair<object, object>>.Contains(KeyValuePair<object, object> element)
        {
            return ContainsKey(element.Key);
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return new LocalizationDictionaryEnumerator(this);
        }
        IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object, object>>.GetEnumerator()
        {
            var keysArray = Source.Keys.ToArray();
            var valuesArray = Source.Values.ToArray();

            for (int i = 0; i < Source.Count; ++i)
            {
                yield return new KeyValuePair<object, object>(
                    keysArray[i], valuesArray[i]);
            }
        }


        void ICollection<KeyValuePair<object, object>>.CopyTo(KeyValuePair<object, object>[] array, int index)
        {
            ((ICollection<KeyValuePair<object, object>>)Source).CopyTo(array, index);
        }



        public static implicit operator Dictionary<object, object>(LocalizationDictionary dictionary)
        {
            return dictionary.Source;
        }


        public static explicit operator LocalizationDictionary(Dictionary<object, object> dictionary)
        {
            return From(dictionary);
        }



        public static LocalizationDictionary From<T>(T dictionary)
            where T : Dictionary<object, object>
        {
            return new LocalizationDictionary(dictionary);
        }
    }
}
