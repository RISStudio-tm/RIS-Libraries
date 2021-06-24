// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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

        private static string ToStringRepresent<T>(INestableCollection<T> collection)
        {
            return collection.ToStringRepresent();
        }
        private static string ToStringRepresent<T>(ICollection<T> collection)
        {
            var nestableArray = new NestableArrayCAL<T>();

            foreach (var element in collection)
            {
                nestableArray.Add(element);
            }

            return nestableArray.ToStringRepresent();
        }
        private static string ToStringRepresent<T>(IEnumerable<T> collection)
        {
            var nestableArray = new NestableArrayCAL<T>();

            foreach (var element in collection)
            {
                nestableArray.Add(element);
            }

            return nestableArray.ToStringRepresent();
        }

        private static INestableCollection<T> FromStringRepresentNestable<T>(string represent)
        {
            return NestableHelper.FromStringRepresent<T>(represent);
        }
        private static ICollection<T> FromStringRepresentArray<T>(string represent)
        {
            var nestableArray = new NestableArrayCAL<T>();

            nestableArray.FromStringRepresent(represent);

            var collection = new T[nestableArray.Length];

            for (int i = 0; i < nestableArray.Length; ++i)
            {
                collection[i] = nestableArray[i].GetElement();
            }

            return collection;
        }
        private static ICollection<T> FromStringRepresentCollection<T>(string represent)
        {
            var nestableArray = new NestableArrayCAL<T>();

            nestableArray.FromStringRepresent(represent);

            var collection = new List<T>(nestableArray.Length);

            for (int i = 0; i < nestableArray.Length; ++i)
            {
                collection.Add(nestableArray[i].GetElement());
            }

            return collection;
        }
        private static IEnumerable<T> FromStringRepresentEnumerable<T>(string represent)
        {
            var nestableArray = new NestableArrayCAL<T>();

            nestableArray.FromStringRepresent(represent);

            var collection = new List<T>(nestableArray.Length);

            for (int i = 0; i < nestableArray.Length; ++i)
            {
                collection.Add(nestableArray[i].GetElement());
            }

            return collection;
        }

        public object GetValue()
        {
            return _propertyInfo.GetValue(_settingsBase);
        }

        public string GetValueToString()
        {
            var value = GetValue();

            switch (value)
            {
                case null:
                    return string.Empty;
                case string _:
                    return value.ToString();
                case float settingValue:
                    return settingValue.ToString(CultureInfo.InvariantCulture);
                case double settingValue:
                    return settingValue.ToString(CultureInfo.InvariantCulture);
                case decimal settingValue:
                    return settingValue.ToString(CultureInfo.InvariantCulture);
                case DateTime settingValue:
                    return settingValue.ToString(CultureInfo.InvariantCulture);
                case INestableCollection collectionValue:
                    switch (collectionValue)
                    {
                        case INestableCollection<string> settingValue:
                            return ToStringRepresent(settingValue);
                        case INestableCollection<sbyte> settingValue:
                            return ToStringRepresent(settingValue);
                        case INestableCollection<byte> settingValue:
                            return ToStringRepresent(settingValue);
                        case INestableCollection<short> settingValue:
                            return ToStringRepresent(settingValue);
                        case INestableCollection<ushort> settingValue:
                            return ToStringRepresent(settingValue);
                        case INestableCollection<int> settingValue:
                            return ToStringRepresent(settingValue);
                        case INestableCollection<uint> settingValue:
                            return ToStringRepresent(settingValue);
                        case INestableCollection<long> settingValue:
                            return ToStringRepresent(settingValue);
                        case INestableCollection<ulong> settingValue:
                            return ToStringRepresent(settingValue);
                        case INestableCollection<float> settingValue:
                            return ToStringRepresent(settingValue);
                        case INestableCollection<double> settingValue:
                            return ToStringRepresent(settingValue);
                        case INestableCollection<decimal> settingValue:
                            return ToStringRepresent(settingValue);
                        case INestableCollection<char> settingValue:
                            return ToStringRepresent(settingValue);
                        case INestableCollection<bool> settingValue:
                            return ToStringRepresent(settingValue);
                        case INestableCollection<IntPtr> settingValue:
                            return ToStringRepresent(settingValue);
                        case INestableCollection<UIntPtr> settingValue:
                            return ToStringRepresent(settingValue);
                        default:
                            return value.ToString();
                    }
                case ICollection collectionValue:
                    switch (collectionValue)
                    {
                        case ICollection<string> settingValue:
                            return ToStringRepresent(settingValue);
                        case ICollection<sbyte> settingValue:
                            return ToStringRepresent(settingValue);
                        case ICollection<byte> settingValue:
                            return ToStringRepresent(settingValue);
                        case ICollection<short> settingValue:
                            return ToStringRepresent(settingValue);
                        case ICollection<ushort> settingValue:
                            return ToStringRepresent(settingValue);
                        case ICollection<int> settingValue:
                            return ToStringRepresent(settingValue);
                        case ICollection<uint> settingValue:
                            return ToStringRepresent(settingValue);
                        case ICollection<long> settingValue:
                            return ToStringRepresent(settingValue);
                        case ICollection<ulong> settingValue:
                            return ToStringRepresent(settingValue);
                        case ICollection<float> settingValue:
                            return ToStringRepresent(settingValue);
                        case ICollection<double> settingValue:
                            return ToStringRepresent(settingValue);
                        case ICollection<decimal> settingValue:
                            return ToStringRepresent(settingValue);
                        case ICollection<char> settingValue:
                            return ToStringRepresent(settingValue);
                        case ICollection<bool> settingValue:
                            return ToStringRepresent(settingValue);
                        case ICollection<IntPtr> settingValue:
                            return ToStringRepresent(settingValue);
                        case ICollection<UIntPtr> settingValue:
                            return ToStringRepresent(settingValue);
                        default:
                            return value.ToString();
                    }
                case IEnumerable collectionValue:
                    switch (collectionValue)
                    {
                        case IEnumerable<string> settingValue:
                            return ToStringRepresent(settingValue);
                        case IEnumerable<sbyte> settingValue:
                            return ToStringRepresent(settingValue);
                        case IEnumerable<byte> settingValue:
                            return ToStringRepresent(settingValue);
                        case IEnumerable<short> settingValue:
                            return ToStringRepresent(settingValue);
                        case IEnumerable<ushort> settingValue:
                            return ToStringRepresent(settingValue);
                        case IEnumerable<int> settingValue:
                            return ToStringRepresent(settingValue);
                        case IEnumerable<uint> settingValue:
                            return ToStringRepresent(settingValue);
                        case IEnumerable<long> settingValue:
                            return ToStringRepresent(settingValue);
                        case IEnumerable<ulong> settingValue:
                            return ToStringRepresent(settingValue);
                        case IEnumerable<float> settingValue:
                            return ToStringRepresent(settingValue);
                        case IEnumerable<double> settingValue:
                            return ToStringRepresent(settingValue);
                        case IEnumerable<decimal> settingValue:
                            return ToStringRepresent(settingValue);
                        case IEnumerable<char> settingValue:
                            return ToStringRepresent(settingValue);
                        case IEnumerable<bool> settingValue:
                            return ToStringRepresent(settingValue);
                        case IEnumerable<IntPtr> settingValue:
                            return ToStringRepresent(settingValue);
                        case IEnumerable<UIntPtr> settingValue:
                            return ToStringRepresent(settingValue);
                        default:
                            return value.ToString();
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
                _propertyInfo.SetValue(_settingsBase,
                    Convert.ChangeType(value, Type, CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }

        public void SetValueFromString(string value)
        {
            if (value == null)
            {
                SetValue(default);
            }
            else if (Type == typeof(string))
            {
                SetValue(value);
            }
            else if (typeof(INestableCollection).IsAssignableFrom(Type))
            {
                if (typeof(INestableCollection<string>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<string>(value));
                else if (typeof(INestableCollection<sbyte>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<sbyte>(value));
                else if (typeof(INestableCollection<byte>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<byte>(value));
                else if (typeof(INestableCollection<short>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<short>(value));
                else if (typeof(INestableCollection<ushort>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<ushort>(value));
                else if (typeof(INestableCollection<int>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<int>(value));
                else if (typeof(INestableCollection<uint>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<uint>(value));
                else if (typeof(INestableCollection<long>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<long>(value));
                else if (typeof(INestableCollection<ulong>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<ulong>(value));
                else if (typeof(INestableCollection<float>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<float>(value));
                else if (typeof(INestableCollection<double>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<double>(value));
                else if (typeof(INestableCollection<decimal>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<decimal>(value));
                else if (typeof(INestableCollection<char>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<char>(value));
                else if (typeof(INestableCollection<bool>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<bool>(value));
                else if (typeof(INestableCollection<IntPtr>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<IntPtr>(value));
                else if (typeof(INestableCollection<UIntPtr>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<UIntPtr>(value));
                else
                    SetValue(value);
            }
            else if (typeof(Array).IsAssignableFrom(Type))
            {
                if (typeof(string[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<string>(value));
                else if (typeof(sbyte[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<sbyte>(value));
                else if (typeof(byte[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<byte>(value));
                else if (typeof(short[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<short>(value));
                else if (typeof(ushort[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<ushort>(value));
                else if (typeof(int[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<int>(value));
                else if (typeof(uint[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<uint>(value));
                else if (typeof(long[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<long>(value));
                else if (typeof(ulong[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<ulong>(value));
                else if (typeof(float[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<float>(value));
                else if (typeof(double[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<double>(value));
                else if (typeof(decimal[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<decimal>(value));
                else if (typeof(char[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<char>(value));
                else if (typeof(bool[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<bool>(value));
                else if (typeof(IntPtr[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<IntPtr>(value));
                else if(typeof(UIntPtr[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<UIntPtr>(value));
                else
                    SetValue(value);
            }
            else if (typeof(ICollection).IsAssignableFrom(Type))
            {
                if (typeof(ICollection<string>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<string>(value));
                else if (typeof(ICollection<sbyte>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<sbyte>(value));
                else if (typeof(ICollection<byte>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<byte>(value));
                else if (typeof(ICollection<short>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<short>(value));
                else if (typeof(ICollection<ushort>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<ushort>(value));
                else if (typeof(ICollection<int>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<int>(value));
                else if (typeof(ICollection<uint>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<uint>(value));
                else if (typeof(ICollection<long>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<long>(value));
                else if (typeof(ICollection<ulong>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<ulong>(value));
                else if (typeof(ICollection<float>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<float>(value));
                else if (typeof(ICollection<double>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<double>(value));
                else if (typeof(ICollection<decimal>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<decimal>(value));
                else if (typeof(ICollection<char>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<char>(value));
                else if (typeof(ICollection<bool>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<bool>(value));
                else if (typeof(ICollection<IntPtr>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<IntPtr>(value));
                else if (typeof(ICollection<UIntPtr>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<UIntPtr>(value));
                else
                    SetValue(value);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(Type))
            {
                if (typeof(IEnumerable<string>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<string>(value));
                else if (typeof(IEnumerable<sbyte>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<sbyte>(value));
                else if (typeof(IEnumerable<byte>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<byte>(value));
                else if (typeof(IEnumerable<short>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<short>(value));
                else if (typeof(IEnumerable<ushort>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<ushort>(value));
                else if (typeof(IEnumerable<int>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<int>(value));
                else if (typeof(IEnumerable<uint>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<uint>(value));
                else if (typeof(IEnumerable<long>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<long>(value));
                else if (typeof(IEnumerable<ulong>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<ulong>(value));
                else if (typeof(IEnumerable<float>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<float>(value));
                else if (typeof(IEnumerable<double>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<double>(value));
                else if (typeof(IEnumerable<decimal>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<decimal>(value));
                else if (typeof(IEnumerable<char>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<char>(value));
                else if (typeof(IEnumerable<bool>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<bool>(value));
                else if (typeof(IEnumerable<IntPtr>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<IntPtr>(value));
                else if (typeof(IEnumerable<UIntPtr>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<UIntPtr>(value));
                else
                    SetValue(value);
            }
            else
            {
                SetValue(value);
            }
        }
    }
}
