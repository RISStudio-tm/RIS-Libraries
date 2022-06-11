// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using RIS.Extensions;

namespace RIS.Localization.Entities
{
    public sealed class LocalizationResourceDictionary : ILocalizationDictionary
    {
        public object this[object key]
        {
            get
            {
                return Source[key];
            }
            set
            {
                Source[key] = value;
            }
        }



        public ResourceDictionary Source { get; }


        public ReadOnlyCollection<ILocalizationDictionary> MergedDictionaries
        {
            get
            {
                return new ReadOnlyCollection<ILocalizationDictionary>(
                    Source.MergedDictionaries
                        .Select(dictionary => (ILocalizationDictionary)From(dictionary))
                        .ToList());
            }
        }


        public ICollection<object> Keys
        {
            get
            {
                return (ICollection<object>)Source.Keys;
            }
        }
        public ICollection<object> Values
        {
            get
            {
                return (ICollection<object>)Source.Values;
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
                return Source.IsReadOnly;
            }
        }



        private LocalizationResourceDictionary(ResourceDictionary dictionary)
        {
            Source = dictionary;
        }



        public bool AddMergedDictionary(ILocalizationDictionary dictionary)
        {
            if (dictionary == null)
                return false;
            if (GetType() != dictionary.GetType())
                return false;

            return AddMergedDictionary((LocalizationResourceDictionary)dictionary);
        }
        public bool AddMergedDictionary(LocalizationResourceDictionary dictionary)
        {
            if (dictionary == null)
                return false;

            Source.MergedDictionaries.Add(
                dictionary.Source);

            return true;
        }

        public bool InsertMergedDictionary(int index, ILocalizationDictionary dictionary)
        {
            if (dictionary == null)
                return false;
            if (GetType() != dictionary.GetType())
                return false;

            return InsertMergedDictionary(index, (LocalizationResourceDictionary)dictionary);
        }
        public bool InsertMergedDictionary(int index, LocalizationResourceDictionary dictionary)
        {
            if (dictionary == null)
                return false;
            if (index < 0 || index > Source.MergedDictionaries.Count)
                return false;

            Source.MergedDictionaries.Insert(
                index, dictionary.Source);

            return true;
        }

        public bool RemoveMergedDictionary(ILocalizationDictionary dictionary)
        {
            if (dictionary == null)
                return false;
            if (GetType() != dictionary.GetType())
                return false;

            return RemoveMergedDictionary((LocalizationResourceDictionary)dictionary);
        }
        public bool RemoveMergedDictionary(LocalizationResourceDictionary dictionary)
        {
            if (dictionary == null)
                return false;

            return Source.MergedDictionaries.Remove(
                dictionary.Source);
        }

        public bool RemoveAtMergedDictionary(int index)
        {
            if (index < 0)
                return false;

            Source.MergedDictionaries.RemoveAt(
                index);

            return true;
        }


        public void Add(object key, object value)
        {
            Source.Add(key, value);
        }

        public bool Remove(object key)
        {
            if (!Source.Contains(key))
                return false;

            Source.Remove(key);

            return true;
        }

        public bool ContainsKey(object key)
        {
            return Source.Contains(key);
        }

        public bool TryGetValue(object key, out object value)
        {
            static int IndexOfInternal(ICollection<object> keys, object targetKey)
            {
                for (int i = 0; i < keys.Count; ++i)
                {
                    var indexes = keys
                        .IndexesWhere(elementKey => elementKey == targetKey)
                        .ToArray();

                    if (indexes.Length != 0)
                        return indexes[0];
                }

                return -1;
            }

            static bool TryGetValueInternal(IDictionary<object, object> dictionary, object key, out object value) 
            {
                var index = IndexOfInternal(dictionary.Keys, key);

                if (index >= 0)
                {
                    value = dictionary.Values.ElementAt(index);

                    return true;
                }

                value = default;

                return false;
            }



            if (TryGetValueInternal(this, key, out value))
                return true;

            var mergedDictionaries = MergedDictionaries;

            for (int i = mergedDictionaries.Count - 1; i > -1; --i)
            {
                var mergedDictionary = mergedDictionaries[i];

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
            return ((IDictionary)Source).GetEnumerator();
        }
        IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object, object>>.GetEnumerator()
        {
            if (!(Source.Keys is object[] keysArray)
                || !(Source.Values is object[] valuesArray))
            {
                yield break;
            }

            for (int i = 0; i < Source.Count; ++i)
            {
                yield return new KeyValuePair<object, object>(
                    keysArray[i], valuesArray[i]);
            }
        }


        void ICollection<KeyValuePair<object, object>>.CopyTo(KeyValuePair<object, object>[] array, int index)
        {
            ((ICollection)Source).CopyTo(array, index);
        }



        public static implicit operator ResourceDictionary(LocalizationResourceDictionary dictionary)
        {
            return dictionary.Source;
        }


        public static explicit operator LocalizationResourceDictionary(ResourceDictionary dictionary)
        {
            return From(dictionary);
        }



        public static LocalizationResourceDictionary From<T>(T dictionary)
            where T : ResourceDictionary
        {
            return new LocalizationResourceDictionary(dictionary);
        }
    }
}
