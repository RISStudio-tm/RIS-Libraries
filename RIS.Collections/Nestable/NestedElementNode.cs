﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

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

        public ref NestedElement<T> GetRef()
        {
            return ref _nestedElement;
        }

        public T GetElement()
        {
            return _nestedElement.GetElement();
        }
        public T[] GetArray()
        {
            return _nestedElement.GetArray();
        }
        public INestableCollection<T> GetCollection()
        {
            return _nestedElement.GetCollection();
        }

        public void Set(NestedElement<T> value)
        {
            _nestedElement.Set(value);
        }
        public void Set(T value)
        {
            _nestedElement.Set(value);
        }
        public void Set(T[] value)
        {
            _nestedElement.Set(value);
        }
        public void Set(INestableCollection<T> value)
        {
            _nestedElement.Set(value);
        }
    }
}
