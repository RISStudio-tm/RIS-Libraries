﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace RIS.Cryptography.Hash.Methods
{
    public sealed class MD5iCSP : IHashMethod
    {
        private MD5CryptoServiceProvider MDService { get; }

        public bool Initialized { get; }

        public MD5iCSP()
        {
            MDService = new MD5CryptoServiceProvider();
            MDService.Initialize();

            Initialized = true;
        }

        public string GetHash(string plainText)
        {
            byte[] data = Utils.SecureUTF8.GetBytes(plainText);

            return GetHash(data);
        }
        public string GetHash(byte[] data)
        {
            byte[] hashBytes = MDService.ComputeHash(data);

            StringBuilder hashText = new StringBuilder(hashBytes.Length * 2);
            for (int i = 0; i < hashBytes.Length; ++i)
            {
                hashText.Append(hashBytes[i].ToString("x2", CultureInfo.InvariantCulture));
            }

            return hashText.ToString();
        }
        public bool VerifyHash(string plainText, string hashText)
        {
            var plainTextHash = GetHash(plainText);

            return Utils.SecureEquals(plainTextHash, hashText, true, true);
        }
    }
}
