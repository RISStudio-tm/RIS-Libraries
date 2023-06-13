// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Globalization;
using System.Text;
#if NETCOREAPP
using System.Runtime.Intrinsics.X86;
#endif

namespace RIS.Cryptography.Hash.Methods
{
    public sealed class CRC32C : IRawHashMethod
    {
        private static readonly Func<CRC32C, bool> InitializeFunction;
        private static readonly Func<byte[], Algorithms.CRC32C, byte[]> GetHashFunction;

        private Algorithms.CRC32C CRCService { get; set; }

        public bool Initialized { get; }

        static CRC32C()
        {
#if NETCOREAPP

            if (Sse42.X64.IsSupported && Sse42.IsSupported)
            {
                InitializeFunction = _ =>
                {
                    return true;
                };
                GetHashFunction = (data, _) =>
                {
                    ulong hashValue = 0xFFFFFFFF;

                    var dataSpan = new Span<byte>(data);
                    var remainingCount = dataSpan.Length % 8;

                    for (int i = 0; i < dataSpan.Length - remainingCount; i += 8)
                    {
                        hashValue = Sse42.X64.Crc32(hashValue,
                            BitConverter.ToUInt64(dataSpan.Slice(i, 8)));
                    }

                    for (int i = dataSpan.Length - remainingCount; i < dataSpan.Length; ++i)
                    {
                        hashValue = Sse42.Crc32((uint)hashValue,
                            dataSpan[i]);
                    }

                    hashValue ^= 0xFFFFFFFF;

                    return BytesUtils.ToBytesBE(
                        (uint)hashValue);
                };
            }
            else if (Sse42.IsSupported)
            {
                InitializeFunction = _ =>
                {
                    return true;
                };
                GetHashFunction = (data, _) =>
                {
                    uint hashValue = 0xFFFFFFFF;

                    var dataSpan = new Span<byte>(data);
                    var remainingCount = dataSpan.Length % 4;

                    for (int i = 0; i < dataSpan.Length - remainingCount; i += 4)
                    {
                        hashValue = Sse42.Crc32(hashValue,
                            BitConverter.ToUInt32(dataSpan.Slice(i, 4)));
                    }

                    for (int i = dataSpan.Length - remainingCount; i < dataSpan.Length; ++i)
                    {
                        hashValue = Sse42.Crc32(hashValue,
                            dataSpan[i]);
                    }

                    hashValue ^= 0xFFFFFFFF;

                    return BytesUtils.ToBytesBE(
                        hashValue);
                };
            }
            else
            {
                InitializeFunction = instance =>
                {
                    instance.CRCService = new Algorithms.CRC32C();
                    instance.CRCService.Initialize();

                    return true;
                };
                GetHashFunction = (data, service) =>
                {
                    return service.ComputeHash(data);
                };
            }

#elif NETFRAMEWORK

            InitializeFunction = instance =>
            {
                instance.CRCService = new Algorithms.CRC32C();
                instance.CRCService.Initialize();

                return true;
            };
            GetHashFunction = (data, service) =>
            {
                return service.ComputeHash(data);
            };

#endif
        }

        public CRC32C()
        {
            if (!InitializeFunction(this))
            {
                var exception = new Exception($"HashMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

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
            return GetHashFunction(data, CRCService);
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
