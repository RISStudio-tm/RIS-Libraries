// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Cryptography.Cipher
{
    public enum DefaultCipherMethod : byte
    {
        Rijndael = 1,
        //RSAiCSP = 2,
        //RSAiCNG = 3,
        RSA = 4,
        AES = 5
    }

    public enum DefaultSignMethod : byte
    {
        RSA = 1
    }
}
