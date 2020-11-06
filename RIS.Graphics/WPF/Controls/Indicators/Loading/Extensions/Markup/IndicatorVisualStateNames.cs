// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Windows.Markup;

namespace RIS.Graphics.WPF.Controls.Indicators.Loading.Extensions.Markup
{
    internal sealed class IndicatorVisualStateNames : MarkupExtension
    {
        private static IndicatorVisualStateNames _activeState;
        public static IndicatorVisualStateNames ActiveState
        {
            get
            {
                return _activeState ??=
                    new IndicatorVisualStateNames("Active");
            }
        }
        private static IndicatorVisualStateNames _inactiveState;
        public static IndicatorVisualStateNames InactiveState
        {
            get
            {
                return _inactiveState ??=
                    new IndicatorVisualStateNames("Inactive");
            }
        }

        public string Name { get; }

        private IndicatorVisualStateNames(string name)
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