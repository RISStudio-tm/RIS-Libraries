// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RIS.Localization.UI.WPF.Markup.Extensions.Entities
{
    internal sealed class BindingValueProvider : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;



        public static readonly PropertyInfo ValueProperty;

        private object _value;
        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }



        static BindingValueProvider()
        {
            ValueProperty = typeof(BindingValueProvider)
                .GetProperty(nameof(Value));
        }



        private void OnPropertyChanged(
            [CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
