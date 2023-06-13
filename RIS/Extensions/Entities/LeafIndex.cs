// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace RIS.Extensions.Entities
{
    public class LeafIndex
    {
        public readonly string SingleChars;
        public readonly string Chars;
        public readonly IDictionary<char, string[]> MultipleChars;

        public LeafIndex(string singleChars, string chars,
            IDictionary<char, string[]> multipleChars)
        {
            SingleChars = singleChars;
            Chars = chars;
            MultipleChars = multipleChars;
        }

        public static LeafIndex FromStrings(IEnumerable<string> strings)
        {
            return FromStrings(strings as string[] ?? strings?.ToArray());
        }
        public static LeafIndex FromStrings(params string[] strings)
        {
            if (strings == null || strings.Length == 0)
                return null;

            var multipleChars = strings
                .Where(s => !string.IsNullOrEmpty(s))
                .ToLookup(s => s[0], s => s.Substring(1))
                .ToDictionary(g => g.Key, g => g
                    .OrderByDescending(s => s.Length)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray());
            var chars = new string(multipleChars.Keys
                .ToArray());
            var singleChars = new string(strings
                .Where(s => s?.Length == 1)
                .Select(s => s[0])
                .ToArray());

            return new LeafIndex(singleChars,
                chars, multipleChars);
        }
    }
}
