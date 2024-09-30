// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using RIS.Cryptography.Cipher.Methods;
using RIS.Cryptography.Hash.Methods.Entities;
using RIS.Randomizing;
using RIS.Randomizing.Secure;

namespace RIS.Cryptography.Hash.Methods
{
    public sealed class PHM1iArgon2id
    {
        private const int PartHashLength = 16;
        private const int MinInputStringLength = 8;

        private static SHA256 KeyHashProvider { get; }
        private static IUnbiasedRandom RandomGenerator { get; }

        private Argon2idWP HashProvider { get; }
        private Rijndael CipherProvider { get; set; }

        private byte[] _globalKey;
        public byte[] GlobalKeyBytes
        {
            get
            {
                return _globalKey;
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    GlobalKey = null;

                    return;
                }

                _globalKey = value;

                HashProvider.KnownSecretBytes = _globalKey;
                CipherProvider = new Rijndael(
                    GlobalKey,
                    RijndaelKeySize.L256Bit);
            }
        }
        public string GlobalKey
        {
            get
            {
                return Convert.ToBase64String(_globalKey);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = $"global+key+{GetType().Name}";

                _globalKey = KeyHashProvider
                    .GetHashBytes(value);

                HashProvider.KnownSecretBytes = _globalKey;
                CipherProvider = new Rijndael(
                    _globalKey, RijndaelKeySize.L256Bit);
            }
        }
        public int MemorySize
        {
            get
            {
                return HashProvider.MemorySize;
            }
            set
            {
                if (value < 8)
                    value = 8;

                HashProvider.MemorySize = value;
            }
        }
        public int Iterations
        {
            get
            {
                return HashProvider.Iterations;
            }
            set
            {
                if (value < 1)
                    value = 1;

                HashProvider.Iterations = value;
            }
        }
        public int DegreeOfParallelism
        {
            get
            {
                return HashProvider.DegreeOfParallelism;
            }
            set
            {
                if (value < 1)
                    value = 1;

                HashProvider.DegreeOfParallelism = value;
            }
        }



        static PHM1iArgon2id()
        {
            KeyHashProvider = new SHA256();
            RandomGenerator = new SecureRandom();
        }

        public PHM1iArgon2id(
            int memorySize = 1 * 1024 * 64, int iterations = 1,
            int degreeOfParallelism = 2)
        {
            if (GlobalKeyBytes == null)
                GlobalKey = null;

            HashProvider = new Argon2idWP
            {
                HashLength = PartHashLength,
                SaltLength = 16,
                KnownSecretBytes = GlobalKeyBytes,
                MemorySize = memorySize,
                Iterations = iterations,
                DegreeOfParallelism = degreeOfParallelism
            };
            CipherProvider = new Rijndael(
                GlobalKey,
                RijndaelKeySize.L256Bit);
        }
        public PHM1iArgon2id(byte[] globalKey,
            int memorySize = 1 * 1024 * 64, int iterations = 1,
            int degreeOfParallelism = 2)
            : this(memorySize, iterations, degreeOfParallelism)
        {
            GlobalKeyBytes = globalKey;
        }
        public PHM1iArgon2id(string globalKey,
            int memorySize = 1 * 1024 * 64, int iterations = 1,
            int degreeOfParallelism = 2)
            : this(memorySize, iterations, degreeOfParallelism)
        {
            GlobalKey = globalKey;
        }



        public PHM1Part[] GetRandomParts(
            string hashText)
        {
            var decryptedHashText = CipherProvider
                .Decrypt(hashText);
            var partsHashes = decryptedHashText
                .Split('%');

            ushort partsCount = (ushort)Rand.Next(
                (int)(partsHashes.Length * 0.4), (int)(partsHashes.Length * 0.7));

            var uniqueIndexes = new SortedSet<ushort>();

            while (uniqueIndexes.Count < partsCount)
            {
                while (true)
                {
                    var index = RandomGenerator.GetNormalizedIndex(
                        (ushort)partsHashes.Length);

                    if (uniqueIndexes.Contains(index))
                        continue;

                    uniqueIndexes.Add(index);

                    break;
                };
            }

            var indexes = uniqueIndexes
                .ToArray();

            var parts = new PHM1Part[partsCount];

            for (int i = 0; i < parts.Length; ++i)
            {
                parts[i] = new PHM1Part(
                    indexes[i]);
            }

            return parts;
        }



