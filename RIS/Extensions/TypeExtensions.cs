// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Text;
using RIS.Extensions.Frames;

namespace RIS.Extensions
{
    public static class TypeExtensions
    {
        public static string GetAssemblyQualifiedName(this Type type)
        {
            if (type == null)
                return string.Empty;
            if (!type.IsGenericType)
                return $"{type.FullName}, {type.Assembly.GetName().Name}";

            var typeName =
                    new StringBuilder(50);
            var typeInfoFrames =
                new TypeGetAssemblyQualifiedNameFrame[5];
            var typeInfoFramesNextIndex = 0;

            typeInfoFrames[typeInfoFramesNextIndex++] =
                new TypeGetAssemblyQualifiedNameFrame(type);

            // Label
            StartNextTypeProcessing:



            ref var currentTypeInfo = ref typeInfoFrames[typeInfoFramesNextIndex - 1];
            ref var currentType = ref currentTypeInfo.Type;
            ref var currentGenericDefinition = ref currentTypeInfo.GenericDefinition;
            ref var currentGenericTypes = ref currentTypeInfo.GenericTypes;
            ref var currentIndex = ref currentTypeInfo.Index;

            if (currentGenericTypes.Length == 0)
                goto FinishTypeProcessing;

            currentIndex = 0;

            typeName.Append(currentGenericDefinition.FullName);
            typeName.Append('[');
            typeName.Append('[');

            // Label
            ContinuePreviousTypeProcessing:



            while (currentIndex < currentGenericTypes.Length)
            {
                ref var genericType = ref currentGenericTypes[currentIndex];

                if (!genericType.IsGenericType)
                {
                    typeName.Append($"{genericType.FullName}, {genericType.Assembly.GetName().Name}");
                    typeName.Append(',');
                    typeName.Append(' ');
                }
                else
                {
                    if (typeInfoFramesNextIndex == typeInfoFrames.Length)
                    {
                        Array.Resize(ref typeInfoFrames, typeInfoFrames.Length + 5);

                        currentTypeInfo = ref typeInfoFrames[typeInfoFramesNextIndex - 1];
                        currentType = ref currentTypeInfo.Type;
                        currentGenericDefinition = ref currentTypeInfo.GenericDefinition;
                        currentGenericTypes = ref currentTypeInfo.GenericTypes;
                        currentIndex = ref currentTypeInfo.Index;
                    }

                    typeInfoFrames[typeInfoFramesNextIndex++] =
                        new TypeGetAssemblyQualifiedNameFrame(genericType);

                    ++currentIndex;

                    goto StartNextTypeProcessing;
                }

                ++currentIndex;
            }

            if (typeName[typeName.Length - 2] == ',')
                typeName.Remove(typeName.Length - 2, 2);

            typeName.Append(']');
            typeName.Append(']');

            typeName.Append(',');
            typeName.Append(' ');
            typeName.Append(currentGenericDefinition.Assembly.GetName().Name);

            // Label
            FinishTypeProcessing:



            typeInfoFrames[typeInfoFramesNextIndex - 1] = default;
            --typeInfoFramesNextIndex;

            if (typeInfoFramesNextIndex == 0)
                return typeName.ToString();

            currentTypeInfo = ref typeInfoFrames[typeInfoFramesNextIndex - 1];
            currentType = ref currentTypeInfo.Type;
            currentGenericDefinition = ref currentTypeInfo.GenericDefinition;
            currentGenericTypes = ref currentTypeInfo.GenericTypes;
            currentIndex = ref currentTypeInfo.Index;

            goto ContinuePreviousTypeProcessing;
        }
    }
}
