// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace RIS.Cryptography.Hash.Methods
{

#if NETFRAMEWORK

    public sealed class MD5iCNG : IHashMethod
    {
        private MD5Cng MDService { get; }

        public bool Initialized { get; }

        public MD5iCNG()
        {
            MDService = new MD5Cng();
            MDService.Initialize();

            Initialized = true;
        }

        public string GetHash(string plainText)
        {
            byte[] data = Utils.GetBytes(plainText);

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
            byte[] data = Utils.GetBytes(plainText);

            return VerifyHash(data, hashText);
        }
        public bool VerifyHash(byte[] data, string hashText)
        {
            var plainTextHash = GetHash(data);

            return Utils.SecureEquals(plainTextHash, hashText,
                true, null);
        }
    }

#endif

}
