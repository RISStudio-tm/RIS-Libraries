﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Globalization;

namespace RIS.Cryptography.Hash.Methods
{
    public sealed class CRC32 : IHashMethod
    {
        private Algorithms.CRC32 CRCService { get; }

        public bool Initialized { get; }

        public CRC32()
        {
            CRCService = new Algorithms.CRC32();
            CRCService.Initialize();

            Initialized = true;
        }

        public string GetHash(string plainText)
        {
            byte[] data = SecureUtils.GetBytes(plainText);

            return GetHash(data);
        }
        public string GetHash(byte[] data)
        {
            uint hashValue = BitConverter.ToUInt32(
                CRCService.ComputeHash(data), 0);

            return hashValue.ToString(
                "x2", CultureInfo.InvariantCulture);
        }

        public bool VerifyHash(string plainText, string hashText)
        {
            byte[] data = SecureUtils.GetBytes(plainText);

            return VerifyHash(data, hashText);
        }
        public bool VerifyHash(byte[] data, string hashText)
        {
            var plainTextHash = GetHash(data);

            return SecureUtils.SecureEquals(plainTextHash, hashText,
                true, null);
        }
    }
}
