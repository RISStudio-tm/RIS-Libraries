// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace RIS.Collections.Nestable
{
    public interface INestableKeyedEnumerable<T> : IKeyedEnumerable<string, NestedElement<T>>
    {

    }



    public interface INestableCollection : ICollection, IEnumerable
    {
        NestableCollectionType CollectionType { get; }
        int Length { get; }

        string ToStringRepresent();

        INestableCollection FromStringRepresent(string represent);

        bool Remove();

        void Clear();
    }

    public interface INestableCollection<T> : INestableCollection, IEnumerable<NestedElement<T>>
    {
        NestedElement<T> this[int index] { get; set; }

        NestedElement<T> Get(int index);

        ref NestedElement<T> GetRef(int index);

        void Set(int index, NestedElement<T> value);
        void Set(int index, T value);
        void Set(int index, T[] value);
        void Set(int index, INestableCollection<T> value);

        new INestableCollection<T> FromStringRepresent(string represent);

        IEnumerable<T> Enumerate();

        bool Add(NestedElement<T> value);
        bool Add(T value);
        bool Add(T[] value);
        bool Add(INestableCollection<T> value);

        void CopyTo(ICollection<T> collection, bool clearBeforeCopy);
    }

    public interface INestableArray : INestableCollection
    {
        new INestableArray FromStringRepresent(string represent);
    }

    public interface INestableArray<T> : INestableArray, INestableCollection<T>
    {
        new INestableArray<T> FromStringRepresent(string represent);
    }

    public interface INestableList : INestableCollection
    {
        new INestableList FromStringRepresent(string represent);

        bool Remove(int index);
    }

    public interface INestableList<T> : INestableList, INestableCollection<T>
    {
        new INestableList<T> FromStringRepresent(string represent);

        bool Insert(NestedElement<T> value, int index);
        bool Insert(T value, int index);
        bool Insert(T[] value, int index);
        bool Insert(INestableCollection<T> value, int index);
    }

    public interface INestableDictionary : INestableCollection
    {
        string Key { get; set; }

        string GetKey(int index);

        int GetIndex(string key);

        bool ChangeKey(string oldKey, string newKey);

        bool ContainsKey(string key);

        new INestableDictionary FromStringRepresent(string represent);

        bool Remove(int index);
        bool Remove(string key);
    }

    // ReSharper disable PossibleInterfaceMemberAmbiguity
    public interface INestableDictionary<T> : INestableDictionary, INestableCollection<T>, INestableKeyedEnumerable<T>
    {
        NestedElement<T> this[string key] { get; set; }

        NestedElement<T> Get(string key);

        ref NestedElement<T> GetRef(string key);

        void Set(string key, NestedElement<T> value);
        void Set(string key, T value);
        void Set(string key, T[] value);
        void Set(string key, INestableCollection<T> value);

        new INestableDictionary<T> FromStringRepresent(string represent);

        IEnumerable<KeyValuePair<string, NestedElement<T>>> AsKeyedEnumerable();

        bool Add(string key, NestedElement<T> value);
        bool Add(string key, T value);
        bool Add(string key, T[] value);
        bool Add(string key, INestableCollection<T> value);

        bool Insert(NestedElement<T> value, int index);
        bool Insert(string key, NestedElement<T> value, int index);
        bool Insert(T value, int index);
        bool Insert(string key, T value, int index);
        bool Insert(T[] value, int index);
        bool Insert(string key, T[] value, int index);
        bool Insert(INestableCollection<T> value, int index);
        bool Insert(string key, INestableCollection<T> value, int index);

        void CopyTo(ICollection<KeyValuePair<string, T>> collection, bool clearBeforeCopy);
        void CopyTo(IDictionary<string, T> collection, bool clearBeforeCopy);
    }
    // ReSharper restore PossibleInterfaceMemberAmbiguity
}
