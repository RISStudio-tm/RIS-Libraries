﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Configuration
{
    public struct RuntimeConfigElement
    {
        public string Name { get; }
        public string JsonPath { get; }



        internal RuntimeConfigElement(
            string name, string jsonPath)
        {
            Name = name;
            JsonPath = jsonPath;
        }
    }
}
