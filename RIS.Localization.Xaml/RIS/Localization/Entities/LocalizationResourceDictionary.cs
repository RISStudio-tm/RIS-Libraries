// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace RIS.Localization.Entities
{
    public sealed class LocalizationResourceDictionary : ILocalizationDictionary, IEnumerable<KeyValuePair<object, object>>
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


        public ICollection Keys
        {
            get
            {
                return Source.Keys;
            }
        }
        public ICollection Values
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


        bool ICollection.IsSynchronized
        {
            get
            {
                return ((ICollection)Source).IsSynchronized;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return Source.IsFixedSize;
            }
        }
        bool IDictionary.IsReadOnly
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

        public void Remove(object key)
        {
            Source.Remove(key);
        }

        public bool Contains(object key)
        {
            return Source.Contains(key);
        }

        public void Clear()
        {
            Source.Clear();
        }


        public IDictionaryEnumerator GetEnumerator()
        {
            return Source.GetEnumerator();
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


        void ICollection.CopyTo(Array array, int index)
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
