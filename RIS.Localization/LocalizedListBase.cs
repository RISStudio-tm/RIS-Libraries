// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using RIS.Extensions;

namespace RIS.Localization
{
    public abstract class LocalizedListBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;



        private readonly LocalizationFactory _localizationFactory;
        private readonly ReadOnlyDictionary<string, LocalizedProperty> _propertyMappings;
        private readonly bool _notifyPerProperty;
        private readonly EventHandler<LocalizationEventArgs> _localizationUpdatedHandler;



        protected LocalizedListBase(
            LocalizationFactory factory,
            bool notifyPerProperty = false)
        {
            _localizationFactory = factory;
            _propertyMappings = GetPropertyMappings();
            _notifyPerProperty = notifyPerProperty;
            _localizationUpdatedHandler = GetLocalizationUpdatedHandler();

            _localizationUpdatedHandler.Invoke(
                _localizationFactory,
                new LocalizationEventArgs(
                    _localizationFactory));

            _localizationFactory.LocalizationUpdated +=
                _localizationUpdatedHandler;
        }

        ~LocalizedListBase()
        {
            _localizationFactory.LocalizationUpdated -=
                _localizationUpdatedHandler;
        }



        private ReadOnlyDictionary<string, LocalizedProperty> GetPropertyMappings()
        {
            var propertyMappings = new Dictionary<string, LocalizedProperty>();

            foreach (var property in GetType().GetProperties(BindingFlags.Instance
                                                                      | BindingFlags.Public
                                                                      | BindingFlags.GetProperty
                                                                      | BindingFlags.SetProperty))
            {
                if (!Attribute.IsDefined(property, typeof(LocalizationKeyAttribute)))
                    continue;

                var localizationKey = property
                    .GetCustomAttribute<LocalizationKeyAttribute>()?
                    .Key;

                if (localizationKey == null)
                    continue;

                object nonLocalizedValue = null;

                if (Attribute.IsDefined(property, typeof(NonLocalizedValueAttribute)))
                {
                    nonLocalizedValue = property
                        .GetCustomAttribute<NonLocalizedValueAttribute>()?
                        .Value;
                }

                var declaringProperty = property
                    .GetDeclaring();
                var localizedProperty = new LocalizedProperty(
                    this,
                    declaringProperty,
                    localizationKey,
                    nonLocalizedValue);

                propertyMappings.Add(
                    localizationKey,
                    localizedProperty);
            }

            return new ReadOnlyDictionary<string, LocalizedProperty>(
                propertyMappings);
        }

        private EventHandler<LocalizationEventArgs> GetLocalizationUpdatedHandler()
        {
            if (_notifyPerProperty)
                return OnLocalizationUpdated_PerProperty;

            return OnLocalizationUpdated_AllProperty;
        }

        private void UpdateValue(
            LocalizedProperty property)
        {
            var key = property.LocalizationKey;
            var defaultValue = property.NonLocalizedValue;
            string value;

            if (defaultValue == null)
            {
                value = _localizationFactory
                    .GetLocalized(key);
            }
            else
            {
                if (!_localizationFactory.TryGetLocalized(key, out value))
                    value = defaultValue.ToString();
            }

            property.SetValue(
                value);
        }



        private void OnLocalizationUpdated_PerProperty(object sender,
            LocalizationEventArgs e)
        {
            foreach (var property in _propertyMappings.Values)
            {
                UpdateValue(
                    property);

                OnPropertyChanged(
                    property.Name);
            }
        }

        private void OnLocalizationUpdated_AllProperty(object sender,
            LocalizationEventArgs e)
        {
            foreach (var property in _propertyMappings.Values)
            {
                UpdateValue(
                    property);
            }

            OnPropertyChanged(
                string.Empty);
        }



        protected void OnPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
