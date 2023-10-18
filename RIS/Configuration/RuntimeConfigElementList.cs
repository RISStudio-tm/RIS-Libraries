// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace RIS.Configuration
{
    public sealed class RuntimeConfigElementList : IEnumerable, IDisposable
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;



        public RuntimeConfigElement this[string key]
        {
            get
            {
                return Get(key);
            }

            private set
            {
                Set(key, value);
            }
        }



        private Dictionary<string, RuntimeConfigElement> Elements { get; }


        public int Count
        {
            get
            {
                return Elements.Count;
            }
        }
        public bool ThrowExceptions { get; }



        internal RuntimeConfigElementList(bool throwExceptions = true)
        {
            ThrowExceptions = throwExceptions;
            Elements = new Dictionary<string, RuntimeConfigElement>();
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



        private RuntimeConfigElement Get(string key)
        {
            if (!ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Элемент с таким ключом не существует");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));

                if (ThrowExceptions)
                    throw exception;

                return new RuntimeConfigElement(null, null);
            }

            return Elements[key];
        }


        private void Set(string key, RuntimeConfigElement value)
        {
            if (!ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Элемент с таким ключом не существует");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));

                if (ThrowExceptions)
                    throw exception;

                return;
            }

            Elements[key] = value;
        }



        internal bool Add(string key, RuntimeConfigElement value)
        {
            if (Count == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));

                if (ThrowExceptions)
                    throw exception;

                return false;
            }

            if (string.IsNullOrEmpty(key))
            {
                var exception = new ArgumentException("Ключ равен null или пустой строке");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));

                if (ThrowExceptions)
                    throw exception;

                return false;
            }

            if (ContainsKey(key))
            {
                var exception = new ArgumentException("Элемент с таким ключом уже существует");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));

                if (ThrowExceptions)
                    throw exception;

                return false;
            }

            Elements.Add(key, value);

            return true;
        }


        private bool Remove(string key)
        {
            if (Count < 1)
            {
                var exception = new Exception("Нельзя удалить элемент, так как коллекция уже пустая");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));

                if (ThrowExceptions)
                    throw exception;

                return false;
            }

            if (string.IsNullOrEmpty(key))
            {
                var exception = new ArgumentException("Ключ равен null или пустой строке");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));

                if (ThrowExceptions)
                    throw exception;

                return false;
            }

            if (!ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Элемент с таким ключом не существует");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));

                if (ThrowExceptions)
                    throw exception;

                return false;
            }

            Elements.Remove(key);

            return true;
        }



        public bool ContainsKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                var exception = new ArgumentException("Ключ равен null или пустой строке");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));

                if (ThrowExceptions)
                    throw exception;

                return false;
            }

            return Elements.ContainsKey(key);
        }


        internal void Clear()
        {
            if (Count != 0)
                Elements.Clear();
        }



        public IEnumerator GetEnumerator()
        {
            return Elements.GetEnumerator();
        }



        public void Dispose()
        {

        }
    }
}
