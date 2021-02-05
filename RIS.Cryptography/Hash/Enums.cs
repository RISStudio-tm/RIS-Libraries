// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Cryptography.Hash
{
    public enum DefaultHashMethod : byte
    {
        MD5iCSP = 1,
#if NETFRAMEWORK
        MD5iCNG = 2,
#endif
        RIPEMD160 = 3,
        SHA1iCSP = 4,
#if NETFRAMEWORK
        SHA1iCNG = 5,
#endif
        SHA256iCSP = 6,
#if NETFRAMEWORK
        SHA256iCNG = 7,
#endif
        SHA384iCSP = 8,
#if NETFRAMEWORK
        SHA384iCNG = 9,
#endif
        SHA512iCSP = 10,
#if NETFRAMEWORK
        SHA512iCNG = 11,
#endif
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
        Argon2idWP = 25
    }

    public enum BCryptHashType : sbyte
    {
        None = -1,
        SHA256 = 0,
        SHA384 = 1,
        SHA512 = 2,
        Legacy384 = 3
    }

    public enum Argon2Type : byte
    {
        Argon2i = 1,
        Argon2d = 2,
        Argon2id = 3
    }
}
