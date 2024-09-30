// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Globalization;
using System.Text;

namespace RIS.Cryptography.Hash.Methods
{
    public sealed class SHA384 : IRawHashMethod
    {
        private System.Security.Cryptography.SHA384 SHAService { get; }

        public bool Initialized { get; }

        public SHA384()
        {
            SHAService = System.Security.Cryptography.SHA384.Create();
            SHAService.Initialize();

            Initialized = true;
        }

        public string GetHash(string plainText)
        {
            byte[] data = SecureUtils.GetBytes(plainText);

            return GetHash(data);
        }
        public string GetHash(byte[] data)
        {
            byte[] hashBytes = GetHashBytes(data);
            StringBuilder hashText = new StringBuilder(hashBytes.Length * 2);

            for (int i = 0; i < hashBytes.Length; ++i)
            {
                hashText.Append(hashBytes[i].ToString(
                    "x2", CultureInfo.InvariantCulture));
            }

            return hashText.ToString();
        }

        public byte[] GetHashBytes(string plainText)
        {
            byte[] data = SecureUtils.GetBytes(plainText);

            return GetHashBytes(data);
        }
        public byte[] GetHashBytes(byte[] data)
        {
            return SHAService.ComputeHash(data);
        }

        public bool VerifyHash(string plainText, string hashText)
        {
            byte[] data = SecureUtils.GetBytes(plainText);

            return VerifyHash(data, hashText);
        }
        public bool VerifyHash(byte[] data, string hashText)
        {
            var plainTextHash = GetHash(data);

            return SecureUtils.SecureEqualsUnsafe(
                plainTextHash, hashText,
                true, null);
        }

        public bool VerifyHashBytes(string plainText, byte[] hashData)
        {
            byte[] data = SecureUtils.GetBytes(plainText);

            return VerifyHashBytes(data, hashData);
        }
        public bool VerifyHashBytes(byte[] data, byte[] hashData)
        {
            var plainTextHashBytes = GetHashBytes(data);

            return SecureUtils.SecureEqualsUnsafe(
                plainTextHashBytes, hashData);
        }
    }
}
