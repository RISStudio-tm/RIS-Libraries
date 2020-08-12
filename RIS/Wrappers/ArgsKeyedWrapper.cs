// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;

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
        {
            _values = new Dictionary<string, object>();

            for (int i = 0; i < values.Length; ++i)
            {
                (string Key, object Value) value = values[i];

                _values.Add(value.Key, value.Value);
            }
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
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return _values[key];
        }
        public T Get<T>(string key)
        {
            if (!_values.ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Коллекция не содержит элемент с указанным ключом");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            object value = _values[key];

            if (!(value is T))
            {
                var exception = new Exception($"Значение элемента с ключом {key} невозможно привести к типу {typeof(T)}");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return (T)value;
        }
    }
}
