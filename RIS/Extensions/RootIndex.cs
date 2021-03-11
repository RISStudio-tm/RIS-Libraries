// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Linq;

namespace RIS.Extensions
{
    public class RootIndex
    {
        public char[] Chars;
        public string SingleChars;
        public IDictionary<char, LeafIndex> MultipleChars;

        public static RootIndex FromStrings(IEnumerable<string> strings)
        {
            return FromStrings(strings as string[] ?? strings.ToArray());
        }
        public static RootIndex FromStrings(params string[] strings)
        {
            var idx = strings
                .Where(s => !string.IsNullOrEmpty(s))
                .ToLookup(s => s[0], s => s.Substring(1))
                .ToDictionary(g => g.Key, g => LeafIndex.FromStrings(g));

            return new RootIndex
            {
                MultipleChars = idx,
                Chars = idx.Keys
                    .ToArray(),
                SingleChars = new string(strings
                    .Where(s => s != null && s.Length == 1)
                    .Select(s => s[0])
                    .ToArray())
            };
        }
    }
}
