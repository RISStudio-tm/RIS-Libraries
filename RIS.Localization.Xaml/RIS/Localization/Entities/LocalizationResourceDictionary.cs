// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace RIS.Localization.Entities
{
    public class LocalizationResourceDictionary : ILocalizationDictionary
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


        public ObservableCollection<ILocalizationDictionary> MergedDictionaries
        {
            get
            {
                return new ObservableCollection<ILocalizationDictionary>(
                    Source.MergedDictionaries
                        .Select(dictionary => From(dictionary)));
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
            return ((IEnumerable)Source).GetEnumerator();
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
