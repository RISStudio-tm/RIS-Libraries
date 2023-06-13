// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Settings.Ini
{
    public sealed class IniSetting
    {
        public string Name { get; }
        public string Value { get; set; }

        public IniSetting(string name)
        {
            Name = name;
            Value = string.Empty;
        }
        public IniSetting(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Name ?? string.Empty}={Value ?? string.Empty}";
        }
    }
}
