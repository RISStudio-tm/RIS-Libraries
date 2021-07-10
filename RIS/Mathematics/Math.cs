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



        public const double Pi =                 3.14159265358979323846264338328;
        public const double InversePi =          0.31830988618379067153776752675;
        public const double DoubleInversePi =    0.63661977236758134307553505349;
        public const double DoublePi =           6.28318530717958647692528676656;
        public const double HalfPi =             1.57079632679489661923132169164;
        public const double SquarePi =           9.86960440108935861883449099988;
        public const double SqrtPi =             1.77245385090551602729816748334;
        public const double E =                  2.71828182845904523536028747135;
        public const double InverseE =           0.36787944117144232159552377016;
        public const double DoubleE =            5.43656365691809047072057494270;
        public const double HalfE =              1.35914091422952261768014373568;
        public const double SquareE =            1.64872127070012814684865078781;
        public const double Log2 =               0.30102999566398119521373889472;
        public const double Ln2 =                0.69314718055994530941723212146;
        public const double Sqrt2 =              1.41421356237309504880168872421;
        public const double Sqrt3 =              1.73205080756887729352744634151;
        public const double EulerConstant =      0.57721566490153286060651209008;
        public const double CatalanConstant =    0.91596559417721901505460351493;
        public const double OmegaConstant =      0.56714329040978387299996866221;
        public const double GaussConstant =      0.83462684167407318628142973280;
        public const double GoldenRatio =        1.61803398874989484820458633437;



        public const double Epsilon =            2.2204460492503131E-16;
        public const double SqrtEpsilon =        1.4901161193847656E-08;
        public const double CbrtEpsilon =        6.0554544523933395E-06;
        public const double LogEpsilon =        -36.043653389117154;
        public const double MinDouble =          2.2250738585072014E-308;
        public const double SqrtMinDouble =      1.4916681462400413E-154;
        public const double LogMinDouble =      -708.39641853226408;
        public const double MaxDouble =          1.7976931348623157E+308;
        public const double SqrtMaxDouble =      1.3407807929942596E+154;
        public const double LogMaxDouble =       709.78271289338397;



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



        public static double ToPercentage(double totalValue,
            double value, int digits = 2)
        {
            if (digits < 0)
                digits = 0;

            double percent = 0;

            if (value < totalValue)
            {
                percent = System.Math.Round(
                    100 - (value / totalValue * 100),
                    digits,
                    MidpointRounding.AwayFromZero);
            }

            return percent;
        }


        public static double ToFahrenheit(double celsius)
        {
            return (celsius * 9 / 5) + 32;
        }

        public static double ToCelsius(double fahrenheit)
        {
            return (fahrenheit - 32) * 5 / 9;
        }


        /// <summary>
        /// Returns the value of the angle in degrees converted from radians.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToDegrees(double radians)
        {
            return 180.0 / Pi * radians;
        }

        /// <summary>
        /// Returns the value of the angle in radians converted from degrees.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToRadians(double degrees)
        {
            return degrees / 180.0 * Pi;
        }


        /// <summary>
        ///     Returns the minus one raised to an integer power.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MinusOnePow(int exponent)
        {
            return (exponent & 1) == 0
                ? 1
                : -1;
        }


        /// <summary>
        ///     Returns the integer part of a float number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double IntegerPart(float number)
        {
            return IntegerPart((double)number);
        }
        /// <summary>
        ///     Returns the integer part of a double number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double IntegerPart(double number)
        {
            return System.Math.Floor(number);
        }


        /// <summary>
        ///     Returns the fractional part of a float number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double FractionalPart(float number)
        {
            return FractionalPart((double)number);
        }
        /// <summary>
        ///     Returns the fractional part of a double number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double FractionalPart(double number)
        {
            return number - System.Math.Floor(number);
        }


        /// <summary>
        ///     Calculates the integral part of a specified float number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Truncate(float number)
        {
            return Truncate((double)number);
        }
        /// <summary>
        ///     Calculates the integral part of a specified double number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Truncate(double number)
        {
            return number > 0
                ? System.Math.Floor(number)
                : System.Math.Ceiling(number);
        }


        /// <summary>
        ///     Returns whether or not two floats are "close". That is, whether or 
        ///     not they are within epsilon of each other. Note that this epsilon is proportional
        ///     to the numbers themselves to that AlmostEquals survives scalar multiplication.
        ///     There are plenty of ways for this to return false even for numbers which
        ///     are theoretically identical, so no code calling this should fail to work
        ///     if this returns false.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostEquals(float number1, float number2)
        {
            return AlmostEquals((double)number1, (double)number2);
        }
        /// <summary>
        ///     Returns whether or not two doubles are "close". That is, whether or 
        ///     not they are within epsilon of each other. Note that this epsilon is proportional
        ///     to the numbers themselves to that AlmostEquals survives scalar multiplication.
        ///     There are plenty of ways for this to return false even for numbers which
        ///     are theoretically identical, so no code calling this should fail to work
        ///     if this returns false.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostEquals(double number1, double number2)
        {
            if (double.IsInfinity(number1)
                && double.IsInfinity(number2))
            {
                return true;
            }
            if (double.IsInfinity(number1)
                || double.IsInfinity(number2))
            {
                return false;
            }

            // This computes (|value1-value2| / (|value1| + |value2| + 10.0)) < Epsilon

            double eps = (System.Math.Abs(number1) + System.Math.Abs(number2) + 10.0) * Epsilon;
            double delta = number1 - number2;

            return (-eps < delta) && (eps > delta);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostEquals(float number1, float number2, float precision)
        {
            if (float.IsInfinity(number1)
                && float.IsInfinity(number2))
            {
                return true;
            }
            if (float.IsInfinity(number1)
                || float.IsInfinity(number2))
            {
                return false;
            }

            if (number1 == 0 || number2 == 0)
                return System.Math.Abs(number1 - number2) <= precision;

            return System.Math.Abs(number1 - number2) <= precision * System.Math.Max(
                System.Math.Abs(number1), System.Math.Abs(number2));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostEquals(double number1, double number2, double precision)
        {
            if (double.IsInfinity(number1)
                && double.IsInfinity(number2))
            {
                return true;
            }
            if (double.IsInfinity(number1)
                || double.IsInfinity(number2))
            {
                return false;
            }

            if (number1 == 0 || number2 == 0)
                return System.Math.Abs(number1 - number2) <= precision;

            return System.Math.Abs(number1 - number2) <= precision * System.Math.Max(
                System.Math.Abs(number1), System.Math.Abs(number2));
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
            return System.Math.Abs(number);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Abs(double number)
        {
            return System.Math.Abs(number);
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
            return System.Math.Abs(number);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double AbsOverflow(double number)
        {
            return System.Math.Abs(number);
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
        public static float AbsNoOverflow(float number)
        {
            return System.Math.Abs(number);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double AbsNoOverflow(double number)
        {
            return System.Math.Abs(number);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(sbyte number)
        {
            return IsPowerOfTwo(AbsNoOverflow(number));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(byte number)
        {
            return IsPowerOfTwo((uint)number);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(short number)
        {
            return IsPowerOfTwo(AbsNoOverflow(number));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(ushort number)
        {
            return IsPowerOfTwo((uint)number);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(int number)
        {
            return IsPowerOfTwo(AbsNoOverflow(number));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(uint number)
        {
            return (number != 0) && ((number & (number - 1)) == 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(long number)
        {
            return IsPowerOfTwo(AbsNoOverflow(number));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(ulong number)
        {
            return (number != 0) && ((number & (number - 1)) == 0);
        }


        /// <summary>
        ///     Returns 2^x more or equals <paramref name="number"/> (next power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static sbyte NextPowerOfTwo(sbyte number)
        {
            byte numberUnsigned = AbsNoOverflow(number);

            if (numberUnsigned > 64)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return (sbyte)NextPowerOfTwo(numberUnsigned);
        }
        /// <summary>
        ///     Returns 2^x more or equals <paramref name="number"/> (next power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static byte NextPowerOfTwo(byte number)
        {
            if (number > 128)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
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
        ///     Returns 2^x more or equals <paramref name="number"/> (next power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static short NextPowerOfTwo(short number)
        {
            ushort numberUnsigned = AbsNoOverflow(number);

            if (numberUnsigned > 16384)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return (short)NextPowerOfTwo(numberUnsigned);
        }
        /// <summary>
        ///     Returns 2^x more or equals <paramref name="number"/> (next power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static ushort NextPowerOfTwo(ushort number)
        {
            if (number > 32768)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
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
        ///     Returns 2^x more or equals <paramref name="number"/> (next power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static int NextPowerOfTwo(int number)
        {
            uint numberUnsigned = AbsNoOverflow(number);

            if (numberUnsigned > 1073741824)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return (int)NextPowerOfTwo(numberUnsigned);
        }
        /// <summary>
        ///     Returns 2^x more or equals <paramref name="number"/> (next power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static uint NextPowerOfTwo(uint number)
        {
            if (number > 2147483648)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
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
        ///     Returns 2^x more or equals <paramref name="number"/> (next power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static long NextPowerOfTwo(long number)
        {
            ulong numberUnsigned = AbsNoOverflow(number);

            if (numberUnsigned > 4611686018427387904)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return (long)NextPowerOfTwo(numberUnsigned);
        }
        /// <summary>
        ///     Returns 2^x more or equals <paramref name="number"/> (next power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static ulong NextPowerOfTwo(ulong number)
        {
            if (number > 9223372036854775808)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
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
        ///     Returns 2^x less or equals <paramref name="number"/> (previous power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static sbyte PrevPowerOfTwo(sbyte number)
        {
            byte numberUnsigned = AbsNoOverflow(number);

            if (numberUnsigned == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
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
        ///     Returns 2^x less or equals <paramref name="number"/> (previous power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static byte PrevPowerOfTwo(byte number)
        {
            if (number == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
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
        ///     Returns 2^x less or equals <paramref name="number"/> (previous power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static short PrevPowerOfTwo(short number)
        {
            ushort numberUnsigned = AbsNoOverflow(number);

            if (numberUnsigned == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
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
        ///     Returns 2^x less or equals <paramref name="number"/> (previous power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static ushort PrevPowerOfTwo(ushort number)
        {
            if (number == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
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
        ///     Returns 2^x less or equals <paramref name="number"/> (previous power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static int PrevPowerOfTwo(int number)
        {
            uint numberUnsigned = AbsNoOverflow(number);

            if (numberUnsigned == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
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
        ///     Returns 2^x less or equals <paramref name="number"/> (previous power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static uint PrevPowerOfTwo(uint number)
        {
            if (number == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
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
        ///     Returns 2^x less or equals <paramref name="number"/> (previous power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static long PrevPowerOfTwo(long number)
        {
            ulong numberUnsigned = AbsNoOverflow(number);

            if (numberUnsigned == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
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
        ///     Returns 2^x less or equals <paramref name="number"/> (previous power of two) (if possible)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static ulong PrevPowerOfTwo(ulong number)
        {
            if (number == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
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
            return DigitsCount(number < 0
                ? AbsNoOverflow(number)
                : (byte)number);
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
            return DigitsCount(number < 0
                ? AbsNoOverflow(number)
                : (ushort)number);
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
            return DigitsCount(number < 0
                ? AbsNoOverflow(number)
                : (uint)number);
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
            return DigitsCount(number < 0
                ? AbsNoOverflow(number)
                : (ulong)number);
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
