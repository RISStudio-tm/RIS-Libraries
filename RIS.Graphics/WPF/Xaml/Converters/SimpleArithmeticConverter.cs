using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace RIS.Graphics.WPF.Xaml.Converters
{
    public class SimpleArithmeticConverter : IValueConverter
    {
        private const string ArithmeticParseExpression = "([+\\-*/]{1,1})\\s{0,}(\\-?[\\d\\.]+)";
        private readonly Regex _arithmeticRegex = new Regex(ArithmeticParseExpression);

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double valueDouble) || parameter == null)
                return null;

            string param = parameter.ToString();

            if (param?.Length == 0)
                return null;

            Match match = _arithmeticRegex.Match(param ?? string.Empty);

            if (!match.Success || match.Groups.Count != 3)
                return null;

            string operation = match.Groups[1].Value.Trim();
            string numericValue = match.Groups[2].Value;

            if (!double.TryParse(numericValue, out double number))
                return null;

            switch (operation)
            {
                case "+":
                    return valueDouble + number;
                case "-":
                    return valueDouble - number;
                case "*":
                    return valueDouble * number;
                case "/":
                    return valueDouble / number;
                default:
                    return null;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}