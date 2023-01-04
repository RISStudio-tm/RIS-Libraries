// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Cryptography.Hash.Algorithms
{
    // ReSharper disable InconsistentNaming
    public static partial class XXHash32
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint UnsafeComputeHash(byte* ptr, int length, uint seed)
        {
            return __inline__XXH32(ptr, length, seed);
        }



        public static unsafe uint ComputeHash(
            byte[] data, int length, uint seed = 0)
        {
            fixed (byte* pData = &data[0])
            {
                return UnsafeComputeHash(pData, length, seed);
            }
        }
        public static unsafe uint ComputeHash(
            byte[] data, int offset, int length, uint seed = 0)
        {
            fixed (byte* pData = &data[0 + offset])
            {
                return UnsafeComputeHash(pData, length, seed);
            }
        }
        public static unsafe uint ComputeHash(
            Span<byte> data, int length, uint seed = 0)
        {
            fixed (byte* pData = &MemoryMarshal.GetReference(data))
            {
                return UnsafeComputeHash(pData, length, seed);
            }
        }
        public static unsafe uint ComputeHash(
            ReadOnlySpan<byte> data, int length, uint seed = 0)
        {
            fixed (byte* pData = &MemoryMarshal.GetReference(data))
            {
                return UnsafeComputeHash(pData, length, seed);
            }
        }
        public static unsafe uint ComputeHash(
            string data, uint seed = 0)
        {
            fixed (char* c = data)
            {
                var ptr = (byte*)c;
                var length = data.Length * 2;

                return UnsafeComputeHash(ptr, length, seed);
            }
        }
        public static ulong ComputeHash(
            ArraySegment<byte> data, uint seed = 0)
        {
            return ComputeHash(data.Array, data.Offset, data.Count, seed);
        }
        public static uint ComputeHash(
            Stream stream, int bufferSize = 4096, uint seed = 0)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize + 16);

            int readBytes;
            var offset = 0;
            long length = 0;

            var v1 = seed + XXH_PRIME32_1 + XXH_PRIME32_2;
            var v2 = seed + XXH_PRIME32_2;
            var v3 = seed + 0;
            var v4 = seed - XXH_PRIME32_1;

            try
            {
                while ((readBytes = stream.Read(buffer, offset, bufferSize)) > 0)
                {
                    length += readBytes;
                    offset += readBytes;

                    if (offset < 16)
                        continue;

                    var r = offset % 16;
                    var l = offset - r;

                    __inline__XXH32_stream_process(buffer, l, ref v1, ref v2, ref v3, ref v4);

                    BytesUtils.BlockCopy(buffer, l, buffer, 0, r);

                    offset = r;
                }

                return __inline__XXH32_stream_finalize(buffer, offset, ref v1, ref v2, ref v3, ref v4, length, seed);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }


        public static ValueTask<uint> ComputeHashAsync(
            Stream stream, int bufferSize = 4096, uint seed = 0)
        {
            return ComputeHashAsync(stream, bufferSize, seed, CancellationToken.None);
        }
        public static async ValueTask<uint> ComputeHashAsync(
            Stream stream, int bufferSize, uint seed, CancellationToken cancellationToken)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize + 16);

            int readBytes;
            var offset = 0;
            long length = 0;

            var v1 = seed + XXH_PRIME32_1 + XXH_PRIME32_2;
            var v2 = seed + XXH_PRIME32_2;
            var v3 = seed + 0;
            var v4 = seed - XXH_PRIME32_1;

            try
            {
                while ((readBytes = await stream.ReadAsync(buffer, offset, bufferSize, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    length += readBytes;
                    offset += readBytes;

                    if (offset < 16)
                        continue;

                    var r = offset % 16;
                    var l = offset - r;

                    __inline__XXH32_stream_process(buffer, l, ref v1, ref v2, ref v3, ref v4);

                    BytesUtils.BlockCopy(buffer, l, buffer, 0, r);

                    offset = r;
                }

                return __inline__XXH32_stream_finalize(buffer, offset, ref v1, ref v2, ref v3, ref v4, length, seed);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
    // ReSharper restore InconsistentNaming
}
