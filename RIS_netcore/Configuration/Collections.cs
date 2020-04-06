using System;
using System.Collections;
using System.Collections.Generic;

namespace RIS.Configuration
{
    public sealed class AppConfigElementList : IEnumerable, IDisposable
    {
        public event RMessageHandler ShowMessage;
        public event RErrorHandler ShowError;

        public AppConfigElement this[string key]
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

        private Dictionary<string, AppConfigElement> Elements { get; }
        public int Count
        {
            get
            {
                return Elements.Count;
            }
        }
        public bool ThrowExceptions { get; private set; }

        internal AppConfigElementList(bool throwExceptions = true)
        {
            Elements = new Dictionary<string, AppConfigElement>();
        }

        private AppConfigElement Get(string key)
        {
            if (!ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Элемент с таким ключом не существует");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));

                if (ThrowExceptions)
                    throw exception;

                return new AppConfigElement(null, null);
            }

            return Elements[key];
        }

        private void Set(string key, AppConfigElement value)
        {
            if (!ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Элемент с таким ключом не существует");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));

                if (ThrowExceptions)
                    throw exception;

                return;
            }

            Elements[key] = value;
        }

        internal bool Add(string key, AppConfigElement value)
        {
            if (Count == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));

                if (ThrowExceptions)
                    throw exception;

                return false;
            }

            if (string.IsNullOrEmpty(key))
            {
                var exception = new ArgumentException("Ключ равен null или пустой строке");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));

                if (ThrowExceptions)
                    throw exception;

                return false;
            }

            if (ContainsKey(key))
            {
                var exception = new ArgumentException("Элемент с таким ключом уже существует");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));

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
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));

                if (ThrowExceptions)
                    throw exception;

                return false;
            }

            if (string.IsNullOrEmpty(key))
            {
                var exception = new ArgumentException("Ключ равен null или пустой строке");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));

                if (ThrowExceptions)
                    throw exception;

                return false;
            }

            if (!ContainsKey(key))
            {
                var exception = new KeyNotFoundException("Элемент с таким ключом не существует");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));

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
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));

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
