﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Runtime.CompilerServices;

namespace RIS.Cryptography.Hash.Algorithms
{
    // Auto-Generated inline
    // ReSharper disable InconsistentNaming
    public partial class XXHash32
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint __inline__XXH32(
            byte* input, int len, uint seed)
        {
            uint h32;

            if (len >= 16)
            {
                var end = input + len;
                var limit = end - 15;

                var v1 = seed + XXH_PRIME32_1 + XXH_PRIME32_2;
                var v2 = seed + XXH_PRIME32_2;
                var v3 = seed + 0;
                var v4 = seed - XXH_PRIME32_1;

                do
                {
                    var reg1 = *((uint*)(input + 0));
                    var reg2 = *((uint*)(input + 4));
                    var reg3 = *((uint*)(input + 8));
                    var reg4 = *((uint*)(input + 12));

                    // XXH32_round
                    v1 += reg1 * XXH_PRIME32_2;
                    v1 = (v1 << 13) | (v1 >> (32 - 13));
                    v1 *= XXH_PRIME32_1;

                    // XXH32_round
                    v2 += reg2 * XXH_PRIME32_2;
                    v2 = (v2 << 13) | (v2 >> (32 - 13));
                    v2 *= XXH_PRIME32_1;

                    // XXH32_round
                    v3 += reg3 * XXH_PRIME32_2;
                    v3 = (v3 << 13) | (v3 >> (32 - 13));
                    v3 *= XXH_PRIME32_1;

                    // XXH32_round
                    v4 += reg4 * XXH_PRIME32_2;
                    v4 = (v4 << 13) | (v4 >> (32 - 13));
                    v4 *= XXH_PRIME32_1;

                    input += 16;
                } while (input < limit);

                h32 = ((v1 << 1) | (v1 >> (32 - 1))) +
                      ((v2 << 7) | (v2 >> (32 - 7))) +
                      ((v3 << 12) | (v3 >> (32 - 12))) +
                      ((v4 << 18) | (v4 >> (32 - 18)));
            }
            else
            {
                h32 = seed + XXH_PRIME32_5;
            }

            h32 += (uint) len;

            // XXH32_finalize
            len &= 15;
            while (len >= 4)
            {
                h32 += *((uint*) input) * XXH_PRIME32_3;
                input += 4;
                h32 = ((h32 << 17) | (h32 >> (32 - 17))) * XXH_PRIME32_4;
                len -= 4;
            }

            while (len > 0)
            {
                h32 += *((byte*) input) * XXH_PRIME32_5;
                ++input;
                h32 = ((h32 << 11) | (h32 >> (32 - 11))) * XXH_PRIME32_1;
                --len;
            }

            // XXH32_avalanche
            h32 ^= h32 >> 15;
            h32 *= XXH_PRIME32_2;
            h32 ^= h32 >> 13;
            h32 *= XXH_PRIME32_3;
            h32 ^= h32 >> 16;

            return h32;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void __inline__XXH32_stream_process(
            byte[] input, int len, ref uint v1, ref uint v2, ref uint v3, ref uint v4)
        {
            fixed (byte* pData = &input[0])
            {
                var ptr = pData;
                var limit = ptr + len;

                do
                {
                    var reg1 = *((uint*)(ptr + 0));
                    var reg2 = *((uint*)(ptr + 4));
                    var reg3 = *((uint*)(ptr + 8));
                    var reg4 = *((uint*)(ptr + 12));

                    // XXH32_round
                    v1 += reg1 * XXH_PRIME32_2;
                    v1 = (v1 << 13) | (v1 >> (32 - 13));
                    v1 *= XXH_PRIME32_1;

                    // XXH32_round
                    v2 += reg2 * XXH_PRIME32_2;
                    v2 = (v2 << 13) | (v2 >> (32 - 13));
                    v2 *= XXH_PRIME32_1;

                    // XXH32_round
                    v3 += reg3 * XXH_PRIME32_2;
                    v3 = (v3 << 13) | (v3 >> (32 - 13));
                    v3 *= XXH_PRIME32_1;

                    // XXH32_round
                    v4 += reg4 * XXH_PRIME32_2;
                    v4 = (v4 << 13) | (v4 >> (32 - 13));
                    v4 *= XXH_PRIME32_1;

                    ptr += 16;

                } while (ptr < limit);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint __inline__XXH32_stream_finalize(
            byte[] input, int len, ref uint v1, ref uint v2, ref uint v3, ref uint v4, long length, uint seed)
        {
            fixed (byte* pData = &input[0])
            {
                var ptr = pData;
                uint h32;

                if (length >= 16)
                {
                    h32 = ((v1 << 1) | (v1 >> (32 - 1))) +  
                          ((v2 << 7) | (v2 >> (32 - 7))) +
                          ((v3 << 12) | (v3 >> (32 - 12))) +
                          ((v4 << 18) | (v4 >> (32 - 18)));
                }
                else
                {
                    h32 = seed + XXH_PRIME32_5;
                }

                h32 += (uint)length;

                // XXH32_finalize
                len &= 15;
                while (len >= 4)
                {
                    h32 += *((uint*)ptr) * XXH_PRIME32_3;
                    ptr += 4;
                    h32 = ((h32 << 17) | (h32 >> (32 - 17))) * XXH_PRIME32_4;
                    len -= 4;
                }

                while (len > 0)
                {
                    h32 += *((byte*)ptr) * XXH_PRIME32_5;
                    ptr++;
                    h32 = ((h32 << 11) | (h32 >> (32 - 11))) * XXH_PRIME32_1;
                    len--;
                }

                // XXH32_avalanche
                h32 ^= h32 >> 15;
                h32 *= XXH_PRIME32_2;
                h32 ^= h32 >> 13;
                h32 *= XXH_PRIME32_3;
                h32 ^= h32 >> 16;

                return h32;
            }
        }
    }
    // ReSharper restore InconsistentNaming
}
