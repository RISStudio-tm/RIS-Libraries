// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Linq;

namespace RIS.Settings.Ini
{
    public sealed class IniBoolOptions
    {
        private Dictionary<string, bool> _boolStringMap;
        private string _trueString = bool.TrueString;
        private string _falseString = bool.FalseString;

        public bool NonZeroNumbersAreTrue { get; }

        public IniBoolOptions(bool nonZeroNumbersAreTrue = true, StringComparer comparer = null)
        {
            _boolStringMap = new Dictionary<string, bool>(comparer ?? StringComparer.OrdinalIgnoreCase)
            {
                [_trueString] = true,
                [_falseString] = false,
                ["yes"] = true,
                ["no"] = false,
                ["on"] = true,
                ["off"] = false,
                ["1"] = true,
                ["0"] = false
            };

            NonZeroNumbersAreTrue = nonZeroNumbersAreTrue;
        }

        public void SetMap(IEnumerable<IniBoolString> boolStrings)
        {
            if (boolStrings == null)
                throw new ArgumentNullException(nameof(boolStrings));

            IniBoolString[] boolStringsArray = boolStrings as IniBoolString[] ?? boolStrings.ToArray();

            IniBoolString boolStringTemp = Array.Find(boolStringsArray, boolString => boolString.Bool);

            if (boolStringTemp == null)
            {
                var exception = new InvalidOperationException("Boolean->word list contains no entry for 'true' values.");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            _trueString = boolStringTemp.String;

            boolStringTemp = Array.Find(boolStringsArray, boolString => !boolString.Bool);

            if (boolStringTemp == null)
            {
                var exception = new InvalidOperationException("Boolean->word list contains no entry for 'false' values.");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            _falseString = boolStringTemp.String;

            _boolStringMap = boolStringsArray.ToDictionary(boolString => boolString.String, boolString => boolString.Bool);
        }

        public string ToString(bool value)
        {
            return value ? _trueString : _falseString;
        }

        public bool TryParse(string s, out bool value)
        {
            if (s != null)
            {
                if (_boolStringMap.TryGetValue(s, out bool b))
                {
                    value = b;

                    return true;
                }

                if (NonZeroNumbersAreTrue && int.TryParse(s, out int i))
                {
                    value = (i != 0);

                    return true;
                }
            }

            value = false;

            return false;
        }
    }
}
