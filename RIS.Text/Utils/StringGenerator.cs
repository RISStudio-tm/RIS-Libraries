﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Text;

namespace RIS.Text.Encoding
{
    public static class StringGenerator
    {
        private const string LettersAndDigits = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string GetAlphabet(int charsCount)
        {
            var result = new StringBuilder(charsCount);
            int i = 0;
            int count = 0;
            do
            {
                char c = (char)i;
                if (!char.IsControl(c) && !char.IsWhiteSpace(c))
                {
                    result.Append(c);
                    count++;
                }
                i++;
            }
            while (count < charsCount);

            return result.ToString();
        }

        public static string GetRandom(int size, bool onlyLettersAndDigits)
        {
            Random r = new Random();
            if (onlyLettersAndDigits)
            {
                var result = new char[size];
                for (int i = 0; i < size; i++)
                    result[i] = LettersAndDigits[r.Next(LettersAndDigits.Length)];
                return new string(result);
            }

            var data = new byte[size];
            for (int i = 0; i < size; i++)
                data[i] = (byte) r.Next(32, 127);
            var encoding = new ASCIIEncoding();
            return encoding.GetString(data);
        }
    }
}
