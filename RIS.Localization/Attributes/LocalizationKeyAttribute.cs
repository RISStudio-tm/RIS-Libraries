// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Localization
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class LocalizationKeyAttribute : Attribute
    {
        public string Key { get; }

        public LocalizationKeyAttribute(
            string key)
        {
            Key = key;
        }
    }
}
