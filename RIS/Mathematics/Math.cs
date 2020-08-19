// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Runtime.CompilerServices;

namespace RIS.Mathematics
{
    public static class Math
    {
        public static event EventHandler<RInformationEventArgs> Information;
		public static event EventHandler<RWarningEventArgs> Warning;
		public static event EventHandler<RErrorEventArgs> Error;

        public static void OnInformation(RInformationEventArgs e)
        {
            OnInformation(null, e);
        }
        public static void OnInformation(object sender, RInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        public static void OnWarning(RWarningEventArgs e)
        {
            OnWarning(null, e);
        }
        public static void OnWarning(object sender, RWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        public static void OnError(RErrorEventArgs e)
        {
            OnError(null, e);
        }
        public static void OnError(object sender, RErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostEquals(double number1, double number2, double precision)
        {
            return System.Math.Abs(number1 - number2) <= precision;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostEquals(float number1, float number2, float precision)
        {
            return System.Math.Abs(number1 - number2) <= precision;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte Abs(sbyte number)
        {
            return number == sbyte.MinValue ? sbyte.MaxValue : (sbyte)((number + (number >> 7)) ^ (number >> 7));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Abs(short number)
        {
            return number == short.MinValue ? short.MaxValue : (short)((number + (number >> 15)) ^ (number >> 15));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(int number)
        {
            return number == int.MinValue ? int.MaxValue : (number + (number >> 31)) ^ (number >> 31);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Abs(long number)
        {
            return number == long.MinValue ? long.MaxValue : (number + (number >> 63)) ^ (number >> 63);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Abs(float number)
        {
            return number == float.MinValue ? float.MaxValue : System.Math.Abs(number);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Abs(double number)
        {
            return number == double.MinValue ? double.MaxValue : System.Math.Abs(number);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte AbsOverflow(sbyte number)
        {
            return checked((sbyte)((number + (number >> 7)) ^ (number >> 7)));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short AbsOverflow(short number)
        {
            return checked((short)((number + (number >> 15)) ^ (number >> 15)));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AbsOverflow(int number)
        {
            return checked((number + (number >> 31)) ^ (number >> 31));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long AbsOverflow(long number)
        {
            return checked((number + (number >> 63)) ^ (number >> 63));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AbsOverflow(float number)
        {
            return checked(System.Math.Abs(number));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double AbsOverflow(double number)
        {
            return checked(System.Math.Abs(number));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte AbsNoOverflow(sbyte number)
        {
            return (byte)((number + (number >> 7)) ^ (number >> 7));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort AbsNoOverflow(short number)
        {
            return (ushort)((number + (number >> 15)) ^ (number >> 15));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AbsNoOverflow(int number)
        {
            return (uint)((number + (number >> 31)) ^ (number >> 31));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong AbsNoOverflow(long number)
        {
            return (ulong)((number + (number >> 63)) ^ (number >> 63));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(sbyte number)
        {
            return IsPowerOfTwo(Abs(number));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(byte number)
        {
            return IsPowerOfTwo((uint)number);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(short number)
        {
            return IsPowerOfTwo(Abs(number));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(ushort number)
        {
            return IsPowerOfTwo((uint)number);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(int number)
        {
            return IsPowerOfTwo(Abs(number));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(uint number)
        {
            return (number != 0) && ((number & (number - 1)) == 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(long number)
        {
            return IsPowerOfTwo(Abs(number));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(ulong number)
        {
            return (number != 0) && ((number & (number - 1)) == 0);
        }


        /// <summary>
        /// Returns 2^x more or equals <paramref name="number"/> (next power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static sbyte NextPowerOfTwo(sbyte number)
        {
            byte numberUnsigned = AbsNoOverflow(number);

            if (numberUnsigned > 64)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return (sbyte)NextPowerOfTwo(numberUnsigned);
        }
        /// <summary>
        /// Returns 2^x more or equals <paramref name="number"/> (next power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static byte NextPowerOfTwo(byte number)
        {
            if (number > 128)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (number == 0)
                return 1;

            --number;

            number = (byte)(number | number >> 1);
            number = (byte)(number | number >> 2);
            number = (byte)(number | number >> 4);

            ++number;

            return number;
        }
        /// <summary>
        /// Returns 2^x more or equals <paramref name="number"/> (next power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static short NextPowerOfTwo(short number)
        {
            ushort numberUnsigned = AbsNoOverflow(number);

            if (numberUnsigned > 16384)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return (short)NextPowerOfTwo(numberUnsigned);
        }
        /// <summary>
        /// Returns 2^x more or equals <paramref name="number"/> (next power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static ushort NextPowerOfTwo(ushort number)
        {
            if (number > 32768)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (number == 0)
                return 1;

            --number;

            number = (ushort)(number | number >> 1);
            number = (ushort)(number | number >> 2);
            number = (ushort)(number | number >> 4);
            number = (ushort)(number | number >> 8);

            ++number;

            return number;
        }
        /// <summary>
        /// Returns 2^x more or equals <paramref name="number"/> (next power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static int NextPowerOfTwo(int number)
        {
            uint numberUnsigned = AbsNoOverflow(number);

            if (numberUnsigned > 1073741824)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return (int)NextPowerOfTwo(numberUnsigned);
        }
        /// <summary>
        /// Returns 2^x more or equals <paramref name="number"/> (next power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static uint NextPowerOfTwo(uint number)
        {
            if (number > 2147483648)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (number == 0)
                return 1;

            --number;

            number |= number >> 1;
            number |= number >> 2;
            number |= number >> 4;
            number |= number >> 8;
            number |= number >> 16;

            ++number;

            return number;
        }
        /// <summary>
        /// Returns 2^x more or equals <paramref name="number"/> (next power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static long NextPowerOfTwo(long number)
        {
            ulong numberUnsigned = AbsNoOverflow(number);

            if (numberUnsigned > 4611686018427387904)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return (long)NextPowerOfTwo(numberUnsigned);
        }
        /// <summary>
        /// Returns 2^x more or equals <paramref name="number"/> (next power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static ulong NextPowerOfTwo(ulong number)
        {
            if (number > 9223372036854775808)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (number == 0)
                return 1;

            --number;

            number |= number >> 1;
            number |= number >> 2;
            number |= number >> 4;
            number |= number >> 8;
            number |= number >> 16;
            number |= number >> 32;

            ++number;

            return number;
        }


        /// <summary>
        /// Returns 2^x less or equals <paramref name="number"/> (previous power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static sbyte PrevPowerOfTwo(sbyte number)
        {
            byte numberUnsigned = AbsNoOverflow(number);

            if (numberUnsigned == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (numberUnsigned > 64)
                return 64;

            if (!IsPowerOfTwo(numberUnsigned))
            {
                numberUnsigned = (byte)(NextPowerOfTwo(numberUnsigned) >> 1);
            }

            return (sbyte)numberUnsigned;
        }
        /// <summary>
        /// Returns 2^x less or equals <paramref name="number"/> (previous power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static byte PrevPowerOfTwo(byte number)
        {
            if (number == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (number > 128)
                return 128;

            if (!IsPowerOfTwo(number))
            {
                number = (byte)(NextPowerOfTwo(number) >> 1);
            }

            return number;
        }
        /// <summary>
        /// Returns 2^x less or equals <paramref name="number"/> (previous power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static short PrevPowerOfTwo(short number)
        {
            ushort numberUnsigned = AbsNoOverflow(number);

            if (numberUnsigned == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (numberUnsigned > 16384)
                return 16384;

            if (!IsPowerOfTwo(numberUnsigned))
            {
                numberUnsigned = (ushort)(NextPowerOfTwo(numberUnsigned) >> 1);
            }

            return (short)numberUnsigned;
        }
        /// <summary>
        /// Returns 2^x less or equals <paramref name="number"/> (previous power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static ushort PrevPowerOfTwo(ushort number)
        {
            if (number == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (number > 32768)
                return 32768;

            if (!IsPowerOfTwo(number))
            {
                number = (ushort)(NextPowerOfTwo(number) >> 1);
            }

            return number;
        }
        /// <summary>
        /// Returns 2^x less or equals <paramref name="number"/> (previous power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static int PrevPowerOfTwo(int number)
        {
            uint numberUnsigned = AbsNoOverflow(number);

            if (numberUnsigned == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (numberUnsigned > 1073741824)
                return 1073741824;

            if (!IsPowerOfTwo(numberUnsigned))
            {
                numberUnsigned = NextPowerOfTwo(numberUnsigned) >> 1;
            }

            return (int)numberUnsigned;
        }
        /// <summary>
        /// Returns 2^x less or equals <paramref name="number"/> (previous power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static uint PrevPowerOfTwo(uint number)
        {
            if (number == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (number > 2147483648)
                return 2147483648;

            if (!IsPowerOfTwo(number))
            {
                number = NextPowerOfTwo(number) >> 1;
            }

            return number;
        }
        /// <summary>
        /// Returns 2^x less or equals <paramref name="number"/> (previous power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static long PrevPowerOfTwo(long number)
        {
            ulong numberUnsigned = AbsNoOverflow(number);

            if (numberUnsigned == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (numberUnsigned > 4611686018427387904)
                return 4611686018427387904;

            if (!IsPowerOfTwo(numberUnsigned))
            {
                numberUnsigned = NextPowerOfTwo(numberUnsigned) >> 1;
            }

            return (long)numberUnsigned;
        }
        /// <summary>
        /// Returns 2^x less or equals <paramref name="number"/> (previous power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static ulong PrevPowerOfTwo(ulong number)
        {
            if (number == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (number > 9223372036854775808)
                return 9223372036854775808;

            if (!IsPowerOfTwo(number))
            {
                number = NextPowerOfTwo(number) >> 1;
            }

            return number;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint DigitsCount(sbyte number)
        {
            if (number < 0)
                number = Abs(number);

            return DigitsCount((byte)number);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint DigitsCount(byte number)
        {
            if (number < 10)
                return 1;
            if (number < 100)
                return 2;

            return 3;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint DigitsCount(short number)
        {
            if (number < 0)
                number = Abs(number);

            return DigitsCount((ushort)number);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint DigitsCount(ushort number)
        {
            if (number < 10)
                return 1;
            if (number < 100)
                return 2;
            if (number < 1000)
                return 3;
            if (number < 10000)
                return 4;

            return 5;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint DigitsCount(int number)
        {
            if (number < 0)
                number = Abs(number);

            return DigitsCount((uint)number);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint DigitsCount(uint number)
        {
            if (number < 10)
                return 1;
            if (number < 100)
                return 2;
            if (number < 1000)
                return 3;
            if (number < 10000)
                return 4;
            if (number < 100000)
                return 5;
            if (number < 1000000)
                return 6;
            if (number < 10000000)
                return 7;
            if (number < 100000000)
                return 8;
            if (number < 1000000000)
                return 9;

            return 10;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint DigitsCount(long number)
        {
            if (number < 0)
                number = Abs(number);

            if (number < 10L)
                return 1;
            if (number < 100L)
                return 2;
            if (number < 1000L)
                return 3;
            if (number < 10000L)
                return 4;
            if (number < 100000L)
                return 5;
            if (number < 1000000L)
                return 6;
            if (number < 10000000L)
                return 7;
            if (number < 100000000L)
                return 8;
            if (number < 1000000000L)
                return 9;
            if (number < 10000000000L)
                return 10;
            if (number < 100000000000L)
                return 11;
            if (number < 1000000000000L)
                return 12;
            if (number < 10000000000000L)
                return 13;
            if (number < 100000000000000L)
                return 14;
            if (number < 1000000000000000L)
                return 15;
            if (number < 10000000000000000L)
                return 16;
            if (number < 100000000000000000L)
                return 17;
            if (number < 1000000000000000000L)
                return 18;

            return 19;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint DigitsCount(ulong number)
        {
            if (number < 10L)
                return 1;
            if (number < 100L)
                return 2;
            if (number < 1000L)
                return 3;
            if (number < 10000L)
                return 4;
            if (number < 100000L)
                return 5;
            if (number < 1000000L)
                return 6;
            if (number < 10000000L)
                return 7;
            if (number < 100000000L)
                return 8;
            if (number < 1000000000L)
                return 9;
            if (number < 10000000000L)
                return 10;
            if (number < 100000000000L)
                return 11;
            if (number < 1000000000000L)
                return 12;
            if (number < 10000000000000L)
                return 13;
            if (number < 100000000000000L)
                return 14;
            if (number < 1000000000000000L)
                return 15;
            if (number < 10000000000000000L)
                return 16;
            if (number < 100000000000000000L)
                return 17;
            if (number < 1000000000000000000L)
                return 18;
            if (number < 10000000000000000000L)
                return 19;

            return 20;
        }
    }
}
