// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Globalization;
using System.Windows.Data;
using NCalc;

namespace RIS.Graphics.Markup
{
    public sealed class ScriptEvaluatorConverter : IValueConverter, IMultiValueConverter
    {
        public bool ThrowExceptions { get; set; }



        public object Convert(object[] values, Type targetType,
            object parameter, CultureInfo culture)
        {
            try
            {
                var parameterString = parameter
                    .ToString();
                var parameterExpression = new Expression(
                    parameterString);

                for (int i = 0; i < values.Length; ++i)
                {
                    parameterExpression.Parameters.Add(
                        $"values{{{i}}}", values[i]);
                }

                return parameterExpression
                    .Evaluate();
            }
            catch
            {
                if (!ThrowExceptions)
                    return Binding.DoNothing;

                throw;
            }
        }
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return Convert(new[] { value },
                targetType, parameter, culture);
        }


        public object[] ConvertBack(object value, Type[] targetTypes,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}