// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Linq;

namespace RIS.Extensions.Entities
{
    public class RootIndex
    {
        public readonly string SingleChars;
        public readonly char[] Chars;
        public readonly IDictionary<char, LeafIndex> MultipleChars;

        private RootIndex(string singleChars, char[] chars,
            IDictionary<char, LeafIndex> multipleChars)
        {
            SingleChars = singleChars;
            Chars = chars;
            MultipleChars = multipleChars;
        }

        public static RootIndex FromStrings(IEnumerable<string> strings)
        {
            return FromStrings(strings as string[] ?? strings.ToArray());
        }
        public static RootIndex FromStrings(params string[] strings)
        {
            var multipleChars = strings
                .Where(s => !string.IsNullOrEmpty(s))
                .ToLookup(s => s[0], s => s.Substring(1))
                .ToDictionary(g => g.Key, g => LeafIndex.FromStrings(g));
            var chars = multipleChars.Keys
                .ToArray();
            var singleChars = new string(strings
                .Where(s => s != null && s.Length == 1)
                .Select(s => s[0])
                .ToArray());

            return new RootIndex(singleChars,
                chars, multipleChars);
        }
    }
}
