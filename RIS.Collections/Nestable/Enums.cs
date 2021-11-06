// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Collections.Nestable
{
    public enum NestedType : byte
    {
        Unknown = 0,
        Element = 1,
        Array = 2,
        Collection = 3
    }

    public enum NestableCollectionType : byte
    {
        Unknown = 0,
        NestableArrayCAL = 1,
        NestableDictionaryL = 2,
        NestableListL = 3
    }
}
