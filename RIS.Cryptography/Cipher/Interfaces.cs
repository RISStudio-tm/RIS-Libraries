// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Cryptography.Cipher
{
    public interface ICipherMethod
    {
        bool Initialized { get; }

        string Encrypt(string plainText);
        byte[] Encrypt(byte[] plainText);

        string Decrypt(string cipherText);
        byte[] Decrypt(byte[] cipherText);
    }
}
