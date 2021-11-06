// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Runtime.CompilerServices;

namespace RIS.Collections.Nestable
{
    public class NestedElementNode<T>
    {
        private NestedElement<T> _nestedElement;

        public NestedElement<T> NestedElement
        {
            get
            {
                return GetRef();
            }
        }
        public ref NestedElement<T> NestedElementRef
        {
            get
            {
                return ref GetRef();
            }
        }



        public object Value
        {
            get
            {
                return _nestedElement.Value;
            }
        }
        public NestedType Type
        {
            get
            {
                return _nestedElement.Type;
            }
        }



        public NestedElementNode(NestedElement<T> value)
        {
            Set(value);
        }
        public NestedElementNode(T value)
        {
            Set(value);
        }
        public NestedElementNode(T[] value)
        {
            Set(value);
        }
        public NestedElementNode(INestableCollection<T> value)
        {
            Set(value);
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref NestedElement<T> GetRef()
        {
            return ref _nestedElement;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetElement()
        {
            return _nestedElement.GetElement();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] GetArray()
        {
            return _nestedElement.GetArray();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public INestableCollection<T> GetCollection()
        {
            return _nestedElement.GetCollection();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(NestedElement<T> value)
        {
            _nestedElement.Set(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(T value)
        {
            _nestedElement.Set(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(T[] value)
        {
            _nestedElement.Set(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(INestableCollection<T> value)
        {
            _nestedElement.Set(value);
        }
    }
}
