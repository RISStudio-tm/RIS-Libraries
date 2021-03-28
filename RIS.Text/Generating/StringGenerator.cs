// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using RIS.Extensions;
using RIS.Randomizing;

namespace RIS.Text.Generating
{
    public class StringGenerator
    {
        public static readonly Regex GenerateStringFormatRegex = new Regex(@"(?<expression>(?:\((?<type>char/digit|char-lower/digit|char-upper/digit|char|char-lower|char-upper|digit|digit-notzero){1}\)){1}(?:\[(?<count>(?:[0]|[1-9][0-9]*))\])?){1}", RegexOptions.Multiline, TimeSpan.FromSeconds(10));

        public static readonly char[] DefaultAlphabet;
        public static readonly char[] DefaultSpecialAlphabet;

        public static readonly char[] CharsAlphabet;
        public static readonly char[] CharsLowerAlphabet;
        public static readonly char[] CharsUpperAlphabet;

        public static readonly char[] DigitsAlphabet;
        public static readonly char[] DigitsNotZeroAlphabet;

        public static readonly char[] CharsAndDigitsAlphabet;
        public static readonly char[] CharsLowerAndDigitsAlphabet;
        public static readonly char[] CharsUpperAndDigitsAlphabet;


        public static readonly StringGenerator Default;



        private readonly RNGCryptoServiceProvider _randomGenerator;


        private IBiasedRandom _biasedRandomGenerator;
        public IBiasedRandom BiasedRandomGenerator
        {
            get
            {
                return _biasedRandomGenerator;
            }
            set
            {
                if (value == null)
                    return;

                _biasedRandomGenerator = value;
            }
        }



        static StringGenerator()
        {
            Default = new StringGenerator(
                new SecureRandom());

            DefaultAlphabet =
                GetAlphabet(new (char start, char end)[]
                {
                    ('a', 'z'),
                    ('A', 'Z'),
                    ('0', '9')
                }).ToArray();
            DefaultSpecialAlphabet =
                GetAlphabet(127 - 32, 32 + 1)
                    .ToArray();

            CharsLowerAlphabet =
                GetAlphabet(new (char start, char end)[]
                {
                    ('a', 'z')
                }).ToArray();
            CharsUpperAlphabet =
                GetAlphabet(new (char start, char end)[]
                {
                    ('A', 'Z')
                }).ToArray();
            CharsAlphabet = CharsLowerAlphabet
                .Concat(CharsUpperAlphabet)
                .ToArray();

            DigitsAlphabet =
                GetAlphabet(new (char start, char end)[]
                {
                    ('0', '9')
                }).ToArray();
            DigitsNotZeroAlphabet = DigitsAlphabet
                .Skip(1)
                .ToArray();

            CharsAndDigitsAlphabet = CharsAlphabet
                .Concat(DigitsAlphabet)
                .ToArray();
            CharsLowerAndDigitsAlphabet = CharsLowerAlphabet
                .Concat(DigitsAlphabet)
                .ToArray();
            CharsUpperAndDigitsAlphabet = CharsUpperAlphabet
                .Concat(DigitsAlphabet)
                .ToArray();
        }

        public StringGenerator(IBiasedRandom randomGenerator = null)
        {
            if (randomGenerator == null)
                randomGenerator = new SecureRandom();

            _randomGenerator = new RNGCryptoServiceProvider();

            BiasedRandomGenerator = randomGenerator;
        }



        public static IEnumerable<char> GetAlphabet(int count,
            int startPosition = 0)
        {
            if (count <= 0)
                return Enumerable.Empty<char>();

            if (startPosition < 0)
                startPosition = 0;

            var result = new char[count];

            int i = startPosition;
            int foundedCount = 0;

            do
            {
                char ch = (char)i;

                if (!char.IsControl(ch)
                    && !char.IsWhiteSpace(ch))
                {
                    result[foundedCount] = ch;
                    ++foundedCount;
                }

                ++i;
            }
            while (foundedCount < count);

            return result;
        }
        public static IEnumerable<char> GetAlphabet(
            (char start, char end)[] intervals)
        {
            var result = new List<char>(100);

            if (intervals == null || intervals.Length == 0)
                return result;

            foreach (var (start, end) in intervals)
            {
                if (start > end)
                    continue;

                if (start == end)
                {
                    result.Add(start);
                    continue;
                }

                var range = new List<char>(end - start + 1);

                for (var ch = start; ch <= end; ++ch)
                {
                    range.Add(ch);
                }

                result.AddRange(range);
            }

            return result;
        }



