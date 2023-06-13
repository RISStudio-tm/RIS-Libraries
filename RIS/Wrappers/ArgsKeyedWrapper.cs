// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace RIS.Wrappers
{
    public class ArgsKeyedWrapper
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        private readonly Dictionary<string, object> _values;

        public object this[string key]
        {
            get
            {
                return Get(key);
            }
        }

        public ArgsKeyedWrapper((string Key, object Value)[] values)
            : this(values.ToList())
        {

        }
        public ArgsKeyedWrapper(IList<(string Key, object Value)> values)
        {
            _values = new Dictionary<string, object>();

            for (int i = 0; i < values.Count; ++i)
            {
                (string Key, object Value) value = values[i];

                _values.Add(value.Key, value.Value);
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

        public object Get(string key)
        {
            if (!_values.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция не содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return _values[key];
        }
        public T Get<T>(string key)
        {
            object value = Get(key);

            if (!(value is T))
            {
                var exception = new Exception($"Значение элемента с ключом {key} невозможно привести к типу {typeof(T)}");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return (T)value;
        }

        public string GetKey(int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
            else if (index > _values.Count - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return _values.Keys.ToArray()[index];
        }

        public object GetValue(int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
            else if (index > _values.Count - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return _values.Values.ToArray()[index];
        }
        public T GetValue<T>(int index)
        {
            object value = GetValue(index);

            if (!(value is T))
            {
                var exception = new Exception($"Значение элемента с индексом {index} невозможно привести к типу {typeof(T)}");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return (T)value;
        }

        public IEnumerable<KeyValuePair<string, object>> Enumerate()
        {
            return _values.ToList();
        }
        public IEnumerable<KeyValuePair<string, T>> Enumerate<T>()
        {
            return Enumerate().Select(pair =>
            {
                return new KeyValuePair<string, T>(pair.Key, (T)pair.Value);
            });
        }

        public IEnumerable<string> EnumerateKeys()
        {
            return _values.Keys;
        }

        public IEnumerable<object> EnumerateValues()
        {
            return _values.Values;
        }
        public IEnumerable<T> EnumerateValues<T>()
        {
            return EnumerateValues().OfType<T>();
        }
    }
}
