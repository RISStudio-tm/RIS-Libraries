﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Settings.Ini
{
    public sealed class IniBoolString
    {
        public string String { get; set; }
        public bool Bool { get; set; }

        public IniBoolString(string word, bool value)
        {
            String = word;
            Bool = value;
        }
    }
}
