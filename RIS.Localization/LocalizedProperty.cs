// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Globalization;
using System.Reflection;
using RIS.Extensions;

namespace RIS.Localization
{
    internal sealed class LocalizedProperty
    {
        private const BindingFlags AccessBindingFlags = BindingFlags.Instance
                                                        | BindingFlags.Static
                                                        | BindingFlags.Public
                                                        | BindingFlags.NonPublic;



        private readonly LocalizedListBase _localizedListBase;
        private readonly PropertyInfo _propertyInfo;

        private readonly object _source;



        public string Name
        {
            get
            {
                return _propertyInfo.Name;
            }
        }
        public Type Type
        {
            get
            {
                return _propertyInfo.PropertyType;
            }
        }

        public bool IsStatic { get; }
        public string LocalizationKey { get; }
        public object NonLocalizedValue { get; }



        public LocalizedProperty(
            LocalizedListBase localizedList,
            PropertyInfo propertyInfo,
            string localizationKey,
            object nonLocalizedValue = null)
        {
            _localizedListBase = localizedList;
            _propertyInfo = propertyInfo;

            IsStatic = _propertyInfo.IsStatic();
            LocalizationKey = localizationKey;
            NonLocalizedValue = nonLocalizedValue;

            _source = !IsStatic
                ? _localizedListBase
                : null;
        }



        public object GetValue()
        {
            try
            {
                if (!_propertyInfo.CanRead)
                    return null;

                return _propertyInfo.GetValue(_source,
                    AccessBindingFlags, null, null,
                    CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }

        public void SetValue(
            object value)
        {
            try
            {
                if (!_propertyInfo.CanWrite)
                    return;

                _propertyInfo.SetValue(_source,
                    Convert.ChangeType(value, Type, CultureInfo.InvariantCulture),
                    AccessBindingFlags, null, null,
                    CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }
    }
}
