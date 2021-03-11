// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Linq;

namespace RIS.Extensions
{
    public class LeafIndex
    {
        public string Chars;
        public string SingleChars;
        public Dictionary<char, string[]> Strings;

        public static LeafIndex FromStrings(IEnumerable<string> strings)
        {
            return FromStrings(strings as string[] ?? strings.ToArray());
        }
        public static LeafIndex FromStrings(params string[] strings)
        {
            var str = strings
                .Where(s => !string.IsNullOrEmpty(s))
                .ToLookup(s => s[0], s => s.Substring(1))
                .ToDictionary(g => g.Key, g => g
                    .OrderByDescending(s => s.Length)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray());

            return new LeafIndex
            {
                Strings = str,
                Chars = new string(str.Keys
                    .ToArray()),
                SingleChars = new string(strings
                    .Where(s => s?.Length == 1)
                    .Select(s => s[0])
                    .ToArray())
            };
        }
    }
}
