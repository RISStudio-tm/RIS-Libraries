// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Cryptography.Hash
{
    public enum BCryptHashType : sbyte
    {
        None = -1,
        SHA256 = 0,
        SHA384 = 1,
        SHA512 = 2,
        Legacy384 = 3,
    }

    public enum Argon2Type : byte
    {
        Argon2i = 1,
        Argon2d = 2,
        Argon2id = 3
    }
}
