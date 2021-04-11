// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Globalization;
using System.Text;
using RIS.Cryptography.Hash.Metadata;
using RIS.Text.Encoding.Base;

namespace RIS.Cryptography.Hash.Methods
{
    public sealed class Argon2idWP : IHashMethod
    {
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
                        _associatedData =
                            Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _associatedData =
                            Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(value)));
                    }
                }
                else
                {
                    _associatedData =
                        Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(value)));
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
                        _knownSecret =
                            Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _knownSecret =
                            Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(value)));
                    }
                }
                else
                {
                    _knownSecret =
                        Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(value)));
                }
            }
        }
        public bool FixedHashLength { get; set; }

        public bool Initialized { get; }

        public Argon2idWP()
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

        public static Argon2Metadata GetMetadata(string hashText)
        {
            return new Argon2Metadata(hashText);
        }

        public string GetHash(string plainText)
        {
            return GetHash(plainText, MemorySize, Iterations,
                DegreeOfParallelism, AssociatedData, KnownSecret);
        }
        public string GetHash(string plainText, int memorySize, int iterations,
            int degreeOfParallelism)
        {
            return GetHash(plainText, memorySize, iterations,
                degreeOfParallelism, AssociatedData, KnownSecret);
        }
        public string GetHash(string plainText, int memorySize, int iterations,
            int degreeOfParallelism, byte[] associatedData)
        {
            return GetHash(plainText, memorySize, iterations,
                degreeOfParallelism, Convert.ToBase64String(associatedData), KnownSecret);
        }
        public string GetHash(string plainText, int memorySize, int iterations,
            int degreeOfParallelism, string associatedData)
        {
            return GetHash(plainText, memorySize, iterations,
                degreeOfParallelism, associatedData, KnownSecret);
        }
        public string GetHash(string plainText, int memorySize, int iterations,
            int degreeOfParallelism, byte[] associatedData, byte[] knownSecret)
        {
            return GetHash(plainText, memorySize, iterations,
                degreeOfParallelism, Convert.ToBase64String(associatedData), Convert.ToBase64String(knownSecret));
        }
        public string GetHash(string plainText, int memorySize, int iterations,
            int degreeOfParallelism, string associatedData, string knownSecret)
        {
            byte[] data = SecureUtils.GetBytes(plainText);

            return GetHash(data, memorySize, iterations,
                degreeOfParallelism, associatedData, knownSecret);
        }
        public string GetHash(string plainText, string salt)
        {
            return GetHash(plainText, salt, MemorySize, Iterations,
                DegreeOfParallelism, AssociatedData, KnownSecret);
        }
        public string GetHash(string plainText, string salt, int memorySize, int iterations,
            int degreeOfParallelism)
        {
            return GetHash(plainText, salt, memorySize, iterations,
                degreeOfParallelism, AssociatedData, KnownSecret);
        }
        public string GetHash(string plainText, string salt, int memorySize, int iterations,
            int degreeOfParallelism, byte[] associatedData)
        {
            return GetHash(plainText, salt, memorySize, iterations,
                degreeOfParallelism, Convert.ToBase64String(associatedData), KnownSecret);
        }
        public string GetHash(string plainText, string salt, int memorySize, int iterations,
            int degreeOfParallelism, string associatedData)
        {
            return GetHash(plainText, salt, memorySize, iterations,
                degreeOfParallelism, associatedData, KnownSecret);
        }
        public string GetHash(string plainText, string salt, int memorySize, int iterations,
            int degreeOfParallelism, byte[] associatedData, byte[] knownSecret)
        {
            return GetHash(plainText, salt, memorySize, iterations,
                degreeOfParallelism, Convert.ToBase64String(associatedData), Convert.ToBase64String(knownSecret));
        }
        public string GetHash(string plainText, string salt, int memorySize, int iterations,
            int degreeOfParallelism, string associatedData, string knownSecret)
        {
            byte[] data = SecureUtils.GetBytes(plainText);

            return GetHash(data, salt, memorySize, iterations,
                degreeOfParallelism, associatedData, knownSecret);
        }

        public string GetHash(byte[] data)
        {
            return GetHash(data, MemorySize, Iterations,
                DegreeOfParallelism, AssociatedData, KnownSecret);
        }
        public string GetHash(byte[] data, int memorySize, int iterations,
            int degreeOfParallelism)
        {
            return GetHash(data, memorySize, iterations,
                degreeOfParallelism, AssociatedData, KnownSecret);
        }
        public string GetHash(byte[] data, int memorySize, int iterations,
            int degreeOfParallelism, byte[] associatedData)
        {
            return GetHash(data, memorySize, iterations,
                degreeOfParallelism, Convert.ToBase64String(associatedData), KnownSecret);
        }
        public string GetHash(byte[] data, int memorySize, int iterations,
            int degreeOfParallelism, string associatedData)
        {
            return GetHash(data, memorySize, iterations,
                degreeOfParallelism, associatedData, KnownSecret);
        }
        public string GetHash(byte[] data, int memorySize, int iterations,
            int degreeOfParallelism, byte[] associatedData, byte[] knownSecret)
        {
            return GetHash(data, memorySize, iterations,
                degreeOfParallelism, Convert.ToBase64String(associatedData), Convert.ToBase64String(knownSecret));
        }
        public string GetHash(byte[] data, int memorySize, int iterations,
            int degreeOfParallelism, string associatedData, string knownSecret)
        {
            byte[] hashSalt = HashMethodsUtils.GenerateSaltBytes(SaltLength);

            if (memorySize < 8)
                memorySize = 8;

            if (iterations < 1)
                iterations = 1;

            if (degreeOfParallelism < 1)
                degreeOfParallelism = 1;

            byte[] associatedDataBytes;

            if (Base64.IsBase64(associatedData))
            {
                try
                {
                    associatedDataBytes =
                        Convert.FromBase64String(associatedData);
                }
                catch (FormatException)
                {
                    associatedDataBytes =
                        Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(associatedData)));
                }
            }
            else
            {
                associatedDataBytes =
                    Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(associatedData)));
            }

            byte[] knownSecretBytes;

            if (Base64.IsBase64(knownSecret))
            {
                try
                {
                    knownSecretBytes =
                        Convert.FromBase64String(knownSecret);
                }
                catch (FormatException)
                {
                    knownSecretBytes =
                        Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(knownSecret)));
                }
            }
            else
            {
                knownSecretBytes =
                    Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(knownSecret)));
            }

            Konscious.Security.Cryptography.Argon2id argon2Service = new Konscious.Security.Cryptography.Argon2id(data)
            {
                Salt = hashSalt,
                DegreeOfParallelism = degreeOfParallelism,
                Iterations = iterations,
                MemorySize = memorySize,
                AssociatedData = associatedDataBytes,
                KnownSecret = knownSecretBytes
            };

            byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);

            return $"${Enum.GetName(typeof(Argon2Type), Argon2Type.Argon2id)?.ToLower()}$v=19$m={memorySize},t={iterations},p={degreeOfParallelism}${Base64.RemovePadding(Convert.ToBase64String(hashSalt))}${Base64.RemovePadding(Convert.ToBase64String(hashBytes))}";
        }
        public string GetHash(byte[] data, string salt)
        {
            return GetHash(data, salt, MemorySize, Iterations,
                DegreeOfParallelism, AssociatedData, KnownSecret);
        }
        public string GetHash(byte[] data, string salt, int memorySize, int iterations,
            int degreeOfParallelism)
        {
            return GetHash(data, salt, memorySize, iterations,
                degreeOfParallelism, AssociatedData, KnownSecret);
        }
        public string GetHash(byte[] data, string salt, int memorySize, int iterations,
            int degreeOfParallelism, byte[] associatedData)
        {
            return GetHash(data, salt, memorySize, iterations,
                degreeOfParallelism, Convert.ToBase64String(associatedData), KnownSecret);
        }
        public string GetHash(byte[] data, string salt, int memorySize, int iterations,
            int degreeOfParallelism, string associatedData)
        {
            return GetHash(data, salt, memorySize, iterations,
                degreeOfParallelism, associatedData, KnownSecret);
        }
        public string GetHash(byte[] data, string salt, int memorySize, int iterations,
            int degreeOfParallelism, byte[] associatedData, byte[] knownSecret)
        {
            return GetHash(data, salt, memorySize, iterations,
                degreeOfParallelism, Convert.ToBase64String(associatedData), Convert.ToBase64String(knownSecret));
        }
        public string GetHash(byte[] data, string salt, int memorySize, int iterations,
            int degreeOfParallelism, string associatedData, string knownSecret)
        {
            byte[] hashSalt;

            if (Base64.IsBase64(salt))
            {
                try
                {
                    hashSalt =
                        Convert.FromBase64String(salt);
                }
                catch (FormatException)
                {
                    hashSalt =
                        Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(salt)));
                }
            }
            else
            {
                hashSalt =
                    Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(salt)));
            }

            if (memorySize < 8)
                memorySize = 8;

            if (iterations < 1)
                iterations = 1;

            if (degreeOfParallelism < 1)
                degreeOfParallelism = 1;

            byte[] associatedDataBytes;

            if (Base64.IsBase64(associatedData))
            {
                try
                {
                    associatedDataBytes =
                        Convert.FromBase64String(associatedData);
                }
                catch (FormatException)
                {
                    associatedDataBytes =
                        Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(associatedData)));
                }
            }
            else
            {
                associatedDataBytes =
                    Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(associatedData)));
            }

            byte[] knownSecretBytes;

            if (Base64.IsBase64(knownSecret))
            {
                try
                {
                    knownSecretBytes =
                        Convert.FromBase64String(knownSecret);
                }
                catch (FormatException)
                {
                    knownSecretBytes =
                        Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(knownSecret)));
                }
            }
            else
            {
                knownSecretBytes =
                    Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(knownSecret)));
            }

            Konscious.Security.Cryptography.Argon2id argon2Service = new Konscious.Security.Cryptography.Argon2id(data)
            {
                Salt = hashSalt,
                DegreeOfParallelism = degreeOfParallelism,
                Iterations = iterations,
                MemorySize = memorySize,
                AssociatedData = associatedDataBytes,
                KnownSecret = knownSecretBytes
            };

            byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);

            return $"${Enum.GetName(typeof(Argon2Type), Argon2Type.Argon2id)?.ToLower()}$v=19$m={memorySize},t={iterations},p={degreeOfParallelism}${Base64.RemovePadding(Convert.ToBase64String(hashSalt))}${Base64.RemovePadding(Convert.ToBase64String(hashBytes))}";
        }

        public bool VerifyHash(string plainText, string hashText)
        {
            return VerifyHash(plainText, hashText, AssociatedData,
                KnownSecret);
        }
        public bool VerifyHash(string plainText, string hashText, byte[] associatedData)
        {
            return VerifyHash(plainText, hashText, Convert.ToBase64String(associatedData),
                KnownSecret);
        }
        public bool VerifyHash(string plainText, string hashText, string associatedData)
        {
            return VerifyHash(plainText, hashText, associatedData,
                KnownSecret);
        }
        public bool VerifyHash(string plainText, string hashText, byte[] associatedData, byte[] knownSecret)
        {
            return VerifyHash(plainText, hashText, Convert.ToBase64String(associatedData),
                Convert.ToBase64String(knownSecret));
        }
        public bool VerifyHash(string plainText, string hashText, string associatedData, string knownSecret)
        {
            Argon2Metadata metadata = GetMetadata(hashText);

            var plainTextHash = GetHash(plainText, metadata.Salt, metadata.MemorySize, metadata.Iterations,
                metadata.DegreeOfParallelism, associatedData, knownSecret);

            return SecureUtils.SecureEquals(plainTextHash, hashText, false, null);
        }

        public bool VerifyHash(byte[] data, string hashText)
        {
            return VerifyHash(data, hashText, AssociatedData,
                KnownSecret);
        }
        public bool VerifyHash(byte[] data, string hashText, byte[] associatedData)
        {
            return VerifyHash(data, hashText, Convert.ToBase64String(associatedData),
                KnownSecret);
        }
        public bool VerifyHash(byte[] data, string hashText, string associatedData)
        {
            return VerifyHash(data, hashText, associatedData,
                KnownSecret);
        }
        public bool VerifyHash(byte[] data, string hashText, byte[] associatedData, byte[] knownSecret)
        {
            return VerifyHash(data, hashText, Convert.ToBase64String(associatedData),
                Convert.ToBase64String(knownSecret));
        }
        public bool VerifyHash(byte[] data, string hashText, string associatedData, string knownSecret)
        {
            Argon2Metadata metadata = GetMetadata(hashText);

            var plainTextHash = GetHash(data, metadata.Salt, metadata.MemorySize, metadata.Iterations,
                metadata.DegreeOfParallelism, associatedData, knownSecret);

            return SecureUtils.SecureEquals(plainTextHash, hashText, false, null);
        }

        public bool VerifyAndUpdateHash(string plainText, string hashText, out bool isUpdated, out string newHashText)
        {
            return VerifyAndUpdateHash(plainText, hashText, MemorySize, Iterations,
                DegreeOfParallelism, AssociatedData, KnownSecret, out isUpdated, out newHashText);
        }
        public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
            int newDegreeOfParallelism, out bool isUpdated, out string newHashText)
        {
            return VerifyAndUpdateHash(plainText, hashText, newMemorySize, newIterations,
                newDegreeOfParallelism, AssociatedData, KnownSecret, out isUpdated, out newHashText);
        }
        public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
            int newDegreeOfParallelism, byte[] associatedData, out bool isUpdated, out string newHashText)
        {
            return VerifyAndUpdateHash(plainText, hashText, newMemorySize, newIterations,
                newDegreeOfParallelism, Convert.ToBase64String(associatedData), KnownSecret,
                out isUpdated, out newHashText);
        }
        public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
            int newDegreeOfParallelism, string associatedData, out bool isUpdated, out string newHashText)
        {
            return VerifyAndUpdateHash(plainText, hashText, newMemorySize, newIterations,
                newDegreeOfParallelism, associatedData, KnownSecret, out isUpdated, out newHashText);
        }
        public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
            int newDegreeOfParallelism, byte[] associatedData, byte[] knownSecret, out bool isUpdated, out string newHashText)
        {
            return VerifyAndUpdateHash(plainText, hashText, newMemorySize, newIterations,
                newDegreeOfParallelism, Convert.ToBase64String(associatedData), Convert.ToBase64String(knownSecret),
                out isUpdated, out newHashText);
        }
        public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
            int newDegreeOfParallelism, string associatedData, string knownSecret, out bool isUpdated, out string newHashText)
        {
            bool result;
            isUpdated = false;
            newHashText = hashText;

            result = VerifyHash(plainText, hashText, associatedData, knownSecret);

            if (!result)
                return false;

            Argon2Metadata metadata = GetMetadata(hashText);

            isUpdated = metadata.MemorySize != newMemorySize || metadata.Iterations != newIterations || metadata.DegreeOfParallelism != newDegreeOfParallelism;

            if (isUpdated)
                newHashText = GetHash(plainText, newMemorySize, newIterations, newDegreeOfParallelism, associatedData, knownSecret);
            else
                newHashText = hashText;

            return true;
        }

        public bool VerifyAndUpdateHash(byte[] data, string hashText, out bool isUpdated, out string newHashText)
        {
            return VerifyAndUpdateHash(data, hashText, MemorySize, Iterations,
                DegreeOfParallelism, AssociatedData, KnownSecret, out isUpdated, out newHashText);
        }
        public bool VerifyAndUpdateHash(byte[] data, string hashText, int newMemorySize, int newIterations,
            int newDegreeOfParallelism, out bool isUpdated, out string newHashText)
        {
            return VerifyAndUpdateHash(data, hashText, newMemorySize, newIterations,
                newDegreeOfParallelism, AssociatedData, KnownSecret, out isUpdated, out newHashText);
        }
        public bool VerifyAndUpdateHash(byte[] data, string hashText, int newMemorySize, int newIterations,
            int newDegreeOfParallelism, byte[] associatedData, out bool isUpdated, out string newHashText)
        {
            return VerifyAndUpdateHash(data, hashText, newMemorySize, newIterations,
                newDegreeOfParallelism, Convert.ToBase64String(associatedData), KnownSecret,
                out isUpdated, out newHashText);
        }
        public bool VerifyAndUpdateHash(byte[] data, string hashText, int newMemorySize, int newIterations,
            int newDegreeOfParallelism, string associatedData, out bool isUpdated, out string newHashText)
        {
            return VerifyAndUpdateHash(data, hashText, newMemorySize, newIterations,
                newDegreeOfParallelism, associatedData, KnownSecret, out isUpdated, out newHashText);
        }
        public bool VerifyAndUpdateHash(byte[] data, string hashText, int newMemorySize, int newIterations,
            int newDegreeOfParallelism, byte[] associatedData, byte[] knownSecret, out bool isUpdated, out string newHashText)
        {
            return VerifyAndUpdateHash(data, hashText, newMemorySize, newIterations,
                newDegreeOfParallelism, Convert.ToBase64String(associatedData), Convert.ToBase64String(knownSecret),
                out isUpdated, out newHashText);
        }
        public bool VerifyAndUpdateHash(byte[] data, string hashText, int newMemorySize, int newIterations,
            int newDegreeOfParallelism, string associatedData, string knownSecret, out bool isUpdated, out string newHashText)
        {
            bool result;
            isUpdated = false;
            newHashText = hashText;

            result = VerifyHash(data, hashText, associatedData, knownSecret);

            if (!result)
                return false;

            Argon2Metadata metadata = GetMetadata(hashText);

            isUpdated = metadata.MemorySize != newMemorySize || metadata.Iterations != newIterations || metadata.DegreeOfParallelism != newDegreeOfParallelism;

            if (isUpdated)
                newHashText = GetHash(data, newMemorySize, newIterations, newDegreeOfParallelism, associatedData, knownSecret);
            else
                newHashText = hashText;

            return true;
        }
    }
}
