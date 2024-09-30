// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Cryptography.Hash
{
    public enum DefaultHashMethod : byte
    {
        //MD5iCSP = 1,
        //MD5iCNG = 2,
        RIPEMD160 = 3,
        //SHA1iCSP = 4,
        //SHA1iCNG = 5,
        //SHA256iCSP = 6,
        //SHA256iCNG = 7,
        //SHA384iCSP = 8,
        //SHA384iCNG = 9,
        //SHA512iCSP = 10,
        //SHA512iCNG = 11,
        CRC32 = 12,
        CRC32C = 13,
        CRC32D = 14,
        CRC32Q = 15,
        BCrypt = 16,
        Argon2iRaw = 17,
        Argon2iWNP = 18,
        Argon2iWP = 19,
        Argon2dRaw = 20,
        Argon2dWNP = 21,
        Argon2dWP = 22,
        Argon2idRaw = 23,
        Argon2idWNP = 24,
        Argon2idWP = 25,
        PHM1iArgon2id = 26,
        XXHash32 = 27,
        XXHash64 = 28,
        XXHash3b64 = 29,
        XXHash3b128 = 30,
        MD5 = 31,
        SHA1 = 32,
        SHA256 = 33,
        SHA384 = 34,
        SHA512 = 35
    }
}
