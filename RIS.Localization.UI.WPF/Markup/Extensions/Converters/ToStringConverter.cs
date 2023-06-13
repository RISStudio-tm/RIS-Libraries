// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.ComponentModel;
using System.Globalization;

namespace RIS.Localization.UI.WPF.Markup.Extensions.Converters
{
    internal sealed class ToStringConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context,
            Type sourceType)
        {
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture, object value)
        {
            if (value is string)
                return value.ToString();

            try
            {
                return Convert.ChangeType(value, typeof(string));
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
                return value.ToString();

            return string.Empty;
        }
    }
}
