// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Globalization;
using System.Text;

namespace RIS.Extensions
{
    public static class StringBuilderExtensions
    {
        public static int IndexOf(this StringBuilder builder,
            string value, int startIndex = 0, bool ignoreCase = false,
            CultureInfo culture = null)
        {
            var length = value.Length;
            var maxSearchLength = (builder.Length - length) + 1;

            if (culture == null)
                culture = CultureInfo.InvariantCulture;

            if (ignoreCase)
                value = value.ToLower(culture);

            var compareFunction = ignoreCase
                ? new Func<char, char, bool>((ch1, ch2) => char.ToLower(ch1, culture) == ch2)
                : new Func<char, char, bool>((ch1, ch2) => ch1 == ch2);

            for (int i = startIndex; i < maxSearchLength; ++i)
            {
                if (!compareFunction(builder[i], value[0]))
                    continue;

                var index = 1;

                while ((index < length)
                       && compareFunction(builder[i + index], value[index]))
                {
                    ++index;
                }

                if (index == length)
                    return i;
            }

            return -1;
        }
    }
}
