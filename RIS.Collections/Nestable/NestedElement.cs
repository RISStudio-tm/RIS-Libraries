// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Collections.Nestable
{
    public struct NestedElement<T> : IEquatable<NestedElement<T>>
    {
        private object _value;
        public object Value
        {
            get
            {
                return _value;
            }
            private set
            {
                if (value == null)
                {
                    Type = NestedType.Element;
                    _value = null;

                    return;
                }

                switch (value)
                {
                    case T _:
                        Type = NestedType.Element;
                        break;
                    case T[] _:
                        Type = NestedType.Array;
                        break;
                    case INestableCollection<T> _:
                        Type = NestedType.NestableCollection;
                        break;
                    default:
                        var exception =
                            new Exception("Поле Value в [NestedElement] не может содержать значение переданного типа");
                        Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                        throw exception;
                }

                _value = value;
            }
        }
        public NestedType Type { get; private set; }

        public NestedElement(T value) : this()
        {
            Value = value;
        }
        public NestedElement(T[] value) : this()
        {
            Value = value;
        }
        public NestedElement(INestableCollection<T> value) : this()
        {
            Value = value;
        }

        public void Set(NestedElement<T> value)
        {
            _value = value.Value;
            Type = value.Type;
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
            if (Value == null)
                return default;

            if (Type != NestedType.Element)
            {
                var exception =
                    new InvalidCastException("Невозможно получить значение [NestedElement], так как оно содержит не тип Element");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            return (T)Value;
        }
        public T[] GetArray()
        {
            if (Value == null)
                return null;

            if (Type != NestedType.Array)
            {
                var exception =
                    new InvalidCastException("Невозможно получить значение [NestedElement], так как оно содержит не тип Array");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            return (T[])Value;
        }
        public INestableCollection<T> GetNestableCollection()
        {
            if (Value == null)
                return null;

            if (Type != NestedType.NestableCollection)
            {
                var exception =
                    new InvalidCastException("Невозможно получить значение [NestedElement], так как оно содержит не тип NestableCollection");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            return (INestableCollection<T>)Value;
        }

        public override bool Equals(object element)
        {
            if (element == null)
            {
                var exception = new ArgumentNullException(nameof(element), "Невозможно сравнить [NestedElement] и null");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            if (!(element is NestedElement<T>))
            {
                var exception = new ArgumentNullException(nameof(element), "Невозможно сравнить [NestedElement] и другой тип");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            NestedElement<T> nestedElement = (NestedElement<T>)element;

            return Type == nestedElement.Type
                   &&_value.Equals(nestedElement._value);
        }
        public bool Equals(NestedElement<T> nestedElement)
        {
            return Type == nestedElement.Type
                   && _value.Equals(nestedElement._value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return NestableCollectionHelper.ToStringRepresent(this);
        }

        public static bool operator ==(NestedElement<T> element1, NestedElement<T> element2)
        {
            return element1.Equals(element2);
        }
        public static bool operator !=(NestedElement<T> element1, NestedElement<T> element2)
        {
            return !element1.Equals(element2);
        }

        public static explicit operator T(NestedElement<T> param)
        {
            if (param.Value == null)
                return default;

            if (param.Type != NestedType.Element)
            {
                var exception =
                    new InvalidCastException("Невозможно преобразовать [NestedElement] к типу Element");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            return (T)param.Value;
        }
        public static explicit operator T[](NestedElement<T> param)
        {
            if (param.Value == null)
                return null;

            if (param.Type != NestedType.Array)
            {
                var exception =
                    new InvalidCastException("Невозможно преобразовать [NestedElement] к типу Array");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
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
        public static explicit operator NestedElement<T>(NestableDictionaryL<T> param)
        {
            return new NestedElement<T>(param);
        }
        public static explicit operator NestedElement<T>(NestableListL<T> param)
        {
            return new NestedElement<T>(param);
        }
    }
}
