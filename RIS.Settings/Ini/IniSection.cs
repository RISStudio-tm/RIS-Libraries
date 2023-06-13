// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;

namespace RIS.Settings.Ini
{
    public sealed class IniSection
    {
        public string Name { get; }
        public Dictionary<string, IniSetting> Settings { get; }

        public IniSection(string name, StringComparer comparer = null)
        {
            Name = name;
            Settings = new Dictionary<string, IniSetting>(comparer ?? StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
