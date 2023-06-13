// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Extensions.Frames
{
    internal struct TypeGetAssemblyQualifiedNameFrame
    {
        public Type Type;
        public int Index;

        public Type GenericDefinition;
        public Type[] GenericTypes;



        public TypeGetAssemblyQualifiedNameFrame(
            Type type, int index = -1)
        {
            Type = type;
            Index = index;

            if (type.IsGenericType)
            {
                GenericDefinition = type.GetGenericTypeDefinition();
                GenericTypes = type.GetGenericArguments();
            }
            else
            {
                GenericDefinition = type;
                GenericTypes = Array.Empty<Type>();
            }
        }
    }
}
