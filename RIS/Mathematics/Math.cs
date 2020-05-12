using System;
using System.Runtime.CompilerServices;

namespace RIS.Mathematics
{
    public static class Math
    {
        public static event EventHandler<RMessageEventArgs> ShowMessage;
        public static event EventHandler<RErrorEventArgs> ShowError;

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
            return (sbyte)((number + (number >> 7)) ^ (number >> 7));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Abs(short number)
        {
            return (short)((number + (number >> 15)) ^ (number >> 15));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(int number)
        {
            return (number + (number >> 31)) ^ (number >> 31);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Abs(long number)
        {
            return (number + (number >> 63)) ^ (number >> 63);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(sbyte number)
        {
            number = Abs(number);
            return IsPowerOfTwo((uint)number);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(byte number)
        {
            return IsPowerOfTwo((uint)number);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(short number)
        {
            number = Abs(number);
            return IsPowerOfTwo((uint)number);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(ushort number)
        {
            return IsPowerOfTwo((uint)number);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(int number)
        {
            number = Abs(number);
            return (number != 0) && ((number & (number - 1)) == 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(uint number)
        {
            return (number != 0) && ((number & (number - 1)) == 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(long number)
        {
            number = Abs(number);
            return (number != 0) && ((number & (number - 1)) == 0);
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
            number = Abs(number);

            if (number > 64)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return (sbyte)NextPowerOfTwo((byte)number);
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
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
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
            number = Abs(number);

            if (number > 16384)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return (short)NextPowerOfTwo((ushort)number);
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
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
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
            number = Abs(number);

            if (number > 1073741824)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return (int)NextPowerOfTwo((uint)number);
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
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
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
            number = Abs(number);

            if (number > 4611686018427387904)
            {
                var exception = new ArgumentException($"Невозможно найти следующую степень двойки для числа {number}", nameof(number));
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return (long)NextPowerOfTwo((ulong)number);
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
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
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
            number = Abs(number);

            if (number == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (number > 64)
                return 64;

            if (!IsPowerOfTwo((byte)number))
            {
                number = (sbyte)(NextPowerOfTwo((byte)number) >> 1);
            }

            return number;
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
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
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
            number = Abs(number);

            if (number == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (number > 16384)
                return 16384;

            if (!IsPowerOfTwo((ushort)number))
            {
                number = (short)(NextPowerOfTwo((ushort)number) >> 1);
            }

            return number;
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
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
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
            number = Abs(number);

            if (number == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (number > 1073741824)
                return 1073741824;

            if (!IsPowerOfTwo((uint)number))
            {
                number = (int)(NextPowerOfTwo((uint)number) >> 1);
            }

            return number;
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
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
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
            number = Abs(number);

            if (number == 0)
            {
                var exception = new ArgumentException($"Невозможно найти предыдущую степень двойки для числа {number}", nameof(number));
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (number > 4611686018427387904)
                return 4611686018427387904;

            if (!IsPowerOfTwo((ulong)number))
            {
                number = (long)NextPowerOfTwo((ulong)number) >> 1;
            }

            return number;
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
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
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
    }
}
