// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using RIS.Collections.Chunked;
using RIS.Collections.Nestable;
#if NETFRAMEWORK
using RIS.Extensions;
#endif

namespace RIS.Settings
{
    public sealed class Setting
    {
        private const BindingFlags AccessBindingFlags = BindingFlags.Instance
                                                        | BindingFlags.Public
                                                        | BindingFlags.NonPublic;

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
        public object DefaultValue { get; internal set; }

        public Setting(
            SettingsBase settings,
            PropertyInfo propertyInfo,
            string category = null,
            object defaultValue = null)
        {
            _settingsBase = settings;
            _propertyInfo = propertyInfo;

            CategoryName = category;
            DefaultValue = defaultValue;
        }

        private static string ToStringRepresent<T>(INestableCollection<T> collection)
        {
            return collection.ToStringRepresent();
        }
        private static string ToStringRepresent<T>(IChunkedCollection<T> collection)
        {
            var nestableCollection = new NestableArrayCAL<T>();

            foreach (var element in collection)
            {
                nestableCollection.Add(element);
            }

            return nestableCollection.ToStringRepresent();
        }
        private static string ToStringRepresent<T>(IDictionary<string, T> collection)
        {
            var nestableCollection = new NestableDictionaryL<T>();

            foreach (var element in collection)
            {
                nestableCollection.Add(element.Key, element.Value);
            }

            return nestableCollection.ToStringRepresent();
        }
        private static string ToStringRepresent<T>(ICollection<T> collection)
        {
            var nestableCollection = new NestableArrayCAL<T>();

            foreach (var element in collection)
            {
                nestableCollection.Add(element);
            }

            return nestableCollection.ToStringRepresent();
        }
        private static string ToStringRepresent<T>(IEnumerable<T> collection)
        {
            var nestableCollection = new NestableArrayCAL<T>();

            foreach (var element in collection)
            {
                nestableCollection.Add(element);
            }

            return nestableCollection.ToStringRepresent();
        }

        private static INestableCollection<T> FromStringRepresentNestable<T>(string represent)
        {
            return NestableHelper.FromStringRepresent<T>(represent);
        }
        private static IChunkedCollection<T> FromStringRepresentChunked<T>(string represent)
        {
            var nestableCollection = new NestableArrayCAL<T>(
                represent);
            var collection = new ChunkedArrayL<T>(nestableCollection.Length);

            for (int i = 0; i < nestableCollection.Length; ++i)
            {
                collection[i] = nestableCollection[i].GetElement();
            }

            return collection;
        }
        private static IDictionary<string, T> FromStringRepresentDictionary<T>(string represent)
        {
            var nestableCollection = new NestableDictionaryL<T>(
                represent);
            var collection = new Dictionary<string, T>(nestableCollection.Length);

            for (int i = 0; i < nestableCollection.Length; ++i)
            {
                collection.Add(nestableCollection.GetKey(i),
                    nestableCollection[i].GetElement());
            }

            return collection;
        }
        private static ICollection<T> FromStringRepresentArray<T>(string represent)
        {
            var nestableCollection = new NestableArrayCAL<T>(
                represent);
            var collection = new T[nestableCollection.Length];

            for (int i = 0; i < nestableCollection.Length; ++i)
            {
                collection[i] = nestableCollection[i].GetElement();
            }

            return collection;
        }
        private static ICollection<T> FromStringRepresentCollection<T>(string represent)
        {
            var nestableCollection = new NestableArrayCAL<T>(
                represent);
            var collection = new List<T>(nestableCollection.Length);

            for (int i = 0; i < nestableCollection.Length; ++i)
            {
                collection.Add(nestableCollection[i].GetElement());
            }

            return collection;
        }
        private static IEnumerable<T> FromStringRepresentEnumerable<T>(string represent)
        {
            var nestableCollection = new NestableArrayCAL<T>(
                represent);
            var collection = new List<T>(nestableCollection.Length);

            for (int i = 0; i < nestableCollection.Length; ++i)
            {
                collection.Add(nestableCollection[i].GetElement());
            }

            return collection;
        }

