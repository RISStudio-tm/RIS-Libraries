// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using RIS.Randomizing;
using RIS.Utilities;

namespace RIS.Cryptography.Hash.Methods
{
    public sealed class XXHash64 : IRawHashMethod
    {
        private uint _seed;
        public uint Seed
        {
            get
            {
                return _seed;
            }
            set
            {
                _seed = value;
            }
        }

        public bool Initialized { get; }

        public XXHash64(uint? seed = null)
        {
            if (seed == null)
            {
                byte[] buffer = new byte[4];

                Rand.Current.NextBytes(buffer);

                seed = BitConverter.ToUInt32(
                    buffer, 0);
            }

            _seed = seed.Value;

            Initialized = true;
        }

        public string GetHash(string plainText)
        {
            byte[] hashBytes = GetHashBytes(plainText);
            StringBuilder hashText = new StringBuilder(hashBytes.Length * 2);

            for (int i = 0; i < hashBytes.Length; ++i)
            {
                hashText.Append(hashBytes[i].ToString(
                    "x2", CultureInfo.InvariantCulture));
            }

            return hashText.ToString();
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
            return BytesUtils.ToBytesBE(Algorithms.XXHash64.ComputeHash(plainText, Seed));
        }
        public byte[] GetHashBytes(byte[] data)
        {
            return BytesUtils.ToBytesBE(Algorithms.XXHash64.ComputeHash(data, data.Length, Seed));
        }

        public bool VerifyHash(string plainText, string hashText)
        {
            var plainTextHash = GetHash(plainText);

            return SecureUtils.SecureEqualsUnsafe(
                plainTextHash, hashText,
                true, null);
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
            var plainTextHashBytes = GetHashBytes(plainText);

            return SecureUtils.SecureEqualsUnsafe(
                plainTextHashBytes, hashData);
        }
        public bool VerifyHashBytes(byte[] data, byte[] hashData)
        {
            var plainTextHashBytes = GetHashBytes(data);

            return SecureUtils.SecureEqualsUnsafe(
                plainTextHashBytes, hashData);
        }
    }
}
