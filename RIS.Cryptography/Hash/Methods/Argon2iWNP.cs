// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using RIS.Text.Encoding.Base;

namespace RIS.Cryptography.Hash.Methods
{
    public sealed class Argon2iWNP : IHashMethod
    {
        public static readonly Argon2Type Type;

        private ushort _saltLength;
        public ushort SaltLength
        {
            get
            {
                return _saltLength;
            }
            set
            {
                if (value < 8)
                    value = 8;

                _saltLength = value;
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

        static Argon2iWNP()
        {
            Type = Argon2Type.Argon2i;
        }

        public Argon2iWNP()
        {
            SaltLength = 16;
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
            var data = SecureUtils.GetBytes(plainText);

            return GetHash(data);
        }
        public string GetHash(byte[] data)
        {
            var salt = HashMethodsUtils.GenerateSaltBytes(SaltLength);

            return GetHash(data, Convert.ToBase64String(salt));
        }
        public string GetHash(string plainText, string salt)
        {
            var data = SecureUtils.GetBytes(plainText);

            return GetHash(data, salt);
        }
        public string GetHash(byte[] data, string salt)
        {
            byte[] hashSalt;

            if (Base64.IsBase64(salt))
            {
                try
                {
                    hashSalt = Convert.FromBase64String(salt);
                }
                catch (FormatException)
                {
                    hashSalt = SecureUtils.GetBytes(salt);
                }
            }
            else
            {
                hashSalt = SecureUtils.GetBytes(salt);
            }

            var argon2Service = new Konscious.Security.Cryptography.Argon2i(data)
            {
                Salt = hashSalt,
                DegreeOfParallelism = DegreeOfParallelism,
                Iterations = Iterations,
                MemorySize = MemorySize,
                AssociatedData = AssociatedDataBytes,
                KnownSecret = KnownSecretBytes
            };
            var hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);
            var hashString = Convert.ToBase64String(hashSalt) + "=/" + Convert.ToBase64String(hashBytes);

            return Convert.ToBase64String(SecureUtils.GetBytes(hashString));
        }

        public bool VerifyHash(string plainText, string hashText)
        {
            var data = SecureUtils.GetBytes(plainText);

            return VerifyHash(data, hashText);
        }
        public bool VerifyHash(byte[] data, string hashText)
        {
            var hashTextPlain = SecureUtils.GetString(
                Convert.FromBase64String(hashText));
            var separatorIndex = hashTextPlain
                .IndexOf("=/", StringComparison.Ordinal);
            var hashSalt = hashTextPlain.Substring(
                0, separatorIndex);
            var plainTextHash = GetHash(data, hashSalt);

            return SecureUtils.SecureEqualsUnsafe(
                plainTextHash, hashText,
                false, null);
        }
    }
}
