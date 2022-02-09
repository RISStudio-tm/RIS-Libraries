// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace RIS.Graphics.Markup
{
    public sealed class SimpleArithmeticConverter : IValueConverter
    {
        private static readonly Regex ArithmeticRegex;



        public bool ThrowExceptions { get; set; }



        static SimpleArithmeticConverter()
        {
            ArithmeticRegex = new Regex(
                "([+\\-*/]{1,1})\\s{0,}(\\-?[\\d\\.]+)");
        }



        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            try
            {
                if (!(value is double valueDouble) || parameter == null)
                    return null;

                var param = parameter.ToString();

                if (param?.Length == 0)
                    return null;

                var match = ArithmeticRegex.Match(
                    param ?? string.Empty);

                if (!match.Success || match.Groups.Count != 3)
                    return null;

                var operation = match
                    .Groups[1]
                    .Value
                    .Trim();
                var numericValue = match
                    .Groups[2]
                    .Value;

                if (!double.TryParse(numericValue, out var number))
                    return null;

                return operation switch
                {
                    "+" => valueDouble + number,
                    "-" => valueDouble - number,
                    "*" => valueDouble * number,
                    "/" => valueDouble / number,
                    _ => null
                };
            }
            catch
            {
                if (!ThrowExceptions)
                    return Binding.DoNothing;

                throw;
            }
        }


        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}