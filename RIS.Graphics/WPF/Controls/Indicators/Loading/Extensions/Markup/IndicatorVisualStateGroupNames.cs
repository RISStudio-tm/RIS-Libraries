// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Windows.Markup;

namespace RIS.Graphics.WPF.Controls.Indicators.Loading.Extensions.Markup
{
    internal sealed class IndicatorVisualStateGroupNames : MarkupExtension
    {
        private static IndicatorVisualStateGroupNames _internalActiveStates;
        public static IndicatorVisualStateGroupNames ActiveStates
        {
            get
            {
                return _internalActiveStates ??=
                    new IndicatorVisualStateGroupNames("ActiveStates");
            }
        }
        private static IndicatorVisualStateGroupNames _sizeStates;
        public static IndicatorVisualStateGroupNames SizeStates
        {
            get
            {
                return _sizeStates ??=
                    new IndicatorVisualStateGroupNames("SizeStates");
            }
        }

        public string Name { get; }

        private IndicatorVisualStateGroupNames(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Name;
        }
    }
}