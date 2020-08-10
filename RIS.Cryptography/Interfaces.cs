// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Cryptography
{
    public interface ICipherMethod
    {
        bool Initialized { get; }

        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }

    public interface IHashMethod
    {
        bool Initialized { get; }

        string GetHash(string plainText);
        bool VerifyHash(string plainText, string hashText);
    }
}
