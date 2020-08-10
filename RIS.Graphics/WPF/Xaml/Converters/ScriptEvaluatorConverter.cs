// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RIS.Graphics.WPF.Xaml.Converters
{
    public class ScriptEvaluatorConverter : IMultiValueConverter, IValueConverter
    {
        private bool _trapExceptions;
        public bool TrapExceptions
        {
            get
            {
                return _trapExceptions;
            }
            set
            {
                _trapExceptions = true;
            }
        }

        public object Convert(object[] values, Type targetType,
            object parameter, CultureInfo culture)
        {
            try
            {
                string parameterString = parameter.ToString();
                NCalc.Expression parameterExpression = new NCalc.Expression(parameterString);

                for (int i = 0; i < values.Length; ++i)
                {
                    parameterExpression.Parameters.Add($"@values[{i}]", values[i]);
                }

                return Task.Factory.StartNew(() => parameterExpression.Evaluate());
            }
            catch
            {
                if (TrapExceptions)
                    return null;

                throw;
            }
        }
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return Convert(new[] { value }, targetType, parameter, culture);
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