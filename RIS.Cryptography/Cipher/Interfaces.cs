// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.IO;

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

    public interface ISignMethod
    {
        bool Initialized { get; }

        string SignData(string dataEncoded);
        byte[] SignData(byte[] data);
        byte[] SignData(Stream data);

        bool VerifyData(string dataEncoded, string signDataEncoded);
        bool VerifyData(byte[] data, byte[] signData);
        bool VerifyData(Stream data, byte[] signData);
    }
}