        public string GetRandom(int minSize, int maxSize,
            bool onlyLettersAndDigits = true)
        {
            return GetRandom(
                minSize,
                maxSize,
                onlyLettersAndDigits
                    ? DefaultAlphabet
                    : DefaultSpecialAlphabet);
        }
        public string GetRandom(int minSize, int maxSize,
            IEnumerable<char> alphabet)
        {
            int size = minSize < maxSize
                ? Rand.Next(minSize, maxSize)
                : minSize;

            return GetRandom(size, alphabet);
        }
        public string GetRandom(int size,
            bool onlyLettersAndDigits = true)
        {
            return GetRandom(
                size,
                onlyLettersAndDigits
                    ? DefaultAlphabet
                    : DefaultSpecialAlphabet);
        }
        public string GetRandom(int size,
            IEnumerable<char> alphabet)
        {
            if (size == 0)
                return string.Empty;

            var alphabetArray = alphabet
                .ToArray();

            if (alphabetArray.Length == 0)
            {
                var exception =
                    new Exception("Alphabet must contain 1 or more characters");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            char[] result = new char[size];
            Random random = Rand.CreateRandom();

            for (int i = 0; i < size; ++i)
            {
                var charIndex = random.Next(alphabetArray.Length);

                result[i] = alphabetArray[charIndex];
            }

            return new string(result);
        }


        public string GenerateString(int minSize, int maxSize,
            bool onlyLettersAndDigits = true)
        {
            return GenerateString(
                minSize,
                maxSize,
                onlyLettersAndDigits
                    ? DefaultAlphabet
                    : DefaultSpecialAlphabet);
        }
        public string GenerateString(int minSize, int maxSize,
            IEnumerable<char> alphabet)
        {
            int size = minSize < maxSize
                ? _randomGenerator.GenerateInt(minSize, maxSize)
                : minSize;

            return GenerateString(size, alphabet);
        }
        public string GenerateString(int size,
            bool onlyLettersAndDigits = true)
        {
            return GenerateString(
                size,
                onlyLettersAndDigits
                    ? DefaultAlphabet
                    : DefaultSpecialAlphabet);
        }
        public string GenerateString(int size,
            IEnumerable<char> alphabet)
        {
            if (size == 0)
                return string.Empty;

            var alphabetArray = alphabet
                .ToArray();

            if (alphabetArray.Length == 0)
            {
                var exception =
                    new Exception("Alphabet must contain 1 or more characters");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            var result = new char[size];
            var randomNumbers = new ushort[size];
            var biasZone =
                (ushort)(ushort.MaxValue - ((ushort.MaxValue + 1) % alphabetArray.Length));

            BiasedRandomGenerator.GetUInt16(randomNumbers, biasZone);

            for (var i = 0; i < size; ++i)
            {
                var charIndex = randomNumbers[i] % alphabetArray.Length;

                result[i] = alphabetArray[charIndex];
            }

            return new string(result);
        }

        public string GenerateString(string format)
        {
            var result = new StringBuilder(format);
            var matches = GenerateStringFormatRegex.Matches(format);

            if (matches.Count == 0)
                return format;

            foreach (Match match in matches)
            {
                if (!match.Success)
                    continue;

                var expression = match.Groups["expression"].Value;
                var type = match.Groups["type"].Value;
                var count = !string.IsNullOrEmpty(match.Groups["count"]?.Value)
                    ? match.Groups["count"].Value.ToInt()
                    : 1;

                char[] alphabet;

                switch (type)
                {
                    case "char":
                        alphabet = CharsAlphabet;
                        break;
                    case "char-lower":
                        alphabet = CharsLowerAlphabet;
                        break;
                    case "char-upper":
                        alphabet = CharsUpperAlphabet;
                        break;
                    case "digit":
                        alphabet = DigitsAlphabet;
                        break;
                    case "digit-notzero":
                        alphabet = DigitsNotZeroAlphabet;
                        break;
                    case "char/digit":
                        alphabet = CharsAndDigitsAlphabet;
                        break;
                    case "char-lower/digit":
                        alphabet = CharsLowerAndDigitsAlphabet;
                        break;
                    case "char-upper/digit":
                        alphabet = CharsUpperAndDigitsAlphabet;
                        break;
                    default:
                        var exception =
                            new Exception($"Unknown generation type[{type}] for expression[{expression}]");
                        Events.OnError(new RErrorEventArgs(exception, exception.Message));
                        throw exception;
                }

                var generatedString = GenerateString(count, alphabet);
                int index = result.IndexOf(expression);

                result = result.Remove(index, expression.Length).Insert(index, generatedString);
            }

            return result.ToString();
        }
    }
}
