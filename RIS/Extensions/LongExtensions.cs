﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Runtime.CompilerServices;

namespace RIS.Extensions
{
    public static class LongExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEven(this long number)
        {
            return (number & 1) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOdd(this long number)
        {
            return (number & 1) == 1;
        }

        public static bool IsPrime(this long number)
        {
            if (number <= 1)
                return false;
            if (number == 2 || number == 3)
                return true;
            if (number % 2 == 0 || number % 5 == 0)
                return false;

            var bound = (long)Math.Floor(
                Math.Sqrt(number));

            for (var i = 3L; i <= bound; i += 2)
            {
                if (number % i == 0)
                    return false;
            }

            return true;
        }



#pragma warning disable IDE0060 // Удалите неиспользуемый параметр
        // ReSharper disable UnusedParameter.Global
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize(this long source)
        {
            return sizeof(long);
        }
        // ReSharper restore UnusedParameter.Global
#pragma warning restore IDE0060 // Удалите неиспользуемый параметр
    }
}
