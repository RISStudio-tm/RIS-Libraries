// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RIS.Collections.Nestable
{
    internal class RepresentPartParseInfo
    {
        private const char ElementStartValue = '\"';
        private const char ArrayStartValue = '[';
        private const char CollectionStartValue = '{';



        private static readonly RepresentPartParseInfo ElementInfo;
        private static readonly RepresentPartParseInfo ArrayInfo;
        private static readonly RepresentPartParseInfo CollectionInfo;



        public readonly NestedType Type;
        public readonly char StartValue;
        public readonly string[] EndValues;
        public readonly int ExcludedStart;
        public readonly int ExcludedEnd;
        public readonly ReadOnlyDictionary<char, char> ParenthesesMap;



        static RepresentPartParseInfo()
        {
            ElementInfo = new RepresentPartParseInfo(
                NestedType.Element,
                '\"',
                new[]
                {
                    "\",",
                    "\"}"
                },
                1,
                1,
                new Dictionary<char, char>
                {
                    { '\"', '\"' }
                });
            ArrayInfo = new RepresentPartParseInfo(
                NestedType.Array,
                '[',
                new[]
                {
                    "],",
                    "]}"
                },
                0,
                -1,
                new Dictionary<char, char>
                {
                    { ']', '[' }
                });
            CollectionInfo = new RepresentPartParseInfo(
                NestedType.Collection,
                '{',
                new[]
                {
                    "},",
                    "}}"
                },
                0,
                -1,
                new Dictionary<char, char>
                {
                    { '}', '{' }
                });
        }

        private RepresentPartParseInfo(NestedType type,
            char startValue, string[] endValues,
            int excludedStart, int excludedEnd,
            Dictionary<char, char> parenthesesMap)
        {
            Type = type;
            StartValue = startValue;
            EndValues = endValues;
            ExcludedStart = excludedStart;
            ExcludedEnd = excludedEnd;
            ParenthesesMap = new ReadOnlyDictionary<char, char>(
                parenthesesMap);
        }



        public static RepresentPartParseInfo Get(NestedType type)
        {
            switch (type)
            {
                case NestedType.Element:
                    return ElementInfo;
                case NestedType.Array:
                    return ArrayInfo;
                case NestedType.Collection:
                    return CollectionInfo;
                default:
                    var exception = new ArgumentException(
                        $"Invalid value for the {nameof(type)} parameter",
                        nameof(type));
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }
        }
        public static RepresentPartParseInfo Get(char startChar)
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
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }
        }
        public static RepresentPartParseInfo Get(string represent, int startIndex)
        {
            if (string.IsNullOrEmpty(represent))
            {
                var exception =
                    new ArgumentOutOfRangeException(nameof(startIndex), $"{nameof(startIndex)} cannot be null or empty");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
            if (startIndex < 0)
            {
                var exception =
                    new ArgumentOutOfRangeException(nameof(startIndex), $"{nameof(startIndex)} cannot be less than zero");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
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
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }
        }
    }
}
