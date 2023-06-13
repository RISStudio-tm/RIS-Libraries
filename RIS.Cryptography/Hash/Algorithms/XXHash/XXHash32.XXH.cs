// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Runtime.CompilerServices;

namespace RIS.Cryptography.Hash.Algorithms
{
    // ReSharper disable InconsistentNaming
    public static partial class XXHash32
    {
        private static readonly uint XXH_PRIME32_1 = 2654435761U;
        private static readonly uint XXH_PRIME32_2 = 2246822519U;
        private static readonly uint XXH_PRIME32_3 = 3266489917U;
        private static readonly uint XXH_PRIME32_4 = 668265263U;
        private static readonly uint XXH_PRIME32_5 = 374761393U;
        


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint XXH_rotl32(
            uint x, int r)
        {
            return (x << r) | (x >> (32 - r));
        }
    }
    // ReSharper restore InconsistentNaming
}
