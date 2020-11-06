// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Windows;
using System.Windows.Markup;

namespace RIS.Graphics.WPF.Controls.Indicators.Loading.Extensions.Markup
{
    [MarkupExtensionReturnType(typeof(Style))]
    public class MultiStyle : MarkupExtension
    {
        private readonly string[] _resourceKeys;

        public MultiStyle(string inputResourceKeys)
        {
            if (string.IsNullOrWhiteSpace(inputResourceKeys))
                throw new ArgumentNullException(nameof(inputResourceKeys));

            _resourceKeys = inputResourceKeys.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            if (_resourceKeys.Length == 0)
                throw new ArgumentException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var resultStyle = new Style();

            foreach (var resourceKey in _resourceKeys)
            {
                var key = (object) resourceKey;

                if (resourceKey == ".")
                {
                    var service = (IProvideValueTarget) serviceProvider.GetService(typeof(IProvideValueTarget));
                    key = service.TargetObject.GetType();
                }

                if (!(new StaticResourceExtension(key).ProvideValue(serviceProvider) is Style currentStyle))
                    throw new ArgumentException();

                resultStyle.Merge(currentStyle);
            }

            return resultStyle;
        }
    }
}