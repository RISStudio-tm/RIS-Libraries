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
                    Type = NestedType.Unknown;
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
                        Type = NestedType.Collection;
                        break;
                    default:
                        var exception = new Exception(
                            "Поле Value в [NestedElement] не может содержать значение переданного типа");
                        Events.OnError(this,
                            new RErrorEventArgs(exception, exception.Message));
                        throw exception;
                }

                _value = value;
            }
        }
        public NestedType Type { get; private set; }



        public NestedElement(T value)
            : this()
        {
            Set(value);
        }
        public NestedElement(T[] value)
            : this()
        {
            Set(value);
        }
        public NestedElement(INestableCollection<T> value)
            : this()
        {
            Set(value);
        }



        public void Set(NestedElement<T> value)
        {
            _value = value.Value;
            Type = value.Type;
        }
        public void Set(T value)
        {
            if (value == null)
            {
                Type = NestedType.Element;
                _value = null;

                return;
            }

            Value = value;
        }
        public void Set(T[] value)
        {
            if (value == null)
            {
                Type = NestedType.Array;
                _value = null;

                return;
            }

            Value = value;
        }
        public void Set(INestableCollection<T> value)
        {
            if (value == null)
            {
                var exception = new ArgumentNullException(
                    nameof(value),
                    "Передаваемая коллекция не может быть равна null");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            Value = value;
        }

        public T GetElement()
        {
            if (Type != NestedType.Element)
            {
                var exception = new InvalidCastException(
                    "Невозможно получить значение [NestedElement], так как оно содержит не тип Element");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (Value == null)
                return default;

            return (T)Value;
        }
        public T[] GetArray()
        {
            if (Type != NestedType.Array)
            {
                var exception = new InvalidCastException(
                    "Невозможно получить значение [NestedElement], так как оно содержит не тип Array");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (Value == null)
                return null;

            return (T[])Value;
        }
        public INestableCollection<T> GetCollection()
        {
            if (Type != NestedType.Collection)
            {
                var exception = new InvalidCastException(
                    "Невозможно получить значение [NestedElement], так как оно содержит не тип Collection");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (Value == null)
                return null;

            return (INestableCollection<T>)Value;
        }



        public override bool Equals(object element)
        {
            if (element == null)
                return false;
            else if (!(element is NestedElement<T>))
                return false;

            var nestedElement = (NestedElement<T>)element;

            return Type == nestedElement.Type
                   && _value.Equals(nestedElement._value);
        }
        public bool Equals(NestedElement<T> nestedElement)
        {
            return Type == nestedElement.Type
                   && _value.Equals(nestedElement._value);
        }

#pragma warning disable SS008 // GetHashCode() refers to mutable, static, or constant member
        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
        // ReSharper restore NonReadonlyMemberInGetHashCode
#pragma warning restore SS008 // GetHashCode() refers to mutable, static, or constant member

        public override string ToString()
        {
            return NestableHelper.ToStringRepresent(this);
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
            return param.GetElement();
        }
        public static explicit operator T[](NestedElement<T> param)
        {
            return param.GetArray();
        }
        public static explicit operator NestableArrayCAL<T>(NestedElement<T> param)
        {
            return (NestableArrayCAL<T>)param.GetCollection();
        }
        public static explicit operator NestableDictionaryL<T>(NestedElement<T> param)
        {
            return (NestableDictionaryL<T>)param.GetCollection();
        }
        public static explicit operator NestableListL<T>(NestedElement<T> param)
        {
            return (NestableListL<T>)param.GetCollection();
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
