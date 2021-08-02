// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Windows;

namespace RIS.Graphics.WPF.Localization.Entities
{
    internal class LocalizationSource : ILocalizationSource
    {
        public readonly FrameworkElement Element;



        private LocalizationSource(FrameworkElement element)
        {
            Element = element;
        }



        public static implicit operator FrameworkElement(LocalizationSource source)
        {
            return source.Element;
        }


        public static explicit operator LocalizationSource(FrameworkElement element)
        {
            return From(element);
        }



        public static LocalizationSource From<T>(T element)
            where T : FrameworkElement
        {
            return new LocalizationSource(element);
        }
    }
}
