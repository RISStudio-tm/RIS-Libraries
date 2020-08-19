// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Extensions
{
    public static class ConsoleExtensions
    {
        public static void WriteColored(string text, ConsoleColor color)
        {
            ConsoleColor colorSource = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = colorSource;
        }

        public static void WriteLineColored(string text, ConsoleColor color)
        {
            ConsoleColor colorSource = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = colorSource;
        }
    }
}
