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
