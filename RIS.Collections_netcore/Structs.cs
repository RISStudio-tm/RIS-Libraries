﻿using System;
using RIS.Collections.NestableCollections;

namespace RIS.Collections
{
    public struct NestedElement<T>
    {
        private object _value;
        public object Value
        {
            get
            {
                if (_value == null)
                {
                    var exception =
                        new InvalidCastException("Поле Value в [NestedElement] содержит значение null, которое не может быть возвращено");
                    Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                return _value;
            }
            private set
            {
                if (value == null)
                {
                    var exception =
                        new InvalidCastException("Поле Value в [NestedElement] не может содержать значение null");
                    Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                if (value is T)
                    Type = NestedType.Element;
                else if (value is T[])
                    Type = NestedType.Array;
                else if (value is INestableCollection<T>)
                    Type = NestedType.NestableCollection;
                else
                {
                    var exception = 
                        new InvalidCastException("Поле Value в [NestedElement] не может содержать значение переданного типа");
                    Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                _value = value;
            }
        }
        public NestedType Type { get; private set; }

        public NestedElement(T value) : this()
        {
            Value = value;
            Type = NestedType.Element;
        }
        public NestedElement(T[] value) : this()
        {
            Value = value;
            Type = NestedType.Array;
        }
        public NestedElement(INestableCollection<T> value) : this()
        {
            Value = value;
            Type = NestedType.NestableCollection;
        }

        public void Set(T value)
        {
            Value = value;
        }
        public void Set(T[] value)
        {
            Value = value;
        }
        public void Set(INestableCollection<T> value)
        {
            Value = value;
        }

        public T GetElement()
        {
            if (Type != NestedType.Element)
            {
                var exception =
                    new InvalidCastException("Невозможно получить значение [NestedElement], так как оно содержит не тип Element");
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return (T)Value;
        }
        public T[] GetArray()
        {
            if (Type != NestedType.Array)
            {
                var exception =
                    new InvalidCastException("Невозможно получить значение [NestedElement], так как оно содержит не тип Array");
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return (T[])Value;
        }
        public INestableCollection<T> GetNestableCollection()
        {
            if (Type != NestedType.NestableCollection)
            {
                var exception =
                    new InvalidCastException("Невозможно получить значение [NestedElement], так как оно содержит не тип NestableCollection");
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return (INestableCollection<T>)Value;
        }

        public override bool Equals(object element)
        {
            if (element == null)
            {
                var exception = new ArgumentNullException(nameof(element), "Невозможно сравнить [NestedElement] и null");
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (!(element is NestedElement<T>))
            {
                var exception = new ArgumentNullException(nameof(element), "Невозможно сравнить [NestedElement] и другой тип");
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            NestedElement<T> nestedElement = (NestedElement<T>)element;
            return this.Value == nestedElement.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(NestedElement<T> element1, NestedElement<T> element2)
        {
            return element1.Value == element2.Value;
        }
        public static bool operator !=(NestedElement<T> element1, NestedElement<T> element2)
        {
            return element1.Value != element2.Value;
        }

        public static explicit operator T(NestedElement<T> param)
        {
            if (param.Value == null)
            {
                var exception = 
                    new Exception("Невозможно неявно преобразовать [NestedElement] к типу Element, так как поле Value равно null");
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (param.Type != NestedType.Element)
            {
                var exception = 
                    new InvalidCastException("Невозможно неявно преобразовать [NestedElement] к типу Element");
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return (T)param.Value;
        }
        public static explicit operator T[](NestedElement<T> param)
        {
            if (param.Value == null)
            {
                var exception = 
                    new Exception("Невозможно неявно преобразовать [NestedElement] к типу Array, так как поле Value равно null");
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (param.Type != NestedType.Array)
            {
                var exception = 
                    new InvalidCastException("Невозможно неявно преобразовать [NestedElement] к типу Array");
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return (T[])param.Value;
        }

        public static explicit operator NestedElement<T>(T param)
        {
            return new NestedElement<T>(param);
        }
        public static explicit operator NestedElement<T>(T[] param)
        {
            return new NestedElement<T>(param);
        }
        public static explicit operator NestedElement<T>(NestableArrayCAL<T> param)
        {
            return new NestedElement<T>(param);
        }
        public static explicit operator NestedElement<T>(NestableListL<T> param)
        {
            return new NestedElement<T>(param);
        }
    }
}
