﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Threading.Tasks;
using static RIS.Unions.UnionsHelper;

namespace RIS.Unions
{
    public class UnionBase<T0, T1> : IUnion
    {
        readonly T0 _value0;
        readonly T1 _value1;
        readonly int _index;

        protected UnionBase(Union<T0, T1> input)
        {
            _index = input.Index;
            switch (_index)
            {
                case 0: _value0 = input.AsT0; break;
                case 1: _value1 = input.AsT1; break;
                default: throw new InvalidOperationException();
            }
        }

        public object Value =>
            _index switch
            {
                0 => _value0,
                1 => _value1,
                _ => throw new InvalidOperationException()
            };

        public int Index => _index;

        public bool IsT0 => _index == 0;
        public bool IsT1 => _index == 1;

        public T0 AsT0 =>
            _index == 0 ?
                _value0 :
                throw new InvalidOperationException($"Cannot return as T0 as result is T{_index}");
        public T1 AsT1 =>
            _index == 1 ?
                _value1 :
                throw new InvalidOperationException($"Cannot return as T1 as result is T{_index}");

        

        public void Switch(Action<T0> f0, Action<T1> f1)
        {
            if (_index == 0 && f0 != null)
            {
                f0(_value0);
                return;
            }
            if (_index == 1 && f1 != null)
            {
                f1(_value1);
                return;
            }
            throw new InvalidOperationException();
        }
        
        public Task Switch(Func<T0, Task> f0, Func<T1, Task> f1)
        {
            if (_index == 0 && f0 != null)
            {                
                return f0(_value0);
            }
            if (_index == 1 && f1 != null)
            {                
                return f1(_value1);
            }
            throw new InvalidOperationException();
        }

        public TResult Match<TResult>(Func<T0, TResult> f0, Func<T1, TResult> f1)
        {
            if (_index == 0 && f0 != null)
            {
                return f0(_value0);
            }
            if (_index == 1 && f1 != null)
            {
                return f1(_value1);
            }
            throw new InvalidOperationException();
        }

        

        

        public bool TryPickT0(out T0 value, out T1 remainder)
        {
            value = IsT0 ? AsT0 : default;
            remainder = _index switch
            {
                0 => default,
                1 => AsT1,
                _ => throw new InvalidOperationException()
            };
            return this.IsT0;
        }
        
        public bool TryPickT1(out T1 value, out T0 remainder)
        {
            value = IsT1 ? AsT1 : default;
            remainder = _index switch
            {
                0 => AsT0,
                1 => default,
                _ => throw new InvalidOperationException()
            };
            return this.IsT1;
        }

        bool Equals(UnionBase<T0, T1> other) =>
            _index == other._index &&
            _index switch
            {
                0 => Equals(_value0, other._value0),
                1 => Equals(_value1, other._value1),
                _ => false
            };

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                    return true;
            }

            return obj is UnionBase<T0, T1> o && Equals(o);
        }

        public override string ToString() =>
            _index switch {
                0 => FormatValue(_value0),
                1 => FormatValue(_value1),
                _ => throw new InvalidOperationException("Unexpected index, which indicates a problem in the Union codegen.")
            };

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = _index switch
                {
                    0 => _value0?.GetHashCode(),
                    1 => _value1?.GetHashCode(),
                    _ => 0
                } ?? 0;
                return (hashCode*397) ^ _index;
            }
        }
    }
}
