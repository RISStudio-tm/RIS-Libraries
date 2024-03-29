﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using RIS.Extensions;

namespace RIS.Collections.Nestable.Serialization
{
    public static class NestableSerializer
    {
        public static event EventHandler<RInformationEventArgs> Information;
        public static event EventHandler<RWarningEventArgs> Warning;
        public static event EventHandler<RErrorEventArgs> Error;

        public static void OnInformation(RInformationEventArgs e)
        {
            OnInformation(null, e);
        }
        public static void OnInformation(object sender, RInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        public static void OnWarning(RWarningEventArgs e)
        {
            OnWarning(null, e);
        }
        public static void OnWarning(object sender, RWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        public static void OnError(RErrorEventArgs e)
        {
            OnError(null, e);
        }
        public static void OnError(object sender, RErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }

        private static INestableDictionary<string> CreateCollection(
            string represent)
        {
            var collection = NestableHelper
                .FromStringRepresent<string>(represent);

            if (NestableHelper.GetGeneralType(collection) != CollectionGeneralType.Dictionary)
            {
                var exception =
                    new Exception("Invalid general type for the resulting collection");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return (INestableDictionary<string>)collection;
        }
        private static INestableDictionary<string> CreateCollection(
            NestableSerializerSettings settings, string key = null)
        {
            var collectionInfo =
                NestableHelper.CreateCollectionByType<string>(
                    settings.GetCollectionType());

            if (collectionInfo.GeneralType != CollectionGeneralType.Dictionary)
            {
                var exception =
                    new Exception("Invalid general type for the resulting collection");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            var collection = (INestableDictionary<string>)collectionInfo.Collection;

            if (!string.IsNullOrEmpty(key))
                collection.Key = key;

            return collection;
        }

        public static string Serialize(object value,
            NestableSerializerSettings settings = null)
        {
            return SerializeInternal(value, settings)
                .ToStringRepresent();
        }
        private static INestableDictionary<string> SerializeInternal(
            object value, NestableSerializerSettings settings)
        {
            if (value == null)
            {
                var exception = new ArgumentNullException(
                    nameof(value),
                    $"{nameof(value)} cannot be null");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (settings == null)
                settings = NestableSerializerSettings.Default;

            var type = value.GetType();
            var name = type.Name;

            if (Attribute.IsDefined(type, typeof(NestableSerializedAttribute)))
            {
                name = ((NestableSerializedAttribute)type
                    .GetCustomAttribute(typeof(NestableSerializedAttribute)))?.Name;

                if (string.IsNullOrWhiteSpace(name))
                    name = type.Name;
            }

            var collection =
                CreateCollection(settings, name);

            collection.Add("FullType", type.GetAssemblyQualifiedName());

            if (settings.Targets == NestableSerializerTargets.None)
                return collection;

            if (settings.HasTarget(NestableSerializerTargets.PublicFields)
                || settings.HasTarget(NestableSerializerTargets.NonPublicFields))
            {
                var bindingFlags = BindingFlags.Instance;

                if (settings.HasTarget(NestableSerializerTargets.PublicFields))
                    bindingFlags |= BindingFlags.Public;
                if (settings.HasTarget(NestableSerializerTargets.NonPublicFields))
                    bindingFlags |= BindingFlags.NonPublic;

                var fieldsCollection =
                    CreateCollection(settings, "Fields");

                foreach (var field in type.GetFields(bindingFlags))
                {
                    if (settings.SkipReadOnlyFields
                        && (field.IsInitOnly
                        || field.IsLiteral))
                    {
                        continue;
                    }

                    if (field.IsNotSerialized
                        || Attribute.IsDefined(field, typeof(NonNestableSerializedAttribute)))
                    {
                        continue;
                    }

                    var fieldName = field.Name;

                    if (Attribute.IsDefined(field, typeof(NestableSerializedAttribute)))
                    {
                        fieldName = ((NestableSerializedAttribute)field
                            .GetCustomAttribute(typeof(NestableSerializedAttribute)))?.Name;

                        if (string.IsNullOrWhiteSpace(fieldName))
                            fieldName = field.Name;
                    }
                    else
                    {
                        if (settings.FieldsWithAttributeOnly)
                            continue;
                    }

                    object fieldValue;

                    try
                    {
                        fieldValue = field.GetValue(value);
                    }
                    catch (NotSupportedException)
                    {
                        continue;
                    }

                    SerializeValueInternal(fieldsCollection,
                        fieldName, field.FieldType,
                        fieldValue, settings);
                }

                if (fieldsCollection.Length > 0)
                    collection.Add("Fields", fieldsCollection);
            }

            if (settings.HasTarget(NestableSerializerTargets.PublicProperties)
                || settings.HasTarget(NestableSerializerTargets.NonPublicProperties))
            {
                var bindingFlags = BindingFlags.Instance;

                if (settings.HasTarget(NestableSerializerTargets.PublicProperties))
                    bindingFlags |= BindingFlags.Public;
                if (settings.HasTarget(NestableSerializerTargets.NonPublicProperties))
                    bindingFlags |= BindingFlags.NonPublic;

                var propertiesCollection =
                    CreateCollection(settings, "Properties");

                foreach (var property in type.GetProperties(bindingFlags))
                {
                    if (settings.SkipReadOnlyProperties
                        && (!property.CanWrite))
                    {
                        continue;
                    }

                    if (!property.CanRead
                        || Attribute.IsDefined(property, typeof(NonNestableSerializedAttribute)))
                    {
                        continue;
                    }

                    var propertyName = property.Name;

                    if (Attribute.IsDefined(property, typeof(NestableSerializedAttribute)))
                    {
                        propertyName = ((NestableSerializedAttribute)property
                            .GetCustomAttribute(typeof(NestableSerializedAttribute)))?.Name;

                        if (string.IsNullOrWhiteSpace(propertyName))
                            propertyName = property.Name;
                    }
                    else
                    {
                        if (settings.PropertiesWithAttributeOnly)
                            continue;
                    }

                    object propertyValue;

                    try
                    {
                        propertyValue = property.GetValue(value, bindingFlags,
                            null, null, CultureInfo.InvariantCulture);
                    }
                    catch (NotSupportedException)
                    {
                        continue;
                    }
                    catch (TargetParameterCountException)
                    {
                        continue;
                    }

                    SerializeValueInternal(propertiesCollection,
                        propertyName, property.PropertyType,
                        propertyValue, settings);
                }

                if (propertiesCollection.Length > 0)
                    collection.Add("Properties", propertiesCollection);
            }

            Type elementType = null;

            if (typeof(IEnumerable).IsAssignableFrom(type)
                && !typeof(Array).IsAssignableFrom(type)
                && !typeof(Enum).IsAssignableFrom(type))
            {
                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.IsGenericType &&
                        interfaceType.GetGenericTypeDefinition() != typeof(ICollection<>))
                    {
                        continue;
                    }

                    elementType = interfaceType.GetGenericArguments()[0];
                    break;
                }
            }

            if (elementType != null)
            {
                var elementsCollection =
                    CreateCollection(settings, "Elements");

                ulong elementIndex = 0;

                foreach (var element in (IEnumerable)value)
                {
                    SerializeValueInternal(elementsCollection,
                        elementIndex.ToString(), elementType,
                        element, settings);

                    ++elementIndex;
                }

                collection.Add("ElementType", elementType.GetAssemblyQualifiedName());
                collection.Add("Length", elementIndex.ToString());
                collection.Add("Elements", elementsCollection);
            }

            return collection;
        }
        private static void SerializeValueInternal(
            INestableDictionary<string> collection,
            string key, Type containerType, object value,
            NestableSerializerSettings settings)
        {
            switch (value)
            {
                case null:
                    collection.Add(
                        key,
                        (string)null);

                    break;
                case DBNull _:
                    collection.Add(
                        key,
                        "db_null");

                    break;
                case sbyte _:
                case byte _:
                case short _:
                case ushort _:
                case int _:
                case uint _:
                case long _:
                case ulong _:
                case float _:
                case double _:
                case decimal _:
                case bool _:
                case char _:
                case string _:
                case DateTime _:
                    if (containerType == typeof(object)
                        && value.GetType() != typeof(object))
                    {
                        collection.Add(
                            key,
                            value.GetType().GetAssemblyQualifiedName() +
                            '|' +
                            Convert.ToString(value, CultureInfo.InvariantCulture));

                        break;
                    }

                    collection.Add(
                        key,
                        Convert.ToString(value, CultureInfo.InvariantCulture));

                    break;
                case IntPtr _:
                case UIntPtr _:
                    break;
                case Enum _:
                    if (containerType == typeof(object)
                        && value.GetType() != typeof(object))
                    {
                        collection.Add(
                            key,
                            value.GetType().GetAssemblyQualifiedName() +
                            '|' +
                            Convert.ToString(
                                Convert.ToUInt64(value,
                                    CultureInfo.InvariantCulture),
                                CultureInfo.InvariantCulture));

                        break;
                    }

                    collection.Add(
                        key,
                        Convert.ToString(
                            Convert.ToUInt64(value,
                                CultureInfo.InvariantCulture),
                            CultureInfo.InvariantCulture));

                    break;
                case object[][] array:
                    var arrayCollection11 =
                        CreateCollection(settings, key);

                    arrayCollection11.Add("Lengths", $"{array.GetLength(0)}");

                    var elementType11 = array.GetType().GetElementType();

                    arrayCollection11.Add("Type", elementType11?.GetAssemblyQualifiedName());

                    for (var i = 0; i < array.Length; ++i)
                    {
                        var elementKey = $"{i}";
                        var element = array[i];

                        SerializeValueInternal(arrayCollection11,
                            elementKey, elementType11,
                            element, settings);
                    }

                    collection.Add(
                        key,
                        arrayCollection11);

                    break;
                case object[][,] array:
                    var arrayCollection12 =
                        CreateCollection(settings, key);

                    arrayCollection12.Add("Lengths", $"{array.GetLength(0)}");

                    var elementType12 = array.GetType().GetElementType();

                    arrayCollection12.Add("Type", elementType12.GetAssemblyQualifiedName());

                    for (var i = 0; i < array.Length; ++i)
                    {
                        var elementKey = $"{i}";
                        var element = array[i];

                        SerializeValueInternal(arrayCollection12,
                            elementKey, elementType12,
                            element, settings);
                    }

                    collection.Add(
                        key,
                        arrayCollection12);

                    break;
                case object[][,,] array:
                    var arrayCollection13 =
                        CreateCollection(settings, key);

                    arrayCollection13.Add("Lengths", $"{array.GetLength(0)}");

                    var elementType13 = array.GetType().GetElementType();

                    arrayCollection13.Add("Type", elementType13.GetAssemblyQualifiedName());

                    for (var i = 0; i < array.Length; ++i)
                    {
                        var elementKey = $"{i}";
                        var element = array[i];

                        SerializeValueInternal(arrayCollection13,
                            elementKey, elementType13,
                            element, settings);
                    }

                    collection.Add(
                        key,
                        arrayCollection13);

                    break;
                case object[,][] array:
                    var arrayCollection21 =
                        CreateCollection(settings, key);

                    arrayCollection21.Add("Lengths", $"{array.GetLength(0)}" +
                                                     $",{array.GetLength(1)}");

                    var elementType21 = array.GetType().GetElementType();

                    arrayCollection21.Add("Type", elementType21.GetAssemblyQualifiedName());

                    for (var i = 0; i < array.Length; ++i)
                    {
                        for (var j = 0; j < array.Length; ++j)
                        {
                            var elementKey = $"{i},{j}";
                            var element = array[i, j];

                            SerializeValueInternal(arrayCollection21,
                                elementKey, elementType21,
                                element, settings);
                        }
                    }

                    collection.Add(
                        key,
                        arrayCollection21);

                    break;
                case object[,][,] array:
                    var arrayCollection22 =
                        CreateCollection(settings, key);

                    arrayCollection22.Add("Lengths", $"{array.GetLength(0)}" +
                                                     $",{array.GetLength(1)}");

                    var elementType22 = array.GetType().GetElementType();

                    arrayCollection22.Add("Type", elementType22.GetAssemblyQualifiedName());

                    for (var i = 0; i < array.Length; ++i)
                    {
                        for (var j = 0; j < array.Length; ++j)
                        {
                            var elementKey = $"{i},{j}";
                            var element = array[i, j];

                            SerializeValueInternal(arrayCollection22,
                                elementKey, elementType22,
                                element, settings);
                        }
                    }

                    collection.Add(
                        key,
                        arrayCollection22);

                    break;
                case object[,][,,] array:
                    var arrayCollection23 =
                        CreateCollection(settings, key);

                    arrayCollection23.Add("Lengths", $"{array.GetLength(0)}" +
                                                     $",{array.GetLength(1)}");

                    var elementType23 = array.GetType().GetElementType();

                    arrayCollection23.Add("Type", elementType23.GetAssemblyQualifiedName());

                    for (var i = 0; i < array.Length; ++i)
                    {
                        for (var j = 0; j < array.Length; ++j)
                        {
                            var elementKey = $"{i},{j}";
                            var element = array[i, j];

                            SerializeValueInternal(arrayCollection23,
                                elementKey, elementType23,
                                element, settings);
                        }
                    }

                    collection.Add(
                        key,
                        arrayCollection23);

                    break;
                case object[,,][] array:
                    var arrayCollection31 =
                        CreateCollection(settings, key);

                    arrayCollection31.Add("Lengths", $"{array.GetLength(0)}" +
                                                     $",{array.GetLength(1)}" +
                                                     $",{array.GetLength(2)}");

                    var elementType31 = array.GetType().GetElementType();

                    arrayCollection31.Add("Type", elementType31.GetAssemblyQualifiedName());

                    for (var i = 0; i < array.Length; ++i)
                    {
                        for (var j = 0; j < array.Length; ++j)
                        {
                            for (var k = 0; k < array.Length; ++k)
                            {
                                var elementKey = $"{i},{j},{k}";
                                var element = array[i, j, k];

                                SerializeValueInternal(arrayCollection31,
                                    elementKey, elementType31,
                                    element, settings);
                            }
                        }
                    }

                    collection.Add(
                        key,
                        arrayCollection31);

                    break;
                case object[,,][,] array:
                    var arrayCollection32 =
                        CreateCollection(settings, key);

                    arrayCollection32.Add("Lengths", $"{array.GetLength(0)}" +
                                                     $",{array.GetLength(1)}" +
                                                     $",{array.GetLength(2)}");

                    var elementType32 = array.GetType().GetElementType();

                    arrayCollection32.Add("Type", elementType32.GetAssemblyQualifiedName());

                    for (var i = 0; i < array.Length; ++i)
                    {
                        for (var j = 0; j < array.Length; ++j)
                        {
                            for (var k = 0; k < array.Length; ++k)
                            {
                                var elementKey = $"{i},{j},{k}";
                                var element = array[i, j, k];

                                SerializeValueInternal(arrayCollection32,
                                    elementKey, elementType32,
                                    element, settings);
                            }
                        }
                    }

                    collection.Add(
                        key,
                        arrayCollection32);

                    break;
                case object[,,][,,] array:
                    var arrayCollection33 =
                        CreateCollection(settings, key);

                    arrayCollection33.Add("Lengths", $"{array.GetLength(0)}" +
                                                     $",{array.GetLength(1)}" +
                                                     $",{array.GetLength(2)}");

                    var elementType33 = array.GetType().GetElementType();

                    arrayCollection33.Add("Type", elementType33.GetAssemblyQualifiedName());

                    for (var i = 0; i < array.Length; ++i)
                    {
                        for (var j = 0; j < array.Length; ++j)
                        {
                            for (var k = 0; k < array.Length; ++k)
                            {
                                var elementKey = $"{i},{j},{k}";
                                var element = array[i, j, k];

                                SerializeValueInternal(arrayCollection33,
                                    elementKey, elementType33,
                                    element, settings);
                            }
                        }
                    }

                    collection.Add(
                        key,
                        arrayCollection33);

                    break;
                case object[] array:
                    var arrayCollection1 =
                        CreateCollection(settings, key);

                    arrayCollection1.Add("Lengths", $"{array.GetLength(0)}");

                    var elementType1 = array.GetType().GetElementType();

                    arrayCollection1.Add("Type", elementType1.GetAssemblyQualifiedName());

                    for (var i = 0; i < array.Length; ++i)
                    {
                        var elementKey = $"{i}";
                        var element = array[i];

                        SerializeValueInternal(arrayCollection1,
                            elementKey, elementType1,
                            element, settings);
                    }

                    collection.Add(
                        key,
                        arrayCollection1);

                    break;
                case object[,] array:
                    var arrayCollection2 =
                        CreateCollection(settings, key);

                    arrayCollection2.Add("Lengths", $"{array.GetLength(0)}" +
                                                     $",{array.GetLength(1)}");

                    var elementType2 = array.GetType().GetElementType();

                    arrayCollection2.Add("Type", elementType2.GetAssemblyQualifiedName());

                    for (var i = 0; i < array.Length; ++i)
                    {
                        for (var j = 0; j < array.Length; ++j)
                        {
                            var elementKey = $"{i},{j}";
                            var element = array[i, j];

                            SerializeValueInternal(arrayCollection2,
                                elementKey, elementType2,
                                element, settings);
                        }
                    }

                    collection.Add(
                        key,
                        arrayCollection2);

                    break;
                case object[,,] array:
                    var arrayCollection3 =
                        CreateCollection(settings, key);

                    arrayCollection3.Add("Lengths", $"{array.GetLength(0)}" +
                                                     $",{array.GetLength(1)}" +
                                                     $",{array.GetLength(2)}");
                    
                    var elementType3 = array.GetType().GetElementType();

                    arrayCollection3.Add("Type", elementType3.GetAssemblyQualifiedName());

                    for (var i = 0; i < array.Length; ++i)
                    {
                        for (var j = 0; j < array.Length; ++j)
                        {
                            for (var k = 0; k < array.Length; ++k)
                            {
                                var elementKey = $"{i},{j},{k}";
                                var element = array[i, j, k];

                                SerializeValueInternal(arrayCollection3,
                                    elementKey, elementType3,
                                    element, settings);
                            }
                        }
                    }

                    collection.Add(
                        key,
                        arrayCollection3);

                    break;
                default:
                    collection.Add(
                        key,
                        SerializeInternal(value, settings));

                    break;
            }
        }

        public static T Deserialize<T>(string represent,
            NestableDeserializerSettings settings = null)
            where T : new()
        {
            var value = new T();

            return Deserialize(
                    value,
                    represent,
                    settings);
        }
        public static T Deserialize<T>(T target, string represent,
            NestableDeserializerSettings settings = null)
        {
            if (string.IsNullOrEmpty(represent))
            {
                var exception =
                    new ArgumentNullException(nameof(represent), $"{nameof(represent)} cannot be null or empty");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return (T)DeserializeInternal(
                target,
                CreateCollection(represent),
                settings);
        }
        private static object DeserializeInternal(
            Type type,
            INestableDictionary<string> collection,
            NestableDeserializerSettings settings)
        {
            var target = Activator.CreateInstance(type);

            return DeserializeInternal(
                target,
                collection,
                settings);
        }
        private static object DeserializeInternal(
            object target,
            INestableDictionary<string> collection,
            NestableDeserializerSettings settings)
        {
            if (target == null)
                return null;

            if (settings == null)
                settings = NestableDeserializerSettings.Default;

            var type = target.GetType();

            if (settings.TypeNameCheck)
            {
                var resultingType = type.GetAssemblyQualifiedName();
                var serializedType = collection["FullType"]
                    .GetElement();

                if (serializedType != resultingType)
                {
                    var exception =
                        new Exception($"Name of the serialized type[{serializedType}] and the resulting type[{resultingType}] do not match");
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }
            }

            if (collection.Length == 0)
                return target;

            const BindingFlags bindingFlags = BindingFlags.Instance
                                              | BindingFlags.Public
                                              | BindingFlags.NonPublic;

            if (collection.ContainsKey("Fields"))
            {
                var fieldsCollection = (INestableDictionary<string>)
                    collection["Fields"].GetCollection();

                foreach (var field in type.GetFields(bindingFlags))
                {
                    if (field.IsInitOnly
                        || field.IsLiteral
                        || field.IsNotSerialized
                        || Attribute.IsDefined(field, typeof(NonNestableSerializedAttribute)))
                    {
                        continue;
                    }

                    var fieldName = field.Name;

                    if (Attribute.IsDefined(field, typeof(NestableSerializedAttribute)))
                    {
                        fieldName = ((NestableSerializedAttribute)field
                            .GetCustomAttribute(typeof(NestableSerializedAttribute)))?.Name;

                        if (string.IsNullOrWhiteSpace(fieldName))
                            fieldName = field.Name;
                    }

                    if (!fieldsCollection.ContainsKey(fieldName))
                        continue;

                    object fieldValue = DeserializeValueInternal(
                        fieldsCollection, fieldName,
                        field.FieldType, bindingFlags, settings,
                        out var success);

                    if (!success)
                        continue;

                    try
                    {
                        field.SetValue(target, fieldValue, bindingFlags,
                            null, CultureInfo.InvariantCulture);
                    }
                    catch (NotSupportedException)
                    {
                        continue;
                    }
                }
            }

            if (collection.ContainsKey("Properties"))
            {
                var propertiesCollection = (INestableDictionary<string>)
                    collection["Properties"].GetCollection();

                foreach (var property in type.GetProperties(bindingFlags))
                {
                    if (!property.CanRead
                        || !property.CanWrite
                        || Attribute.IsDefined(property, typeof(NonNestableSerializedAttribute)))
                    {
                        continue;
                    }

                    var propertyName = property.Name;

                    if (Attribute.IsDefined(property, typeof(NestableSerializedAttribute)))
                    {
                        propertyName = ((NestableSerializedAttribute)property
                            .GetCustomAttribute(typeof(NestableSerializedAttribute)))?.Name;

                        if (string.IsNullOrWhiteSpace(propertyName))
                            propertyName = property.Name;
                    }

                    if (!propertiesCollection.ContainsKey(propertyName))
                        continue;

                    object propertyValue = DeserializeValueInternal(
                        propertiesCollection, propertyName,
                        property.PropertyType, bindingFlags, settings,
                        out var success);

                    if (!success)
                        continue;

                    try
                    {
                        property.SetValue(target, propertyValue, bindingFlags,
                            null, null, CultureInfo.InvariantCulture);
                    }
                    catch (NotSupportedException)
                    {
                        continue;
                    }
                }
            }

            if (collection.ContainsKey("Elements"))
            {
                var length = Convert.ToUInt64(
                    collection["Length"].GetElement(),
                    CultureInfo.InvariantCulture);
                var elementType = Type.GetType(
                    collection["ElementType"].GetElement());
                var elementsCollection = (INestableDictionary<string>)
                    collection["Elements"].GetCollection();

                var addMethod = type
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(method => method.Name == "Add"
                                              && method.GetParameters().Length == 1);

                if (addMethod != null)
                {
                    ulong elementIndex = 0;

                    while (elementIndex < length)
                    {
                        object elementValue = DeserializeValueInternal(
                            elementsCollection, elementIndex.ToString(),
                            elementType, bindingFlags, settings,
                            out var success);

                        if (!success)
                        {
                            ++elementIndex;

                            continue;
                        }

                        addMethod.Invoke(target,
                            new[] {elementValue});

                        ++elementIndex;
                    }
                }
            }

            return target;
        }
        private static object DeserializeValueInternal(
            INestableDictionary<string> collection,
            string key, Type type,
            BindingFlags bindingFlags,
            NestableDeserializerSettings settings,
            out bool success)
        {
            success = true;

            ref var collectionElement = ref collection.GetRef(key);

            if (collectionElement.Value == null
                || collectionElement.Type == NestedType.Unknown)
            {
                if (!type.IsValueType)
                    return null;

                return Activator.CreateInstance(type, bindingFlags,
                    null, null, CultureInfo.InvariantCulture);
            }

            if (typeof(DBNull).IsAssignableFrom(type))
            {
                return DBNull.Value;
            }
            else if (typeof(sbyte).IsAssignableFrom(type)
                     || typeof(byte).IsAssignableFrom(type)
                     || typeof(short).IsAssignableFrom(type)
                     || typeof(ushort).IsAssignableFrom(type)
                     || typeof(int).IsAssignableFrom(type)
                     || typeof(uint).IsAssignableFrom(type)
                     || typeof(long).IsAssignableFrom(type)
                     || typeof(ulong).IsAssignableFrom(type)
                     || typeof(float).IsAssignableFrom(type)
                     || typeof(double).IsAssignableFrom(type)
                     || typeof(decimal).IsAssignableFrom(type)
                     || typeof(bool).IsAssignableFrom(type)
                     || typeof(char).IsAssignableFrom(type)
                     || typeof(string).IsAssignableFrom(type)
                     || typeof(DateTime).IsAssignableFrom(type))
            {
                return Convert.ChangeType(
                    collectionElement.GetElement(),
                    type, CultureInfo.InvariantCulture);
            }
            else if (typeof(IntPtr).IsAssignableFrom(type)
                     || typeof(UIntPtr).IsAssignableFrom(type))
            {
                success = false;
                return null;
            }
            else if (typeof(Enum).IsAssignableFrom(type))
            {
                var enumName = Enum.GetName(type, Convert.ToUInt64(
                    collectionElement.GetElement(),
                    CultureInfo.InvariantCulture));

                if (string.IsNullOrEmpty(enumName))
                {
                    success = false;
                    return null;
                }

                return Enum.Parse(type, enumName);
            }
            else if (typeof(object[][]).IsAssignableFrom(type))
            {
                var arrayCollection = (INestableDictionary<string>)
                    collectionElement.GetCollection();

                var lengths = arrayCollection["Lengths"]
                    .GetElement()
                    .Split(',')
                    .Select(element =>
                    {
                        return Convert.ToInt32(
                            element,
                            CultureInfo.InvariantCulture);
                    })
                    .ToArray();

                var result = new object[lengths[0]][];

                for (var i = 0; i < lengths[0]; ++i)
                {
                    var elementKey = $"{i}";

                    result[i] = (object[])
                        DeserializeValueInternal(arrayCollection,
                            elementKey, typeof(object[]),
                            bindingFlags, settings, out success);

                    if (!success)
                        return null;
                }

                return result;
            }
            else if (typeof(object[][,]).IsAssignableFrom(type))
            {
                var arrayCollection = (INestableDictionary<string>)
                    collectionElement.GetCollection();

                var lengths = arrayCollection["Lengths"]
                    .GetElement()
                    .Split(',')
                    .Select(element =>
                    {
                        return Convert.ToInt32(
                            element,
                            CultureInfo.InvariantCulture);
                    })
                    .ToArray();

                var result = new object[lengths[0]][,];

                for (var i = 0; i < lengths[0]; ++i)
                {
                    var elementKey = $"{i}";

                    result[i] = (object[,])
                        DeserializeValueInternal(arrayCollection,
                            elementKey, typeof(object[,]),
                            bindingFlags, settings, out success);

                    if (!success)
                        return null;
                }

                return result;
            }
            else if (typeof(object[][,,]).IsAssignableFrom(type))
            {
                var arrayCollection = (INestableDictionary<string>)
                    collectionElement.GetCollection();

                var lengths = arrayCollection["Lengths"]
                    .GetElement()
                    .Split(',')
                    .Select(element =>
                    {
                        return Convert.ToInt32(
                            element,
                            CultureInfo.InvariantCulture);
                    })
                    .ToArray();

                var result = new object[lengths[0]][,,];

                for (var i = 0; i < lengths[0]; ++i)
                {
                    var elementKey = $"{i}";

                    result[i] = (object[,,])
                        DeserializeValueInternal(arrayCollection,
                            elementKey, typeof(object[,,]),
                            bindingFlags, settings, out success);

                    if (!success)
                        return null;
                }

                return result;
            }
            else if (typeof(object[,][]).IsAssignableFrom(type))
            {
                var arrayCollection = (INestableDictionary<string>)
                    collectionElement.GetCollection();

                var lengths = arrayCollection["Lengths"]
                    .GetElement()
                    .Split(',')
                    .Select(element =>
                    {
                        return Convert.ToInt32(
                            element,
                            CultureInfo.InvariantCulture);
                    })
                    .ToArray();

                var result = new object[lengths[0], lengths[1]][];

                for (var i = 0; i < lengths[0]; ++i)
                {
                    for (var j = 0; j < lengths[1]; ++j)
                    {
                        var elementKey = $"{i},{j}";

                        result[i, j] = (object[])
                            DeserializeValueInternal(arrayCollection,
                                elementKey, typeof(object[]),
                                bindingFlags, settings, out success);

                        if (!success)
                            return null;
                    }
                }

                return result;
            }
            else if (typeof(object[,][,]).IsAssignableFrom(type))
            {
                var arrayCollection = (INestableDictionary<string>)
                    collectionElement.GetCollection();

                var lengths = arrayCollection["Lengths"]
                    .GetElement()
                    .Split(',')
                    .Select(element =>
                    {
                        return Convert.ToInt32(
                            element,
                            CultureInfo.InvariantCulture);
                    })
                    .ToArray();

                var result = new object[lengths[0], lengths[1]][,];

                for (var i = 0; i < lengths[0]; ++i)
                {
                    for (var j = 0; j < lengths[1]; ++j)
                    {
                        var elementKey = $"{i},{j}";

                        result[i, j] = (object[,])
                            DeserializeValueInternal(arrayCollection,
                                elementKey, typeof(object[,]),
                                bindingFlags, settings, out success);

                        if (!success)
                            return null;
                    }
                }

                return result;
            }
            else if (typeof(object[,][,,]).IsAssignableFrom(type))
            {
                var arrayCollection = (INestableDictionary<string>)
                    collectionElement.GetCollection();

                var lengths = arrayCollection["Lengths"]
                    .GetElement()
                    .Split(',')
                    .Select(element =>
                    {
                        return Convert.ToInt32(
                            element,
                            CultureInfo.InvariantCulture);
                    })
                    .ToArray();

                var result = new object[lengths[0], lengths[1]][,,];

                for (var i = 0; i < lengths[0]; ++i)
                {
                    for (var j = 0; j < lengths[1]; ++j)
                    {
                        var elementKey = $"{i},{j}";

                        result[i, j] = (object[,,])
                            DeserializeValueInternal(arrayCollection,
                                elementKey, typeof(object[,,]),
                                bindingFlags, settings, out success);

                        if (!success)
                            return null;
                    }
                }

                return result;
            }
            else if (typeof(object[,,][]).IsAssignableFrom(type))
            {
                var arrayCollection = (INestableDictionary<string>)
                    collectionElement.GetCollection();

                var lengths = arrayCollection["Lengths"]
                    .GetElement()
                    .Split(',')
                    .Select(element =>
                    {
                        return Convert.ToInt32(
                            element,
                            CultureInfo.InvariantCulture);
                    })
                    .ToArray();

                var result = new object[lengths[0], lengths[1], lengths[2]][];

                for (var i = 0; i < lengths[0]; ++i)
                {
                    for (var j = 0; j < lengths[1]; ++j)
                    {
                        for (var k = 0; k < lengths[2]; ++k)
                        {
                            var elementKey = $"{i},{j},{k}";

                            result[i, j, k] = (object[])
                                DeserializeValueInternal(arrayCollection,
                                    elementKey, typeof(object[]),
                                    bindingFlags, settings, out success);

                            if (!success)
                                return null;
                        }
                    }
                }

                return result;
            }
            else if (typeof(object[,,][,]).IsAssignableFrom(type))
            {
                var arrayCollection = (INestableDictionary<string>)
                    collectionElement.GetCollection();

                var lengths = arrayCollection["Lengths"]
                    .GetElement()
                    .Split(',')
                    .Select(element =>
                    {
                        return Convert.ToInt32(
                            element,
                            CultureInfo.InvariantCulture);
                    })
                    .ToArray();

                var result = new object[lengths[0], lengths[1], lengths[2]][,];

                for (var i = 0; i < lengths[0]; ++i)
                {
                    for (var j = 0; j < lengths[1]; ++j)
                    {
                        for (var k = 0; k < lengths[2]; ++k)
                        {
                            var elementKey = $"{i},{j},{k}";

                            result[i, j, k] = (object[,])
                                DeserializeValueInternal(arrayCollection,
                                    elementKey, typeof(object[,]),
                                    bindingFlags, settings, out success);

                            if (!success)
                                return null;
                        }
                    }
                }

                return result;
            }
            else if (typeof(object[,,][,,]).IsAssignableFrom(type))
            {
                var arrayCollection = (INestableDictionary<string>)
                    collectionElement.GetCollection();

                var lengths = arrayCollection["Lengths"]
                    .GetElement()
                    .Split(',')
                    .Select(element =>
                    {
                        return Convert.ToInt32(
                            element,
                            CultureInfo.InvariantCulture);
                    })
                    .ToArray();

                var result = new object[lengths[0], lengths[1], lengths[2]][,,];

                for (var i = 0; i < lengths[0]; ++i)
                {
                    for (var j = 0; j < lengths[1]; ++j)
                    {
                        for (var k = 0; k < lengths[2]; ++k)
                        {
                            var elementKey = $"{i},{j},{k}";

                            result[i, j, k] = (object[,,])
                                DeserializeValueInternal(arrayCollection,
                                    elementKey, typeof(object[,,]),
                                    bindingFlags, settings, out success);

                            if (!success)
                                return null;
                        }
                    }
                }

                return result;
            }
            else if (typeof(object[]).IsAssignableFrom(type))
            {
                var arrayCollection = (INestableDictionary<string>)
                    collectionElement.GetCollection();

                var lengths = arrayCollection["Lengths"]
                    .GetElement()
                    .Split(',')
                    .Select(element =>
                    {
                        return Convert.ToInt32(
                            element,
                            CultureInfo.InvariantCulture);
                    })
                    .ToArray();
                var arrayElementType = Type.GetType(
                    arrayCollection["Type"].GetElement(),
                    false);

                if (arrayElementType == null)
                    arrayElementType = typeof(object);

                var result = Array.CreateInstance(arrayElementType, lengths);

                for (var i = 0; i < lengths[0]; ++i)
                {
                    var elementKey = $"{i}";

                    var value =
                        DeserializeValueInternal(arrayCollection,
                            elementKey, arrayElementType,
                            bindingFlags, settings, out success);

                    if (!success)
                        return null;

                    value = Convert.ChangeType(
                        value, arrayElementType);

                    result.SetValue(value, i);
                }

                return result;
            }
            else if (typeof(object[,]).IsAssignableFrom(type))
            {
                var arrayCollection = (INestableDictionary<string>)
                    collectionElement.GetCollection();

                var lengths = arrayCollection["Lengths"]
                    .GetElement()
                    .Split(',')
                    .Select(element =>
                    {
                        return Convert.ToInt32(
                            element,
                            CultureInfo.InvariantCulture);
                    })
                    .ToArray();
                var arrayElementType = Type.GetType(
                    arrayCollection["Type"].GetElement(),
                    false);

                if (arrayElementType == null)
                    arrayElementType = typeof(object);

                var result = Array.CreateInstance(arrayElementType, lengths);

                for (var i = 0; i < lengths[0]; ++i)
                {
                    for (var j = 0; j < lengths[1]; ++j)
                    {
                        var elementKey = $"{i},{j}";

                        var value =
                            DeserializeValueInternal(arrayCollection,
                                elementKey, arrayElementType,
                                bindingFlags, settings, out success);

                        if (!success)
                            return null;

                        value = Convert.ChangeType(
                            value, arrayElementType);

                        result.SetValue(value, i, j);
                    }
                }

                return result;
            }
            else if (typeof(object[,,]).IsAssignableFrom(type))
            {
                var arrayCollection = (INestableDictionary<string>)
                    collectionElement.GetCollection();

                var lengths = arrayCollection["Lengths"]
                    .GetElement()
                    .Split(',')
                    .Select(element =>
                    {
                        return Convert.ToInt32(
                            element,
                            CultureInfo.InvariantCulture);
                    })
                    .ToArray();
                var arrayElementType = Type.GetType(
                    arrayCollection["Type"].GetElement(),
                    false);

                if (arrayElementType == null)
                    arrayElementType = typeof(object);

                var result = Array.CreateInstance(arrayElementType, lengths);

                for (var i = 0; i < lengths[0]; ++i)
                {
                    for (var j = 0; j < lengths[1]; ++j)
                    {
                        for (var k = 0; k < lengths[2]; ++k)
                        {
                            var elementKey = $"{i},{j},{k}";

                            var value =
                                DeserializeValueInternal(arrayCollection,
                                    elementKey, arrayElementType,
                                    bindingFlags, settings, out success);

                            if (!success)
                                return null;

                            value = Convert.ChangeType(
                                value, arrayElementType);

                            result.SetValue(value, i, j, k);
                        }
                    }
                }

                return result;
            }
            else
            {
                if (collectionElement.Type == NestedType.Element)
                {
                    var objectElement = collectionElement.GetElement();

                    if (objectElement == null)
                        return null;
                    if (objectElement == "db_null")
                        return DBNull.Value;

                    var objectElementTypeIndex = objectElement.IndexOf('|');

                    if (objectElementTypeIndex != -1)
                    {
                        var objectElementType =
                            objectElement.Substring(0, objectElementTypeIndex);
                        collectionElement.Set(
                            objectElement.Substring(objectElementTypeIndex + 1));

                        if (!string.IsNullOrEmpty(objectElementType))
                        {
                            return DeserializeValueInternal(collection,
                                key, Type.GetType(objectElementType),
                                bindingFlags, settings, out success);
                        }
                    }

                    return Convert.ChangeType(
                        objectElement, type,
                        CultureInfo.InvariantCulture);
                }

                var objectCollection = (INestableDictionary<string>)
                    collectionElement.GetCollection();

                if (!objectCollection.ContainsKey("FullType"))
                {
                    success = false;
                    return null;
                }

                var objectType = Type.GetType(
                    objectCollection["FullType"].GetElement());

                return DeserializeInternal(objectType,
                    objectCollection, settings);
            }
        }
    }
}
