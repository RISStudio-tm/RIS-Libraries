// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Globalization;
using System.Reflection;

namespace RIS.Localization
{
    internal sealed class LocalizedProperty
    {
        private const BindingFlags AccessBindingFlags = BindingFlags.Instance
                                                        | BindingFlags.Public
                                                        | BindingFlags.NonPublic;



        private readonly LocalizedListBase _localizedListBase;
        private readonly PropertyInfo _propertyInfo;



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

            LocalizationKey = localizationKey;
            NonLocalizedValue = nonLocalizedValue;
        }



        public object GetValue()
        {
            try
            {
                if (!_propertyInfo.CanRead)
                    return null;

                return _propertyInfo.GetValue(_localizedListBase,
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

                _propertyInfo.SetValue(_localizedListBase,
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
