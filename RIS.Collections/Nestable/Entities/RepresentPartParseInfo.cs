// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using RIS.Collections.Trees;

namespace RIS.Collections.Nestable.Entities
{
    internal readonly struct RepresentPartParseInfo
    {
        private const char ElementStartValue = '\"';
        private const char ArrayStartValue = '[';
        private const char CollectionStartValue = '{';



        private static readonly RepresentPartParseInfo ElementInfo;
        private static readonly RepresentPartParseInfo ArrayInfo;
        private static readonly RepresentPartParseInfo CollectionInfo;



        public readonly NestedType Type;
        public readonly char StartValue;
        public readonly Trie EndValuesTrie;
        public readonly KeyValuePair<char, char> ParenthesesMap;



        static RepresentPartParseInfo()
        {
            ElementInfo = new RepresentPartParseInfo(
                NestedType.Element,
                '\"',
                new ReadOnlySpan<char[]>(new[]
                {
                    new[] { '\"', ',' },
                    new[] { '\"', '}' }
                }),
                new KeyValuePair<char, char>('\"', '\"'));
            ArrayInfo = new RepresentPartParseInfo(
                NestedType.Array,
                '[',
                new ReadOnlySpan<char[]>(new[]
                {
                    new[] { ']', ',' },
                    new[] { ']', '}' }
                }),
                new KeyValuePair<char, char>('[', ']'));
            CollectionInfo = new RepresentPartParseInfo(
                NestedType.Collection,
                '{',
                new ReadOnlySpan<char[]>(new []
                {
                    new[] { '}', ',' },
                    new[] { '}', '}' }
                }),
                new KeyValuePair<char, char>('{', '}'));
        }



        private RepresentPartParseInfo(NestedType type,
            char startValue, ReadOnlySpan<char[]> endValues,
            KeyValuePair<char, char> parenthesesMap)
        {
            Type = type;
            StartValue = startValue;
            EndValuesTrie = new Trie(
                endValues);
            ParenthesesMap = parenthesesMap;
        }



        public static RepresentPartParseInfo Get(
            NestedType type)
        {
            switch (type)
            {
                case NestedType.Element:
                    return ElementInfo;
                case NestedType.Array:
                    return ArrayInfo;
                case NestedType.Collection:
                    return CollectionInfo;
                case NestedType.Unknown:
                default:
                    var exception = new ArgumentException(
                        $"Invalid value for the {nameof(type)} parameter",
                        nameof(type));
                    Events.OnError(
                        new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }
        }
        public static RepresentPartParseInfo Get(
            char startChar)
        {
            switch (startChar)
            {
                case ElementStartValue:
                    return ElementInfo;
                case ArrayStartValue:
                    return ArrayInfo;
                case CollectionStartValue:
                    return CollectionInfo;
                default:
                    var exception = new ArgumentException(
                        $"{nameof(startChar)}[{startChar}] is an unknown character of the beginning of the representation",
                        nameof(startChar));
                    Events.OnError(
                        new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }
        }
        public static RepresentPartParseInfo Get(
            string represent, int startIndex)
        {
            if (string.IsNullOrEmpty(represent))
            {
                var exception = new ArgumentException(
                    $"{nameof(represent)} cannot be null or empty",
                    nameof(represent));
                Events.OnError(
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return Get(represent.AsSpan(),
                startIndex);
        }
        public static RepresentPartParseInfo Get(
            ReadOnlySpan<char> represent, int startIndex)
        {
            if (represent == null)
            {
                var exception = new ArgumentException(
                    $"{nameof(represent)} cannot be null",
                    nameof(represent));
                Events.OnError(
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (startIndex < 0)
            {
                var exception = new ArgumentOutOfRangeException(
                    nameof(startIndex),
                    $"{nameof(startIndex)} cannot be less than zero");
                Events.OnError(
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            switch (represent[startIndex])
            {
                case ElementStartValue:
                    return ElementInfo;
                case ArrayStartValue:
                    return ArrayInfo;
                case CollectionStartValue:
                    return CollectionInfo;
                default:
                    var exception = new ArgumentException(
                        $"{nameof(startIndex)}[{startIndex}] indicates an unknown character of the beginning of the representation",
                        nameof(startIndex));
                    Events.OnError(
                        new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }
        }
    }
}
