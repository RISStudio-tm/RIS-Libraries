// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using RIS.Localization.UI.WPF.Markup.Extensions.Converters;
using RIS.Localization.UI.WPF.Markup.Extensions.Entities;

namespace RIS.Localization.UI.WPF.Markup
{
    [TypeConverter(typeof(ToStringConverter))]
    [MarkupExtensionReturnType(typeof(string))]
    public class LocalizedStringExtension : MarkupExtension, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;



        private string _resourceKey;
        [ConstructorArgument("resourceKey")]
        public string ResourceKey
        {
            get
            {
                return _resourceKey;
            }
            set
            {
                _resourceKey = value
                               ?? throw new ArgumentNullException(nameof(value));
                OnPropertyChanged(nameof(ResourceKey));

                UpdateValue();
            }
        }
        public IValueConverter Converter { get; set; }
        public object ConverterParameter { get; set; }


        private WeakReference TargetObject { get; set; }
        private object TargetProperty { get; set; }
        private Type TargetPropertyType { get; set; }



        public LocalizedStringExtension(
            string resourceKey)
        {
            _resourceKey = resourceKey
                           ?? throw new ArgumentNullException(nameof(resourceKey));

            LocalizationManager.DefaultLocalizationChanged += LocalizationManager_DefaultLocalizationChanged;
            LocalizationManager.LocalizationChanged += LocalizationManager_LocalizationChanged;
            LocalizationManager.LocalizationUpdated += LocalizationManager_LocalizationUpdated;
        }

        ~LocalizedStringExtension()
        {
            LocalizationManager.DefaultLocalizationChanged -= LocalizationManager_DefaultLocalizationChanged;
            LocalizationManager.LocalizationChanged -= LocalizationManager_LocalizationChanged;
            LocalizationManager.LocalizationUpdated -= LocalizationManager_LocalizationUpdated;
        }



        private object FormatValue(
            object value)
        {
            Type propertyType = TargetPropertyType;

            if (TargetProperty is DependencyProperty dependencyProperty)
                propertyType = dependencyProperty.PropertyType;

            if (Converter != null)
            {
                var culture = LocalizationManager.CurrentUIFactory?.CurrentLocalization?
                    .Culture ?? CultureInfo.InvariantCulture;

                value = Converter.Convert(
                    value,
                    propertyType,
                    ConverterParameter,
                    culture);
            }
            else
            {
                TypeConverter converter = TypeDescriptor
                    .GetConverter(propertyType);

                if (converter.CanConvertFrom(value.GetType()))
                {
                    value = converter.ConvertFrom(
                        value);
                }
            }

            return value;
        }

        private void UpdateValue()
        {
            var targetObject = TargetObject.Target;

            if (targetObject == null)
                return;
            if (targetObject is DependencyObject { IsSealed: true })
                return;

            object value = LocalizationManager.CurrentUIFactory?
                .GetLocalized(ResourceKey);

            if (value == null)
            {
                if (TargetPropertyType.IsValueType)
                    value = Activator.CreateInstance(TargetPropertyType);
                else
                    value = ToString();
            }

            if (TargetObject == null
                || TargetProperty == null)
            {
                return;
            }

            value = FormatValue(value);

            if (TargetProperty is DependencyProperty dependencyProperty)
            {
                if (targetObject is DependencyObject targetDependencyObject)
                {
                    targetDependencyObject.SetValue(
                        dependencyProperty, value);
                }
            }
            else if (TargetProperty is PropertyInfo propertyInfo)
            {
                propertyInfo.SetValue(
                    targetObject, value, null);
            }
        }



        public override object ProvideValue(
            IServiceProvider serviceProvider)
        {
            var value = ProvideValueInternal(
                serviceProvider);

            if (value == null
                || value == this
                || TargetObject == null
                || TargetProperty == null)
            {
                return value;
            }

            return FormatValue(value);
        }
        private object ProvideValueInternal(
            IServiceProvider serviceProvider)
        {
            if (ResourceKey == null)
                throw new ArgumentNullException(nameof(ResourceKey));

            if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service))
                return this;

            var targetObject = service.TargetObject;
            var targetProperty = service.TargetProperty;
            Type targetPropertyType = null;

            object result = null;

            if (targetObject is Setter setter)
            {
                targetObject = new BindingValueProvider();
                targetProperty = BindingValueProvider.ValueProperty;
                targetPropertyType = setter.Property?.PropertyType ?? typeof(object);

                result = new Binding(nameof(BindingValueProvider.Value))
                {
                    Source = targetObject,
                    Mode = BindingMode.OneWay
                };
            }
            else if (targetObject is Binding binding)
            {
                binding.Path = new PropertyPath(nameof(BindingValueProvider.Value));
                binding.Mode = BindingMode.OneWay;

                targetObject = new BindingValueProvider();
                targetProperty = BindingValueProvider.ValueProperty;

                result = targetObject;
            }

            TargetObject = new WeakReference(targetObject);
            TargetProperty = targetProperty;

            if (targetPropertyType == null)
            {
                if (targetProperty is PropertyInfo propertyInfo)
                {
                    targetPropertyType = propertyInfo.PropertyType;

                    if (propertyInfo.GetIndexParameters().Length != 0)
                        throw new InvalidOperationException("Indexers are not supported");
                }
                else if (targetProperty is DependencyProperty dependencyProperty)
                {
                    targetPropertyType = dependencyProperty.PropertyType;
                }
                else
                {
                    return this;
                }
            }

            TargetPropertyType = targetPropertyType;

            if (targetObject is DictionaryEntry)
                return null;

            if (result != null)
            {
                UpdateValue();

                return result;
            }

            result = LocalizationManager.CurrentUIFactory?
                .GetLocalized(ResourceKey);

            if (result == null)
            {
                if (TargetPropertyType.IsValueType)
                    result = Activator.CreateInstance(TargetPropertyType);
                else
                    result = ToString();
            }

            if (typeof(IList<>).IsAssignableFrom(targetPropertyType))
                return result;
            if (result != null && targetPropertyType.IsInstanceOfType(result))
                return result;

            return targetPropertyType.IsValueType
                ? Activator.CreateInstance(targetPropertyType)
                : null;
        }



        public override string ToString()
        {
            return "$" + ResourceKey;
        }



        private void LocalizationManager_DefaultLocalizationChanged(object sender, LocalizationChangedEventArgs e)
        {
            UpdateValue();
        }

        private void LocalizationManager_LocalizationChanged(object sender, LocalizationChangedEventArgs e)
        {
            UpdateValue();
        }

        private void LocalizationManager_LocalizationUpdated(object sender, EventArgs e)
        {
            UpdateValue();
        }



        protected void OnPropertyChanged(
            [CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(property));
        }
    }
}
