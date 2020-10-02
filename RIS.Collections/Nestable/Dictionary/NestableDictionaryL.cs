// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RIS.Collections.Nestable
{
    public class NestableDictionaryL<T> : INestableDictionary<T>, ICollection, IEnumerable<T>, IEnumerable
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        public NestedElement<T> this[int index]
        {
            get
            {
                return Get(index);
            }
            set
            {
                Set(index, value);
            }
        }
        public NestedElement<T> this[string key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Set(key, value);
            }
        }

        private List<(string Key, NestedElementNode<T> Node)> ValuesCollection { get; }
        private Dictionary<string, (int Index, NestedElementNode<T> Node)> KeysCollection { get; }
        private long _nextRandomKey;
        private string NextRandomKey
        {
            get
            {
                ++_nextRandomKey;

                return _nextRandomKey.ToString();
            }
        }

        public int Length { get; private set; }
        public object SyncRoot { get; }
        public bool IsSynchronized { get; }
        public NestableCollectionType CollectionType { get; }
        private string _key;
        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                {
                    _key = NextRandomKey;
                    return;
                }

                _key = value;
            }
        }

        public NestableDictionaryL()
        {
            SyncRoot = new object();
            IsSynchronized = false;
            CollectionType = NestableCollectionHelper.GetCollectionType(GetType().Name);
            Key = NextRandomKey;

            Length = 0;
            ValuesCollection = new List<(string Key, NestedElementNode<T> Element)>();
            KeysCollection = new Dictionary<string, (int Index, NestedElementNode<T> Element)>();
        }
        public NestableDictionaryL(int length)
        {
            if (length < 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(length), "Длина коллекции не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            SyncRoot = new object();
            IsSynchronized = false;
            CollectionType = NestableCollectionHelper.GetCollectionType(GetType().Name);
            Key = NextRandomKey;

            Length = length;
            ValuesCollection = new List<(string Key, NestedElementNode<T> Element)>(length);
            KeysCollection = new Dictionary<string, (int Index, NestedElementNode<T> Element)>(length);

            for (int i = 0; i < Length; ++i)
            {
                string key = NextRandomKey;
                NestedElementNode<T> node = new NestedElementNode<T>(new NestedElement<T>());

                ValuesCollection.Add((key, node));
                KeysCollection.Add(key, (i, node));
            }
        }

        public void OnInformation(RInformationEventArgs e)
        {
            OnInformation(this, e);
        }
        public void OnInformation(object sender, RInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        public void OnWarning(RWarningEventArgs e)
        {
            OnWarning(this, e);
        }
        public void OnWarning(object sender, RWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        public void OnError(RErrorEventArgs e)
        {
            OnError(this, e);
        }
        public void OnError(object sender, RErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }

        public string GetKey(int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return ValuesCollection[index].Key;
        }

        public int GetIndex(string key)
        {
            if (!KeysCollection.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция не содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return KeysCollection[key].Index;
        }

        public NestedElement<T> Get(int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return ValuesCollection[index].Node.NestedElement;
        }
        public NestedElement<T> Get(string key)
        {
            if (!KeysCollection.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция не содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return KeysCollection[key].Node.NestedElement;
        }

        public ref NestedElement<T> GetRef(int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return ref ValuesCollection[index].Node.NestedElementRef;
        }
        public ref NestedElement<T> GetRef(string key)
        {
            if (!KeysCollection.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция не содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return ref KeysCollection[key].Node.NestedElementRef;
        }

        public void Set(int index, NestedElement<T> value)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            (string key, NestedElementNode<T> node) = ValuesCollection[index];

            node.Set(value);

            if (value.Type != NestedType.NestableCollection)
                return;

            INestableCollection<T> valueCollection = value.GetNestableCollection();

            switch (NestableCollectionHelper.GetGeneralType(valueCollection))
            {
                case CollectionGeneralType.Array:
                case CollectionGeneralType.List:
                    break;
                case CollectionGeneralType.Dictionary:
                    ((INestableDictionary<T>)valueCollection).Key = key;
                    break;
                case CollectionGeneralType.Unknown:
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение CollectionGeneralType у коллекции", nameof(value));
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
            }
        }
        public void Set(string key, NestedElement<T> value)
        {
            if (!KeysCollection.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция не содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            KeysCollection[key].Node.Set(value);

            if (value.Type != NestedType.NestableCollection)
                return;

            INestableCollection<T> valueCollection = value.GetNestableCollection();

            switch (NestableCollectionHelper.GetGeneralType(valueCollection))
            {
                case CollectionGeneralType.Array:
                case CollectionGeneralType.List:
                    break;
                case CollectionGeneralType.Dictionary:
                    ((INestableDictionary<T>)valueCollection).Key = key;
                    break;
                case CollectionGeneralType.Unknown:
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение CollectionGeneralType у коллекции", nameof(value));
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
            }
        }
        public void Set(int index, T value)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ValuesCollection[index].Node.Set(value);
        }
        public void Set(string key, T value)
        {
            if (!KeysCollection.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция не содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            KeysCollection[key].Node.Set(value);
        }
        public void Set(int index, T[] value)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ValuesCollection[index].Node.Set(value);
        }
        public void Set(string key, T[] value)
        {
            if (!KeysCollection.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция не содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            KeysCollection[key].Node.Set(value);
        }
        public void Set(int index, INestableCollection<T> value)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            (string key, NestedElementNode<T> node) = ValuesCollection[index];

            node.Set(value);

            switch (NestableCollectionHelper.GetGeneralType(value))
            {
                case CollectionGeneralType.Array:
                case CollectionGeneralType.List:
                    break;
                case CollectionGeneralType.Dictionary:
                    ((INestableDictionary<T>)value).Key = key;
                    break;
                case CollectionGeneralType.Unknown:
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение CollectionGeneralType у коллекции", nameof(value));
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
            }
        }
        public void Set(string key, INestableCollection<T> value)
        {
            if (!KeysCollection.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция не содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            KeysCollection[key].Node.Set(value);

            switch (NestableCollectionHelper.GetGeneralType(value))
            {
                case CollectionGeneralType.Array:
                case CollectionGeneralType.List:
                    break;
                case CollectionGeneralType.Dictionary:
                    ((INestableDictionary<T>)value).Key = key;
                    break;
                case CollectionGeneralType.Unknown:
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение CollectionGeneralType у коллекции", nameof(value));
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
            }
        }

        public bool ChangeKey(string oldKey, string newKey)
        {
            if (!KeysCollection.ContainsKey(oldKey))
            {
                var exception = new KeyNotFoundException("Коллекция не содержит элемент с указанным старым ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (string.IsNullOrEmpty(newKey) || string.IsNullOrWhiteSpace(newKey))
            {
                var exception = new Exception("Новый ключ не может быть null, пустым или содержать только пробелы");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (KeysCollection.ContainsKey(newKey))
            {
                var exception = new KeyNotFoundException("Коллекция уже содержит элемент с указанным новым ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            (int index, NestedElementNode<T> node) = KeysCollection[oldKey];

            KeysCollection.Add(newKey, (index, node));
            ValuesCollection[index] = (newKey, node);

            KeysCollection.Remove(oldKey);

            if (node.NestedElementRef.Type != NestedType.NestableCollection)
                return true;

            INestableCollection<T> valueCollection = node.NestedElementRef.GetNestableCollection();

            switch (NestableCollectionHelper.GetGeneralType(valueCollection))
            {
                case CollectionGeneralType.Array:
                case CollectionGeneralType.List:
                    break;
                case CollectionGeneralType.Dictionary:
                    ((INestableDictionary<T>)valueCollection).Key = newKey;
                    break;
                case CollectionGeneralType.Unknown:
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение CollectionGeneralType у коллекции", nameof(node.NestedElement));
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
            }

            return true;
        }

        public string ToStringRepresent()
        {
            return NestableCollectionHelper.ToStringRepresent<T>(this);
        }

        public void FromStringRepresent(string represent)
        {
            NestableCollectionHelper.FromStringRepresent<T>(represent, this);
        }

        public IEnumerable<T> Enumerate()
        {
            return NestableCollectionHelper.Enumerate<T>(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            IEnumerable<T> value = NestableCollectionHelper.Enumerate<T>(this);

            foreach (var element in value)
            {
                yield return element;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerable<T> value = NestableCollectionHelper.Enumerate<T>(this);

            foreach (var element in value)
            {
                yield return element;
            }
        }

        int ICollection.Count
        {
            get
            {
                return Length;
            }
        }

        public bool Add(NestedElement<T> value)
        {
            return Add(NextRandomKey, value);
        }
        public bool Add(string key, NestedElement<T> value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                var exception = new Exception("Ключ не может быть null, пустым или содержать только пробелы");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (KeysCollection.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция уже содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (Length == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            int index = Length;
            NestedElementNode<T> node = new NestedElementNode<T>(value);

            ValuesCollection.Add((key, node));
            KeysCollection.Add(key, (index, node));
            ++Length;

            if (value.Type != NestedType.NestableCollection)
                return true;

            INestableCollection<T> valueCollection = value.GetNestableCollection();

            switch (NestableCollectionHelper.GetGeneralType(valueCollection))
            {
                case CollectionGeneralType.Array:
                case CollectionGeneralType.List:
                    break;
                case CollectionGeneralType.Dictionary:
                    ((INestableDictionary<T>)valueCollection).Key = key;
                    break;
                case CollectionGeneralType.Unknown:
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение CollectionGeneralType у коллекции", nameof(value));
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
            }

            return true;
        }
        public bool Add(T value)
        {
            return Add(NextRandomKey, value);
        }
        public bool Add(string key, T value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                var exception = new Exception("Ключ не может быть null, пустым или содержать только пробелы");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (KeysCollection.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция уже содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (Length == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            int index = Length;
            NestedElementNode<T> node = new NestedElementNode<T>(value);

            ValuesCollection.Add((key, node));
            KeysCollection.Add(key, (index, node));
            ++Length;

            return true;
        }
        public bool Add(T[] value)
        {
            return Add(NextRandomKey, value);
        }
        public bool Add(string key, T[] value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                var exception = new Exception("Ключ не может быть null, пустым или содержать только пробелы");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (KeysCollection.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция уже содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (Length == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            int index = Length;
            NestedElementNode<T> node = new NestedElementNode<T>(value);

            ValuesCollection.Add((key, node));
            KeysCollection.Add(key, (index, node));
            ++Length;

            return true;
        }
        public bool Add(INestableCollection<T> value)
        {
            return Add(NextRandomKey, value);
        }
        public bool Add(string key, INestableCollection<T> value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                var exception = new Exception("Ключ не может быть null, пустым или содержать только пробелы");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (KeysCollection.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция уже содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (Length == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            int index = Length;
            NestedElementNode<T> node = new NestedElementNode<T>(value);

            ValuesCollection.Add((key, node));
            KeysCollection.Add(key, (index, node));
            ++Length;

            switch (NestableCollectionHelper.GetGeneralType(value))
            {
                case CollectionGeneralType.Array:
                case CollectionGeneralType.List:
                    break;
                case CollectionGeneralType.Dictionary:
                    ((INestableDictionary<T>)value).Key = key;
                    break;
                case CollectionGeneralType.Unknown:
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение CollectionGeneralType у коллекции", nameof(value));
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
            }

            return true;
        }

        public bool Insert(NestedElement<T> value, int index)
        {
            return Insert(NextRandomKey, value, index);
        }
        public bool Insert(string key, NestedElement<T> value, int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                var exception = new Exception("Ключ не может быть null, пустым или содержать только пробелы");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (KeysCollection.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция уже содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (Length == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            NestedElementNode<T> node = new NestedElementNode<T>(value);

            ValuesCollection.Insert(index, (key, node));
            KeysCollection.Add(key, (index, node));
            ++Length;

            for (int i = index + 1; i < ValuesCollection.Count; ++i)
            {
                (string nodeKey, NestedElementNode<T> nodeValue) = ValuesCollection[i];
                KeysCollection[nodeKey] = (i, nodeValue);
            }

            if (value.Type != NestedType.NestableCollection)
                return true;

            INestableCollection<T> valueCollection = value.GetNestableCollection();

            switch (NestableCollectionHelper.GetGeneralType(valueCollection))
            {
                case CollectionGeneralType.Array:
                case CollectionGeneralType.List:
                    break;
                case CollectionGeneralType.Dictionary:
                    ((INestableDictionary<T>)valueCollection).Key = key;
                    break;
                case CollectionGeneralType.Unknown:
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение CollectionGeneralType у коллекции", nameof(value));
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
            }

            return true;
        }
        public bool Insert(T value, int index)
        {
            return Insert(NextRandomKey, value, index);
        }
        public bool Insert(string key, T value, int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                var exception = new Exception("Ключ не может быть null, пустым или содержать только пробелы");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (KeysCollection.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция уже содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (Length == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            NestedElementNode<T> node = new NestedElementNode<T>(value);

            ValuesCollection.Insert(index, (key, node));
            KeysCollection.Add(key, (index, node));
            ++Length;

            for (int i = index + 1; i < ValuesCollection.Count; ++i)
            {
                (string nodeKey, NestedElementNode<T> nodeValue) = ValuesCollection[i];
                KeysCollection[nodeKey] = (i, nodeValue);
            }

            return true;
        }
        public bool Insert(T[] value, int index)
        {
            return Insert(NextRandomKey, value, index);
        }
        public bool Insert(string key, T[] value, int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                var exception = new Exception("Ключ не может быть null, пустым или содержать только пробелы");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (KeysCollection.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция уже содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (Length == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            NestedElementNode<T> node = new NestedElementNode<T>(value);

            ValuesCollection.Insert(index, (key, node));
            KeysCollection.Add(key, (index, node));
            ++Length;

            for (int i = index + 1; i < ValuesCollection.Count; ++i)
            {
                (string nodeKey, NestedElementNode<T> nodeValue) = ValuesCollection[i];
                KeysCollection[nodeKey] = (i, nodeValue);
            }

            return true;
        }
        public bool Insert(INestableCollection<T> value, int index)
        {
            return Insert(NextRandomKey, value, index);
        }
        public bool Insert(string key, INestableCollection<T> value, int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                var exception = new Exception("Ключ не может быть null, пустым или содержать только пробелы");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (KeysCollection.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция уже содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (Length == int.MaxValue)
            {
                var exception = new Exception("Нельзя вставить элемент, так как коллекция уже содержит максимальное их количество");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            NestedElementNode<T> node = new NestedElementNode<T>(value);

            ValuesCollection.Insert(index, (key, node));
            KeysCollection.Add(key, (index, node));
            ++Length;

            for (int i = index + 1; i < ValuesCollection.Count; ++i)
            {
                (string nodeKey, NestedElementNode<T> nodeValue) = ValuesCollection[i];
                KeysCollection[nodeKey] = (i, nodeValue);
            }

            switch (NestableCollectionHelper.GetGeneralType(value))
            {
                case CollectionGeneralType.Array:
                case CollectionGeneralType.List:
                    break;
                case CollectionGeneralType.Dictionary:
                    ((INestableDictionary<T>)value).Key = key;
                    break;
                case CollectionGeneralType.Unknown:
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение CollectionGeneralType у коллекции", nameof(value));
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
            }

            return true;
        }

        public bool Remove()
        {
            if (Length < 1)
            {
                var exception = new Exception("Нельзя удалить элемент, так как коллекция уже пустая");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            int index = ValuesCollection.Count - 1;

            KeysCollection.Remove(ValuesCollection[index].Key);
            ValuesCollection.RemoveAt(index);
            --Length;

            return true;
        }
        public bool Remove(int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (Length < 1)
            {
                var exception = new Exception("Нельзя удалить элемент, так как коллекция уже пустая");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            string key = ValuesCollection[index].Key;

            ValuesCollection.RemoveAt(index);
            KeysCollection.Remove(key);
            --Length;

            for (int i = index; i < ValuesCollection.Count; ++i)
            {
                (string nodeKey, NestedElementNode<T> nodeValue) = ValuesCollection[i];
                KeysCollection[nodeKey] = (i, nodeValue);
            }

            return true;
        }
        public bool Remove(string key)
        {
            if (!KeysCollection.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция не содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (Length < 1)
            {
                var exception = new Exception("Нельзя удалить элемент, так как коллекция уже пустая");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            int index = KeysCollection[key].Index;

            ValuesCollection.RemoveAt(index);
            KeysCollection.Remove(key);
            --Length;

            for (int i = index; i < ValuesCollection.Count; ++i)
            {
                (string nodeKey, NestedElementNode<T> nodeValue) = ValuesCollection[i];
                KeysCollection[nodeKey] = (i, nodeValue);
            }

            return true;
        }

        public void Clear()
        {
            Length = 0;
            KeysCollection.Clear();
            ValuesCollection.Clear();
        }

        public void CopyTo(Array array, int index)
        {
            if (array != null && array.Rank != 1)
            {
                var exception = new RankException("Копирование в многомерные массивы не поддерживается");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            if (array.GetValue(0).GetType() != typeof(T))
            {
                var exception = new ArrayTypeMismatchException("Для копирования тип целевого массива не может отличаться от типа текущей коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            List<T> thisCollection = Enumerate().ToList();

            if (thisCollection.Count < 1)
            {
                var exception = new Exception("Нельзя скопировать пустую коллекцию");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            if (array.Length - index < thisCollection.Count)
            {
                var exception = new Exception("Для копирования длина целевого массива, начиная с указанного индекса, не может быть меньше длины текущей коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            int arrayIndex = index;
            for (int i = 0; i < thisCollection.Count; ++i)
            {
                array.SetValue(thisCollection[i], arrayIndex);
                ++arrayIndex;
            }
        }
        public void CopyTo(IList<T> collection, bool clearBeforeCopy)
        {
            if (collection == null)
            {
                var exception = new ArgumentNullException(nameof(collection), "Целевая коллекция не может быть равна null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            List<T> thisCollection = Enumerate().ToList();

            if (thisCollection.Count < 1)
            {
                var exception = new Exception("Нельзя скопировать пустую коллекцию");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (clearBeforeCopy)
                collection.Clear();

            for (int i = 0; i < thisCollection.Count; ++i)
            {
                collection.Add(thisCollection[i]);
            }
        }
    }
}
