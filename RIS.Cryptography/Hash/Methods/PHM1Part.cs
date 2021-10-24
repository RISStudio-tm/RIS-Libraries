// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Cryptography.Hash.Methods
{
    public sealed class PHM1Part
    {
        public ushort Index { get; }
        public char? Content { get; set; }

        public PHM1Part(
            ushort index, char? content = null)
        {
            Index = index;
            Content = content;
        }
    }
}
