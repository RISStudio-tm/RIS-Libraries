// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Cryptography.Hash
{
    public interface IHashMethod
    {
        bool Initialized { get; }

        string GetHash(string plainText);
        string GetHash(byte[] data);

        bool VerifyHash(string plainText, string hashText);
        bool VerifyHash(byte[] data, string hashText);
    }
}
