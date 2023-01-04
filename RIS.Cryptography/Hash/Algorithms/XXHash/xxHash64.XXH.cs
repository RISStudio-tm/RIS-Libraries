// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Runtime.CompilerServices;

namespace RIS.Cryptography.Hash.Algorithms
{
    // ReSharper disable InconsistentNaming
    public static partial class XXHash64
    {
        private static readonly ulong XXH_PRIME64_1 = 11400714785074694791UL;
        private static readonly ulong XXH_PRIME64_2 = 14029467366897019727UL;
        private static readonly ulong XXH_PRIME64_3 = 1609587929392839161UL;
        private static readonly ulong XXH_PRIME64_4 = 9650029242287828579UL;
        private static readonly ulong XXH_PRIME64_5 = 2870177450012600261UL;
    


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong XXH_rotl64(
            ulong x, int r)
        {
            return (x << r) | (x >> (64 - r));
        }
    }
    // ReSharper restore InconsistentNaming
}
