// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;

namespace RIS.Collections.Trees
{
    public class Trie : IEnumerable
    {
        private readonly TrieNode _root;



        public Trie()
        {
            _root = new TrieNode();
        }
        public Trie(string[] keys)
            : this()
        {
            Add(keys);
        }
        public Trie(IEnumerable<string> keys)
            : this()
        {
            Add(keys);
        }
        public Trie(ReadOnlySpan<char[]> keys)
            : this()
        {
            Add(keys);
        }



        public void Add(
            string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            Add(key.AsSpan());
        }
        public void Add(
            ReadOnlySpan<char> key)
        {
            if (key == null || key.IsEmpty)
                return;

            var node = _root;

            foreach (var keyPart in key)
            {
                node = node.AddChild(keyPart);
            }

            node.IsEnd = true;
        }
        public void Add(
            string[] keys)
        {
            if (keys == null)
                return;

            foreach (var key in keys)
            {
                Add(key);
            }
        }
        public void Add(
            IEnumerable<string> keys)
        {
            if (keys == null)
                return;

            foreach (var key in keys)
            {
                Add(key);
            }
        }
        public void Add(
            ReadOnlySpan<char[]> keys)
        {
            if (keys == null)
                return;

            foreach (var key in keys)
            {
                Add(key);
            }
        }

        public void Remove(
            string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            Remove(key.AsSpan());
        }
        public void Remove(
            ReadOnlySpan<char> key)
        {
            if (key == null || key.IsEmpty)
                return;

            var node = _root;

            foreach (var keyPart in key)
            {
                node = node[keyPart];

                if (node == null)
                    return;
            }

            node.IsEnd = false;
        }
        public void Remove(
            string[] keys)
        {
            if (keys == null)
                return;

            foreach (var key in keys)
            {
                Remove(key);
            }
        }
        public void Remove(
            IEnumerable<string> keys)
        {
            if (keys == null)
                return;

            foreach (var key in keys)
            {
                Remove(key);
            }
        }

        public bool ContainsKey(
            string source,
            int startIndex = 0, int length = -1)
        {
            if (string.IsNullOrEmpty(source))
                return false;

            if (startIndex > source.Length - 1)
                startIndex = source.Length - 1;
            if (startIndex < 0)
                startIndex = 0;
            if (length > source.Length - startIndex)
                length = source.Length - startIndex;
            if (length < 0)
                length = source.Length - startIndex;

            return ContainsKey(source.AsSpan()
                .Slice(startIndex, length));
        }
        public bool ContainsKey(
            ReadOnlySpan<char> source)
        {
            if (source == null || source.IsEmpty)
                return false;

            var index = 0;
            var node = _root;

            while (index < source.Length)
            {
                node = node[source[index]];

                if (node == null)
                    return false;

                ++index;
            }

            return node.IsEnd;
        }

        public bool ContainsSubkeys(
            string source,
            int startIndex = 0, int length = -1)
        {
            if (string.IsNullOrEmpty(source))
                return false;

            if (startIndex > source.Length - 1)
                startIndex = source.Length - 1;
            if (startIndex < 0)
                startIndex = 0;
            if (length > source.Length - startIndex)
                length = source.Length - startIndex;
            if (length < 0)
                length = source.Length - startIndex;

            return ContainsSubkeys(source.AsSpan()
                .Slice(startIndex, length));
        }
        public bool ContainsSubkeys(
            ReadOnlySpan<char> source)
        {
            if (source == null || source.IsEmpty)
                return false;

            var index = 0;
            var node = _root;

            while (index < source.Length)
            {
                node = node[source[index]];

                if (node == null)
                    return false;
                if (node.IsEnd)
                    return true;

                ++index;
            }

            return node.IsEnd;
        }



        public bool Contains(
            string source,
            int startIndex = 0, int length = -1)
        {
            return IndexOfAny(source,
                    startIndex, length)
                .Index != -1;
        }
        public bool Contains(
            ReadOnlySpan<char> source)
        {
            return IndexOfAny(source)
                .Index != -1;
        }

        public (int Index, int Count) IndexOfAny(
            string source,
            int startIndex = 0, int length = -1)
        {
            if (string.IsNullOrEmpty(source))
                return (-1, 0);

            if (startIndex > source.Length - 1)
                startIndex = source.Length - 1;
            if (startIndex < 0)
                startIndex = 0;
            if (length > source.Length - startIndex)
                length = source.Length - startIndex;
            if (length < 0)
                length = source.Length - startIndex;

            return IndexOfAny(source.AsSpan()
                .Slice(startIndex, length));
        }
        public (int Index, int Count) IndexOfAny(
            ReadOnlySpan<char> source)
        {
            if (source == null || source.IsEmpty)
                return (-1, 0);

            var index = 0;

            while (index < source.Length)
            {
                var node = _root;
                var occurrenceIndex = index;

                while (occurrenceIndex < source.Length)
                {
                    node = node[source[occurrenceIndex]];

                    if (node == null)
                        break;
                    if (node.IsEnd)
                        return (index, occurrenceIndex - index + 1);

                    ++occurrenceIndex;
                }

                ++index;
            }

            return (-1, 0);
        }

        public IEnumerable<(int Index, int Count)> IndexOfAll(
            string source,
            int startIndex = 0, int length = -1)
        {
            if (string.IsNullOrEmpty(source))
                return Array.Empty<(int Index, int Count)>();

            if (startIndex > source.Length - 1)
                startIndex = source.Length - 1;
            if (startIndex < 0)
                startIndex = 0;
            if (length > source.Length - startIndex)
                length = source.Length - startIndex;
            if (length < 0)
                length = source.Length - startIndex;

            return IndexOfAll(source.AsSpan()
                .Slice(startIndex, length));
        }
        public IEnumerable<(int Index, int Count)> IndexOfAll(
            ReadOnlySpan<char> source)
        {
            if (source == null || source.IsEmpty)
                return Array.Empty<(int Index, int Count)>();

            var index = 0;
            var result = new List<(int Index, int Count)>(10);

            while (index < source.Length)
            {
                var node = _root;
                var occurrenceIndex = index;

                while (occurrenceIndex < source.Length)
                {
                    node = node[source[occurrenceIndex]];

                    if (node == null)
                        break;
                    if (node.IsEnd)
                        result.Add((index, occurrenceIndex - index + 1));

                    ++occurrenceIndex;
                }

                ++index;
            }

            return result;
        }



        // Hack for use the collection initializer syntax
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield break;
        }
    };
}
