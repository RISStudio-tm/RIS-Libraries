// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS
{
    [AttributeUsage(AttributeTargets.Enum)]
    public sealed class DefaultEnumValueAttribute : Attribute
    {
        public Enum DefaultValue { get; }

        public DefaultEnumValueAttribute(object defaultValue)
        {
            DefaultValue = (Enum)defaultValue;
        }
    }
}
