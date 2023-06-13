// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Reflection.Mapping
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class MappedMethodAttribute : Attribute
    {
        public string Name { get; }

        public MappedMethodAttribute(string name = null)
        {
            Name = name;
        }
    }
}
