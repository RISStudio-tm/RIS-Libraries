﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace RIS.Cryptography.Hash.Methods
{

#if NETFRAMEWORK

    public sealed class SHA1iCNG : IHashMethod
    {
        private SHA1Cng SHAService { get; }

        public bool Initialized { get; }

        public SHA1iCNG()
        {
            SHAService = new SHA1Cng();
            SHAService.Initialize();

            Initialized = true;
        }

        public string GetHash(string plainText)
        {
            byte[] data = Utils.SecureUTF8.GetBytes(plainText);

            return GetHash(data);
        }
        public string GetHash(byte[] data)
        {
            byte[] hashBytes = SHAService.ComputeHash(data);

            StringBuilder hashText = new StringBuilder(hashBytes.Length * 2);
            for (int i = 0; i < hashBytes.Length; ++i)
            {
                hashText.Append(hashBytes[i].ToString("x2", CultureInfo.InvariantCulture));
            }

            return hashText.ToString();
        }
        public bool VerifyHash(string plainText, string hashText)
        {
            string plainTextHash = GetHash(plainText);

            return Utils.SecureEquals(plainTextHash, hashText, true, true);
        }
    }

#endif

}
