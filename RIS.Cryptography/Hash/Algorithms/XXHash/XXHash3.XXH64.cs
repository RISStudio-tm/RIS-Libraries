// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Runtime.CompilerServices;

namespace RIS.Cryptography.Hash.Algorithms
{
    // ReSharper disable InconsistentNaming
    public static partial class XXHash3
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong XXH64_avalanche(
            ulong hash)
        {
            hash ^= hash >> 33;
            hash *= XXH_PRIME64_2;
            hash ^= hash >> 29;
            hash *= XXH_PRIME64_3;
            hash ^= hash >> 32;

            return hash;
        }
    }
    // ReSharper restore InconsistentNaming
}
