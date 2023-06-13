// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using RIS.Text.Encoding.Base;

namespace RIS.Cryptography.Hash.Methods
{
    public sealed class Argon2dRaw : IRawHashMethod
    {
        public static readonly Argon2Type Type;

        private byte[] _salt;
        public byte[] SaltBytes
        {
            get
            {
                return _salt;
            }
            set
            {
                if (value.Length < 8)
                    value = new byte[8];

                _salt = value;
            }
        }
        public string Salt
        {
            get
            {
                return Convert.ToBase64String(_salt);
            }
            set
            {
                byte[] salt;

                if (Base64.IsBase64(value))
                {
                    try
                    {
                        salt = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        salt = SecureUtils.GetBytes(value);
                    }
                }
                else
                {
                    salt = SecureUtils.GetBytes(value);
                }

                if (salt.Length < 8)
                    salt = new byte[8];

                _salt = salt;
            }
        }
        private ushort _hashLength;
        public ushort HashLength
        {
            get
            {
                return _hashLength;
            }
            set
            {
                if (value < 8)
                    value = 8;

                _hashLength = value;
            }
        }
        private int _degreeOfParallelism;
        public int DegreeOfParallelism
        {
            get
            {
                return _degreeOfParallelism;
            }
            set
            {
                if (value < 1)
                    value = 1;

                _degreeOfParallelism = value;
            }
        }
        private int _iterations;
        public int Iterations
        {
            get
            {
                return _iterations;
            }
            set
            {
                if (value < 1)
                    value = 1;

                _iterations = value;
            }
        }
        private int _memorySize;
        public int MemorySize
        {
            get
            {
                return _memorySize;
            }
            set
            {
                if (value < 8)
                    value = 8;

                _memorySize = value;
            }
        }
        private byte[] _associatedData;
        public byte[] AssociatedDataBytes
        {
            get
            {
                return _associatedData;
            }
            set
            {
                _associatedData = value;
            }
        }
        public string AssociatedData
        {
            get
            {
                return Convert.ToBase64String(_associatedData);
            }
            set
            {
                if (Base64.IsBase64(value))
                {
                    try
                    {
                        _associatedData = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _associatedData = SecureUtils.GetBytes(value);
                    }
                }
                else
                {
                    _associatedData = SecureUtils.GetBytes(value);
                }
            }
        }
        private byte[] _knownSecret;
        public byte[] KnownSecretBytes
        {
            get
            {
                return _knownSecret;
            }
            set
            {
                _knownSecret = value;
            }
        }
        public string KnownSecret
        {
            get
            {
                return Convert.ToBase64String(_knownSecret);
            }
            set
            {
                if (Base64.IsBase64(value))
                {
                    try
                    {
                        _knownSecret = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _knownSecret = SecureUtils.GetBytes(value);
                    }
                }
                else
                {
                    _knownSecret = SecureUtils.GetBytes(value);
                }
            }
        }
        public bool FixedHashLength { get; set; }

        public bool Initialized { get; }

        static Argon2dRaw()
        {
            Type = Argon2Type.Argon2d;
        }

        public Argon2dRaw()
        {
            SaltBytes = new byte[8];
            HashLength = 32;
            DegreeOfParallelism = 2 * 2;
            Iterations = 4;
            MemorySize = (1 * 1024) * 128;
            AssociatedDataBytes = Array.Empty<byte>();
            KnownSecretBytes = Array.Empty<byte>();

            FixedHashLength = true;

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
            Konscious.Security.Cryptography.Argon2d argon2Service = new Konscious.Security.Cryptography.Argon2d(data)
            {
                Salt = SaltBytes,
                DegreeOfParallelism = DegreeOfParallelism,
                Iterations = Iterations,
                MemorySize = MemorySize,
                AssociatedData = AssociatedDataBytes,
                KnownSecret = KnownSecretBytes
            };

            return argon2Service.GetBytes(FixedHashLength ? HashLength : SaltBytes.Length);
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
