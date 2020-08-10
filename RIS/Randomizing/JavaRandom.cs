// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Randomizing
{
    internal sealed class JavaRandom : NextBitsRandom
    {
        internal long Seed;

        public JavaRandom(long seed)
            : base(unchecked((int)seed))
        {
            Seed = (seed ^ 0x5DEECE66DL) & ((1L << 48) - 1);
        }

        internal override int NextBits(int countBits)
        {
            unchecked
            {
                Seed = ((Seed * 0x5DEECE66DL) + 0xBL) & ((1L << 48) - 1);

                return (int)((ulong)Seed >> (48 - countBits));
            }
        }
    }
}
