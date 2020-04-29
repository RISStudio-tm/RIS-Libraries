using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace RIS.Cryptography
{
    public static class Utils
    {
        public static Encoding SecureUTF8 { get; }

        static Utils()
        {
            SecureUTF8 = new UTF8Encoding(false, true);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals(string left, string right, bool ignoreCase = false, bool invariantCulture = true)
        {
            if (ignoreCase)
            {
                if (invariantCulture)
                {
                    left = left.ToLowerInvariant();
                    right = right.ToLowerInvariant();
                }
                else
                {
                    left = left.ToLower();
                    right = right.ToLower();
                }
            }

            return SecureEquals(SecureUTF8.GetBytes(left), SecureUTF8.GetBytes(right));
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals(byte[] left, byte[] right)
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;

            uint diff = (uint)(left.Length ^ right.Length);
            for (int i = 0; i < left.Length && i < right.Length; i++)
            {
                diff |= (uint)(left[i] ^ right[i]);
            }

            return diff == 0;
        }
    }
}
