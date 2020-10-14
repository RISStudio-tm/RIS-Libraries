// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SettingCategoryAttribute : Attribute
    {
        public string Name { get; }

        public SettingCategoryAttribute(string name)
        {
            Name = name;
        }
    }
}
