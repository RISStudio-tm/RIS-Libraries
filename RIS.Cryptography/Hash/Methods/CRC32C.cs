// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Globalization;
#if NETCOREAPP
using System.Runtime.Intrinsics.X86;
#endif

namespace RIS.Cryptography.Hash.Methods
{
    public sealed class CRC32C : IHashMethod
    {
        private Algorithms.CRC32C CRCService { get; }

        public bool Initialized { get; }

        public CRC32C()
        {

#if NETCOREAPP
            if (!Sse42.X64.IsSupported && !Sse42.IsSupported)
            {
                CRCService = new Algorithms.CRC32C();
                CRCService.Initialize();
            }

#elif NETFRAMEWORK

                CRCService = new Algorithms.CRC32C();
                CRCService.Initialize();

#endif

            Initialized = true;
        }

        public string GetHash(string plainText)
        {
            byte[] data = Utils.GetBytes(plainText);

            return GetHash(data);
        }
        public string GetHash(byte[] data)
        {

#if NETCOREAPP

            ulong hashValue = 0xFFFFFFFF;

            if (Sse42.X64.IsSupported)
            {
                //hashValue ^= 0xFFFFFFFF;

                Span<byte> dataSpan = new Span<byte>(data);
                int remainingCount = dataSpan.Length % 8;

                for (int i = 0; i < dataSpan.Length - remainingCount; i += 8)
                    hashValue = Sse42.X64.Crc32(hashValue, BitConverter.ToUInt64(dataSpan.Slice(i, 8)));

                if (remainingCount % 2 == 0)
                {
                    for (int i = 0; i < remainingCount; i += 2)
                    {
                        hashValue = Sse42.Crc32((uint)hashValue,
                            BitConverter.ToUInt16(dataSpan.Slice(dataSpan.Length - remainingCount + i, 2)));
                    }
                }
                else
                {
                    for (int i = 0; i < remainingCount; ++i)
                    {
                        hashValue = Sse42.Crc32((uint)hashValue,
                            dataSpan.Slice(dataSpan.Length - remainingCount + i, 1)[0]);
                    }
                }

                //if (remainingCount == 1)
                //{
                //    hashValue = Sse42.Crc32((uint)hashValue, dataSpan.Slice(dataSpan.Length - 1, 1)[0]);
                //}
                //else if (remainingCount == 2)
                //{
                //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt16(dataSpan.Slice(dataSpan.Length - 2, 2)));
                //}
                //else if (remainingCount == 3)
                //{
                //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt16(dataSpan.Slice(dataSpan.Length - 3, 2)));
                //    hashValue = Sse42.Crc32((uint)hashValue, dataSpan.Slice(dataSpan.Length - 1, 1)[0]);
                //}
                //else if (remainingCount == 4)
                //{
                //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt32(dataSpan.Slice(dataSpan.Length - 4, 4)));
                //}
                //else if (remainingCount == 5)
                //{
                //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt32(dataSpan.Slice(dataSpan.Length - 5, 4)));
                //    hashValue = Sse42.Crc32((uint)hashValue, dataSpan.Slice(dataSpan.Length - 1, 1)[0]);
                //}
                //else if (remainingCount == 6)
                //{
                //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt32(dataSpan.Slice(dataSpan.Length - 6, 4)));
                //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt16(dataSpan.Slice(dataSpan.Length - 2, 2)));
                //}
                //else if (remainingCount == 7)
                //{
                //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt32(dataSpan.Slice(data.Length - 7, 4)));
                //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt16(dataSpan.Slice(data.Length - 3, 2)));
                //    hashValue = Sse42.Crc32((uint)hashValue, dataSpan.Slice(dataSpan.Length - 1, 1)[0]);
                //}

                hashValue ^= 0xFFFFFFFF;
            }
            else if (Sse42.IsSupported)
            {
                //hashValue ^= 0xFFFFFFFF;

                Span<byte> dataSpan = new Span<byte>(data);
                int remainingCount = dataSpan.Length % 4;

                for (int i = 0; i < dataSpan.Length - (data.Length % 4); i += 4)
                    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt32(dataSpan.Slice(i, 4)));

                if (remainingCount % 2 == 0)
                {
                    for (int i = 0; i < remainingCount; i += 2)
                    {
                        hashValue = Sse42.Crc32((uint)hashValue,
                            BitConverter.ToUInt16(dataSpan.Slice(dataSpan.Length - remainingCount + i, 2)));
                    }
                }
                else
                {
                    for (int i = 0; i < remainingCount; ++i)
                    {
                        hashValue = Sse42.Crc32((uint)hashValue,
                            dataSpan.Slice(dataSpan.Length - remainingCount + i, 1)[0]);
                    }
                }

                //if (remainingCount == 1)
                //{
                //    hashValue = Sse42.Crc32((uint)hashValue, dataSpan.Slice(dataSpan.Length - 1, 1)[0]);
                //}
                //else if (remainingCount == 2)
                //{
                //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt16(dataSpan.Slice(dataSpan.Length - 2, 2)));
                //}
                //else if (remainingCount == 3)
                //{
                //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt16(dataSpan.Slice(dataSpan.Length - 3, 2)));
                //    hashValue = Sse42.Crc32((uint)hashValue, dataSpan.Slice(dataSpan.Length - 1, 1)[0]);
                //}

                hashValue ^= 0xFFFFFFFF;
            }
            else
            {
                hashValue = BitConverter.ToUInt32(
                    CRCService.ComputeHash(data), 0);
            }

#elif NETFRAMEWORK

                uint hashValue = BitConverter.ToUInt32(
                    CRCService.ComputeHash(data), 0);

#endif

            return hashValue.ToString(
                "x2", CultureInfo.InvariantCulture);
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
}