        public object GetValue()
        {
            try
            {
                if (!_propertyInfo.CanRead)
                    return null;

                return _propertyInfo.GetValue(_settingsBase,
                    AccessBindingFlags, null, null,
                    CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                throw;
            }
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
                    return settingValue.ToString(
                        CultureInfo.InvariantCulture);
                case double settingValue:
                    return settingValue.ToString(
                        CultureInfo.InvariantCulture);
                case decimal settingValue:
                    return settingValue.ToString(
                        CultureInfo.InvariantCulture);
                case DateTime settingValue:
                    return settingValue.ToString(
                        CultureInfo.InvariantCulture);
                case Enum settingValue:
                    return Enum.Format(
                        settingValue.GetType(),
                        settingValue,
                        "d");
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
                        case INestableCollection<DateTime> settingValue:
                            return ToStringRepresent(settingValue);
                        default:
                            return value.ToString();
                    }
                case IChunkedCollection collectionValue:
                    switch (collectionValue)
                    {
                        case IChunkedCollection<string> settingValue:
                            return ToStringRepresent(settingValue);
                        case IChunkedCollection<sbyte> settingValue:
                            return ToStringRepresent(settingValue);
                        case IChunkedCollection<byte> settingValue:
                            return ToStringRepresent(settingValue);
                        case IChunkedCollection<short> settingValue:
                            return ToStringRepresent(settingValue);
                        case IChunkedCollection<ushort> settingValue:
                            return ToStringRepresent(settingValue);
                        case IChunkedCollection<int> settingValue:
                            return ToStringRepresent(settingValue);
                        case IChunkedCollection<uint> settingValue:
                            return ToStringRepresent(settingValue);
                        case IChunkedCollection<long> settingValue:
                            return ToStringRepresent(settingValue);
                        case IChunkedCollection<ulong> settingValue:
                            return ToStringRepresent(settingValue);
                        case IChunkedCollection<float> settingValue:
                            return ToStringRepresent(settingValue);
                        case IChunkedCollection<double> settingValue:
                            return ToStringRepresent(settingValue);
                        case IChunkedCollection<decimal> settingValue:
                            return ToStringRepresent(settingValue);
                        case IChunkedCollection<char> settingValue:
                            return ToStringRepresent(settingValue);
                        case IChunkedCollection<bool> settingValue:
                            return ToStringRepresent(settingValue);
                        case IChunkedCollection<IntPtr> settingValue:
                            return ToStringRepresent(settingValue);
                        case IChunkedCollection<UIntPtr> settingValue:
                            return ToStringRepresent(settingValue);
                        case IChunkedCollection<DateTime> settingValue:
                            return ToStringRepresent(settingValue);
                        default:
                            return value.ToString();
                    }
                case IDictionary collectionValue:
                    switch (collectionValue)
                    {
                        case IDictionary<string, string> settingValue:
                            return ToStringRepresent(settingValue);
                        case IDictionary<string, sbyte> settingValue:
                            return ToStringRepresent(settingValue);
                        case IDictionary<string, byte> settingValue:
                            return ToStringRepresent(settingValue);
                        case IDictionary<string, short> settingValue:
                            return ToStringRepresent(settingValue);
                        case IDictionary<string, ushort> settingValue:
                            return ToStringRepresent(settingValue);
                        case IDictionary<string, int> settingValue:
                            return ToStringRepresent(settingValue);
                        case IDictionary<string, uint> settingValue:
                            return ToStringRepresent(settingValue);
                        case IDictionary<string, long> settingValue:
                            return ToStringRepresent(settingValue);
                        case IDictionary<string, ulong> settingValue:
                            return ToStringRepresent(settingValue);
                        case IDictionary<string, float> settingValue:
                            return ToStringRepresent(settingValue);
                        case IDictionary<string, double> settingValue:
                            return ToStringRepresent(settingValue);
                        case IDictionary<string, decimal> settingValue:
                            return ToStringRepresent(settingValue);
                        case IDictionary<string, char> settingValue:
                            return ToStringRepresent(settingValue);
                        case IDictionary<string, bool> settingValue:
                            return ToStringRepresent(settingValue);
                        case IDictionary<string, IntPtr> settingValue:
                            return ToStringRepresent(settingValue);
                        case IDictionary<string, UIntPtr> settingValue:
                            return ToStringRepresent(settingValue);
                        case IDictionary<string, DateTime> settingValue:
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
                        case ICollection<DateTime> settingValue:
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
                        case IEnumerable<DateTime> settingValue:
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
            try
            {
                if (!_propertyInfo.CanWrite)
                    return;

                if (Type.IsEnum
                    && !Enum.IsDefined(Type, value))
                {
                    value = DefaultValue;
                }

                _propertyInfo.SetValue(_settingsBase,
                    Convert.ChangeType(value, Type, CultureInfo.InvariantCulture),
                    AccessBindingFlags, null ,null,
                    CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }

#pragma warning disable SS018 // Add cases for missing enum member.
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
            else if (Type == typeof(DateTime))
            {
                SetValue(DateTime.Parse(value,
                    CultureInfo.InvariantCulture));
            }
            else if (Type.IsEnum)
            {
#if NETCOREAPP

                if (Enum.TryParse(Type, value, true, out var enumValue))
                {
                    SetValue(enumValue);
                }
                else
                {
                    SetValue(DefaultValue);
                }

#elif NETFRAMEWORK

                var underlyingType = Type
                    .GetEnumUnderlyingType();

                object integerValue = null;

                switch (Type.GetTypeCode(underlyingType))
                {
                    case TypeCode.SByte:
                        integerValue = value.ToSbyte();
                        break;
                    case TypeCode.Byte:
                        integerValue = value.ToByte();
                        break;
                    case TypeCode.Int16:
                        integerValue = value.ToShort();
                        break;
                    case TypeCode.UInt16:
                        integerValue = value.ToUShort();
                        break;
                    case TypeCode.Int32:
                        integerValue = value.ToInt();
                        break;
                    case TypeCode.UInt32:
                        integerValue = value.ToUInt();
                        break;
                    case TypeCode.Int64:
                        integerValue = value.ToLong();
                        break;
                    case TypeCode.UInt64:
                        integerValue = value.ToULong();
                        break;
                    default:
                        break;
                }

                if (integerValue != null
                    && Enum.IsDefined(Type, integerValue))
                {
                    SetValue(Enum.Parse(Type, value));
                }
                else
                {
                    SetValue(DefaultValue);
                }

#endif
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
                else if (typeof(INestableCollection<DateTime>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentNestable<DateTime>(value));
                else
                    SetValue(value);
            }
            else if (typeof(IChunkedCollection).IsAssignableFrom(Type))
            {
                if (typeof(IChunkedCollection<string>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<string>(value));
                else if (typeof(IChunkedCollection<sbyte>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<sbyte>(value));
                else if (typeof(IChunkedCollection<byte>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<byte>(value));
                else if (typeof(IChunkedCollection<short>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<short>(value));
                else if (typeof(IChunkedCollection<ushort>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<ushort>(value));
                else if (typeof(IChunkedCollection<int>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<int>(value));
                else if (typeof(IChunkedCollection<uint>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<uint>(value));
                else if (typeof(IChunkedCollection<long>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<long>(value));
                else if (typeof(IChunkedCollection<ulong>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<ulong>(value));
                else if (typeof(IChunkedCollection<float>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<float>(value));
                else if (typeof(IChunkedCollection<double>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<double>(value));
                else if (typeof(IChunkedCollection<decimal>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<decimal>(value));
                else if (typeof(IChunkedCollection<char>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<char>(value));
                else if (typeof(IChunkedCollection<bool>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<bool>(value));
                else if (typeof(IChunkedCollection<IntPtr>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<IntPtr>(value));
                else if (typeof(IChunkedCollection<UIntPtr>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<UIntPtr>(value));
                else if (typeof(IChunkedCollection<DateTime>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentChunked<DateTime>(value));
                else
                    SetValue(value);
            }
            else if (typeof(IDictionary).IsAssignableFrom(Type))
            {
                if (typeof(IDictionary<string, string>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<string>(value));
                else if (typeof(IDictionary<string, sbyte>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<sbyte>(value));
                else if (typeof(IDictionary<string, byte>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<byte>(value));
                else if (typeof(IDictionary<string, short>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<short>(value));
                else if (typeof(IDictionary<string, ushort>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<ushort>(value));
                else if (typeof(IDictionary<string, int>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<int>(value));
                else if (typeof(IDictionary<string, uint>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<uint>(value));
                else if (typeof(IDictionary<string, long>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<long>(value));
                else if (typeof(IDictionary<string, ulong>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<ulong>(value));
                else if (typeof(IDictionary<string, float>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<float>(value));
                else if (typeof(IDictionary<string, double>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<double>(value));
                else if (typeof(IDictionary<string, decimal>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<decimal>(value));
                else if (typeof(IDictionary<string, char>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<char>(value));
                else if (typeof(IDictionary<string, bool>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<bool>(value));
                else if (typeof(IDictionary<string, IntPtr>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<IntPtr>(value));
                else if (typeof(IDictionary<string, UIntPtr>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<UIntPtr>(value));
                else if (typeof(IDictionary<string, DateTime>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentDictionary<DateTime>(value));
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
                else if (typeof(DateTime[]).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentArray<DateTime>(value));
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
                else if (typeof(ICollection<DateTime>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentCollection<DateTime>(value));
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
                else if (typeof(IEnumerable<DateTime>).IsAssignableFrom(Type))
                    SetValue(FromStringRepresentEnumerable<DateTime>(value));
                else
                    SetValue(value);
            }
            else
            {
                SetValue(value);
            }
        }
#pragma warning restore SS018 // Add cases for missing enum member.
    }
}
