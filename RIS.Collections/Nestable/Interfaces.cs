using System;
using System.Collections.Generic;

namespace RIS.Collections.Nestable
{
    public interface INestableCollection<T>
    {
        NestedElement<T> this[int index] { get; set; }

        int Length { get; }
        NestableCollectionType CollectionType { get; }

        NestedElement<T> Get(int index);

        ref NestedElement<T> GetRef(int index);

        string ToStringRepresent();

        void FromStringRepresent(string represent);

        IEnumerable<T> Enumerate();

        bool Add(NestedElement<T> value);
        bool Add(T value);
        bool Add(T[] value);
        bool Add(INestableCollection<T> value);

        bool Remove();

        void Clear();

        void CopyTo(IList<T> collection, bool clearBeforeCopy);
    }

    public interface INestableArray<T> : INestableCollection<T>
    {
        ref NestedElement<T> GetRef(int index);

        void Set(int index, NestedElement<T> value);
        void Set(int index, T value);
        void Set(int index, T[] value);
        void Set(int index, INestableCollection<T> value);
    }

    public interface INestableList<T> : INestableCollection<T>
    {
        void Set(int index, NestedElement<T> value);
        void Set(int index, T value);
        void Set(int index, T[] value);
        void Set(int index, INestableCollection<T> value);

        bool Insert(NestedElement<T> value, int index);
        bool Insert(T value, int index);
        bool Insert(T[] value, int index);
        bool Insert(INestableCollection<T> value, int index);

        bool Remove(int index);
    }

    public interface INestableDictionary<T> : INestableCollection<T>
    {
        NestedElement<T> this[string key] { get; set; }

        string Key { get; set; }

        string GetKey(int index);

        int GetIndex(string key);

        NestedElement<T> Get(string key);

        ref NestedElement<T> GetRef(string key);

        void Set(int index, NestedElement<T> value);
        void Set(string key, NestedElement<T> value);
        void Set(int index, T value);
        void Set(string key, T value);
        void Set(int index, T[] value);
        void Set(string key, T[] value);
        void Set(int index, INestableCollection<T> value);
        void Set(string key, INestableCollection<T> value);

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

        bool Remove(int index);
        bool Remove(string key);
    }
}
