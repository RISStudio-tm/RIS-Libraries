// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Runtime.CompilerServices;

namespace RIS.Extensions
{
    public static class ULongExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEven(this ulong number)
        {
            return (number & 1) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOdd(this ulong number)
        {
            return (number & 1) == 1;
        }

        public static bool IsPrime(this ulong number)
        {
            if (number <= 1)
                return false;
            if (number == 2 || number == 3)
                return true;
            if (number % 2 == 0 || number % 5 == 0)
                return false;

            var bound = (ulong)Math.Floor(
                Math.Sqrt(number));

            for (var i = 3UL; i <= bound; i += 2)
            {
                if (number % i == 0)
                    return false;
            }

            return true;
        }
    }
}