        public string GetHash(string plainText,
            string associatedData = null)
        {
            var associatedDataBytes = !string.IsNullOrEmpty(associatedData)
                ? SecureUtils.GetBytes(associatedData)
                : null;

            return GetHash(plainText,
                associatedDataBytes);
        }
        public string GetHash(string plainText,
            byte[] associatedData = null)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                var exception =
                    new ArgumentException($"{nameof(plainText)} cannot be null or empty", nameof(plainText));
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
            if (plainText.Length < MinInputStringLength)
            {
                var exception =
                    new ArgumentException($"{nameof(plainText)} length must be greater than {MinInputStringLength}", nameof(plainText));
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (associatedData == null)
                associatedData = Array.Empty<byte>();

            var partsHashes = new string[plainText.Length];

            for (var i = 0; i < plainText.Length; ++i)
            {
                var part = plainText[i]
                    .ToString();

                partsHashes[i] = HashProvider.GetHash(
                    part,
                    HashProvider.MemorySize,
                    HashProvider.Iterations,
                    HashProvider.DegreeOfParallelism,
                    associatedData,
                    HashProvider.KnownSecretBytes);
            }

            var hashText = string
                .Join("%", partsHashes);
            var encryptedHashText = CipherProvider
                .Encrypt(hashText);

            return encryptedHashText;
        }

        public bool VerifyHash(PHM1Part[] parts,
            string hashText, string associatedData = null)
        {
            var associatedDataBytes = !string.IsNullOrEmpty(associatedData)
                ? SecureUtils.GetBytes(associatedData)
                : null;

            return VerifyHash(parts, hashText,
                associatedDataBytes);
        }
        public bool VerifyHash(PHM1Part[] parts,
            string hashText, byte[] associatedData = null)
        {
            if (parts == null || parts.Length == 0)
            {
                var exception =
                    new ArgumentException($"{nameof(parts)} cannot be null or empty", nameof(parts));
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
            if (string.IsNullOrEmpty(hashText))
            {
                var exception =
                    new ArgumentException($"{nameof(hashText)} cannot be null or empty", nameof(hashText));
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (associatedData == null)
                associatedData = Array.Empty<byte>();

            var decryptedHashText = CipherProvider
                .Decrypt(hashText);
            var partsHashes = decryptedHashText
                .Split('%');

            if (partsHashes.Length == 0)
                return false;

            var success = true;

            for (var i = 0; i < parts.Length; ++i)
            {
                var part = parts[i];

                if (part.Index > partsHashes.Length - 1)
                {
                    success = false;

                    continue;
                }

                bool result;

                if (part.Content.HasValue)
                {
                    result = HashProvider.VerifyHash(
                        part.Content.Value.ToString(),
                        partsHashes[part.Index],
                        associatedData,
                        HashProvider.KnownSecretBytes);
                }
                else
                {
                    result = false;
                }

                success &= result;
            }

            return success;
        }

        public bool VerifyAndUpdateHash(PHM1Part[] parts,
            string hashText, out bool isUpdated,
            out string newHashText)
        {
            return VerifyAndUpdateHash(parts, hashText,
                (byte[])null, out isUpdated, out newHashText);
        }
        public bool VerifyAndUpdateHash(PHM1Part[] parts,
            string hashText, string associatedData,
            out bool isUpdated, out string newHashText)
        {
            var associatedDataBytes = !string.IsNullOrEmpty(associatedData)
                ? SecureUtils.GetBytes(associatedData)
                : null;

            return VerifyAndUpdateHash(parts, hashText,
                associatedDataBytes, out isUpdated, out newHashText);
        }
        public bool VerifyAndUpdateHash(PHM1Part[] parts,
            string hashText, byte[] associatedData,
            out bool isUpdated, out string newHashText)
        {
            if (parts == null || parts.Length == 0)
            {
                var exception =
                    new ArgumentException($"{nameof(parts)} cannot be null or empty", nameof(parts));
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
            if (string.IsNullOrEmpty(hashText))
            {
                var exception =
                    new ArgumentException($"{nameof(hashText)} cannot be null or empty", nameof(hashText));
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (associatedData == null)
                associatedData = Array.Empty<byte>();

            isUpdated = false;
            newHashText = hashText;

            var decryptedHashText = CipherProvider
                .Decrypt(hashText);
            var partsHashes = decryptedHashText
                .Split('%');

            if (partsHashes.Length == 0)
                return false;

            var success = true;

            for (var i = 0; i < parts.Length; ++i)
            {
                var part = parts[i];

                if (part.Index > partsHashes.Length - 1)
                {
                    success = false;

                    continue;
                }

                bool result;
                bool partIsUpdated;
                string partNewHashText;

                if (part.Content.HasValue)
                {
                    result = HashProvider.VerifyAndUpdateHash(
                        part.Content.ToString(),
                        partsHashes[part.Index],
                        HashProvider.MemorySize,
                        HashProvider.Iterations,
                        HashProvider.DegreeOfParallelism,
                        associatedData,
                        HashProvider.KnownSecretBytes,
                        out partIsUpdated,
                        out partNewHashText);
                }
                else
                {
                    result = false;
                    partIsUpdated = false;
                    partNewHashText = partsHashes[part.Index];
                }

                if (partIsUpdated)
                {
                    partsHashes[i] = partNewHashText;

                    isUpdated = true;
                }

                success &= result;
            }

            if (!isUpdated)
                return success;

            var updatedHashText = string
                .Join("%", partsHashes);
            newHashText = CipherProvider
                .Encrypt(updatedHashText);

            return success;
        }
    }
}
