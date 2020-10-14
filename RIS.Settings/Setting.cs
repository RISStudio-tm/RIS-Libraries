// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Reflection;
using RIS.Collections.Nestable;

namespace RIS.Settings
{
    public sealed class Setting
    {
        private readonly SettingsBase _settingsBase;
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
        public string CategoryName { get; }

        public Setting(SettingsBase settings, PropertyInfo propertyInfo, string category = null)
        {
            _settingsBase = settings;
            _propertyInfo = propertyInfo;
            CategoryName = category;
        }

        public object GetValue()
        {
            return _propertyInfo.GetValue(_settingsBase);
        }

        public string GetValueToString()
        {
            object value = GetValue();

            switch (value)
            {
                case null:
                    return string.Empty;
                case byte[] array:
                {
                    NestableArrayCAL<byte> nestableArray = new NestableArrayCAL<byte>();

                    for (int i = 0; i < array.Length; ++i)
                    {
                        nestableArray.Add(array[i]);
                    }

                    return nestableArray.ToStringRepresent();
                }
                case string[] array:
                {
                    NestableArrayCAL<string> nestableArray = new NestableArrayCAL<string>();

                    for (int i = 0; i < array.Length; ++i)
                    {
                        nestableArray.Add(array[i]);
                    }

                    return nestableArray.ToStringRepresent();
                }
                default:
                    return value.ToString();
            }
        }

        public void SetValue(object value)
        {
            if (value == null)
                return;

            try
            {
                _propertyInfo.SetValue(_settingsBase, Convert.ChangeType(value, Type));
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }

        public void SetValueFromString(string value)
        {
            if (value == null)
                return;

            if (Type == typeof(byte[]))
            {
                NestableArrayCAL<byte> nestableArray = new NestableArrayCAL<byte>();

                nestableArray.FromStringRepresent(value);

                byte[] array = new byte[nestableArray.Length];

                for (int i = 0; i < nestableArray.Length; ++i)
                {
                    array[i] = nestableArray[i].GetElement();
                }

                SetValue(array);
            }
            else if (Type == typeof(string[]))
            {
                NestableArrayCAL<string> nestableArray = new NestableArrayCAL<string>();

                nestableArray.FromStringRepresent(value);

                string[] array = new string[nestableArray.Length];

                for (int i = 0; i < nestableArray.Length; ++i)
                {
                    array[i] = nestableArray[i].GetElement();
                }

                SetValue(array);
            }
            else
            {
                SetValue(value);
            }
        }
    }
}
