// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Linq;

namespace RIS.Wrappers
{
    public class ArgsWrapper
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        private readonly List<object> _values;

        public object this[int index]
        {
            get
            {
                return Get(index);
            }
        }

        public ArgsWrapper(object[] values)
        {
            _values = new List<object>();

            for (int i = 0; i < values.Length; ++i)
            {
                _values.Add(values[i]);
            }
        }
        public ArgsWrapper(IList<object> values)
        {
            _values = new List<object>();

            for (int i = 0; i < values.Count; ++i)
            {
                _values.Add(values[i]);
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

        public object Get(int index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > _values.Count - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            return _values[index];
        }
        public T Get<T>(int index)
        {
            object value = Get(index);

            if (!(value is T))
            {
                var exception =
                    new Exception($"Значение элемента с индексом {index} невозможно привести к типу {typeof(T)}");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            return (T)value;
        }

        public IEnumerable<object> EnumerateValues()
        {
            return _values;
        }
        public IEnumerable<T> EnumerateValues<T>()
        {
            return EnumerateValues().OfType<T>();
        }
    }
}
