// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using RIS.Extensions;

namespace RIS.Collections.Nestable
{
    public static class NestableHelper
    {
        public static event EventHandler<RInformationEventArgs> Information;
        public static event EventHandler<RWarningEventArgs> Warning;
        public static event EventHandler<RErrorEventArgs> Error;

        private static readonly Dictionary<string, NestableCollectionType> CollectionsTypes;
        private static readonly Dictionary<NestableCollectionType, Type> CollectionsInfo;

        static NestableHelper()
        {
            string[] names = Enum.GetNames(typeof(NestableCollectionType));

            CollectionsTypes = new Dictionary<string, NestableCollectionType>(names.Length);
            CollectionsInfo = new Dictionary<NestableCollectionType, Type>(names.Length);

            for (int i = 0; i < names.Length; ++i)
            {
                ref string value = ref names[i];

                Type type = Type.GetType($"RIS.Collections.Nestable.{value}`1");

                if (type == null)
                    continue;

                NestableCollectionType collectionType =
                    (NestableCollectionType)Enum.Parse(typeof(NestableCollectionType), value);

                CollectionsTypes.Add(
                    value,
                    collectionType
                    );

                CollectionsInfo.Add(
                    collectionType,
                    type
                    );
            }
        }

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



        private static (string Represent, int EndIndex) GetRepresentPart(
            string represent, int startIndex)
        {
            if (string.IsNullOrEmpty(represent))
            {
                var exception =
                    new ArgumentOutOfRangeException(nameof(startIndex), $"{nameof(startIndex)} cannot be null or empty");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
            if (startIndex < 0)
            {
                var exception =
                    new ArgumentOutOfRangeException(nameof(startIndex), $"{nameof(startIndex)} cannot be less than zero");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            RepresentPartParseInfo parseInfo;

            try
            {
                parseInfo = RepresentPartParseInfo.Get(
                    represent, startIndex);
            }
            catch (Exception ex)
            {
                OnError(new RErrorEventArgs(ex, ex.Message));
                throw;
            }

            var skippedOccurrencesCount = 0;

            if (parseInfo.Type == NestedType.Collection)
            {
                Stack<char> parentheses = new Stack<char>();

                for (int i = startIndex; i < represent.Length; ++i)
                {
                    var ch = represent[i];

                    if (((ICollection<char>)parseInfo.ParenthesesMap.Values).Contains(ch))
                    {
                        if ((i > 0 && represent[i - 1] == '/')
                            && (i < represent.Length && represent[i + 1] == '/'))
                        {
                            continue;
                        }

                        parentheses.Push(ch);
                    }
                    else if (parseInfo.ParenthesesMap.TryGetValue(ch, out var chMapped))
                    {
                        if (parentheses.Pop() != chMapped)
                        {
                            var exception =
                                new FormatException($"Expected parentheses character '{chMapped}', but obtained character '{ch}'");
                            Events.OnError(new RErrorEventArgs(exception, exception.Message));
                            OnError(new RErrorEventArgs(exception, exception.Message));
                            throw exception;
                        }

                        if (parentheses.Count == 0)
                            break;

                        ++skippedOccurrencesCount;
                    }
                }
            }

            var endIndex = -1;
            var startIndexOccurrence = startIndex;

            for (int i = 0; i < 1 + skippedOccurrencesCount; ++i)
            {
                endIndex = represent
                    .IndexOfAny(parseInfo.EndValues, startIndexOccurrence)
                    .Index;

                if (endIndex == -1)
                    break;

                startIndexOccurrence = endIndex + 1;
            }

            if (endIndex == -1)
            {
                var exception =
                    new Exception($"Could not find the end of the representation part at the start index {startIndex}");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            var representPart = represent.Substring(
                startIndex + parseInfo.ExcludedStart,
                endIndex - (startIndex + parseInfo.ExcludedEnd));

            return (representPart, endIndex);
        }



        public static (INestableCollection<TValue> Collection, CollectionGeneralType GeneralType) CreateCollectionByType<TValue>(
            NestableCollectionType type)
        {
            Type typeCollection;

            if (!CollectionsInfo.ContainsKey(type))
            {
                return (null, CollectionGeneralType.Unknown);
            }

            typeCollection = CollectionsInfo[type]
                .MakeGenericType(typeof(TValue));
            object collection = typeCollection
                .GetConstructor(Array.Empty<Type>())?
                .Invoke(Array.Empty<object>());
            CollectionGeneralType generalType = CollectionGeneralType.Unknown;

            if (collection is INestableArray<TValue>)
                generalType = CollectionGeneralType.Array;
            else if (collection is INestableDictionary<TValue>)
                generalType = CollectionGeneralType.Dictionary;
            else if (collection is INestableList<TValue>)
                generalType = CollectionGeneralType.List;

            return ((INestableCollection<TValue>)collection, generalType);
        }
        public static (INestableCollection<TValue> Collection, CollectionGeneralType GeneralType) CreateCollectionByType<TValue>(
            NestableCollectionType type, int length)
        {
            Type typeCollection;

            if (!CollectionsInfo.ContainsKey(type))
            {
                return (null, CollectionGeneralType.Unknown);
            }

            typeCollection = CollectionsInfo[type]
                .MakeGenericType(typeof(TValue));
            object collection = typeCollection
                .GetConstructor(new Type[] { typeof(int) })?
                .Invoke(new object[] { length });
            CollectionGeneralType generalType = CollectionGeneralType.Unknown;

            if (collection is INestableArray<TValue>)
                generalType = CollectionGeneralType.Array;
            else if (collection is INestableDictionary<TValue>)
                generalType = CollectionGeneralType.Dictionary;
            else if (collection is INestableList<TValue>)
                generalType = CollectionGeneralType.List;

            return ((INestableCollection<TValue>)collection, generalType);
        }

        public static NestableCollectionType GetCollectionType(string typeString)
        {
            if (typeString.IndexOf('`') >= 0)
                typeString = typeString.Substring(0, typeString.Length - 2);

            if (!CollectionsTypes.ContainsKey(typeString))
                return NestableCollectionType.NestableListL;

            return CollectionsTypes[typeString];
        }
        public static NestableCollectionType GetCollectionType(INestableCollection collection)
        {
            return collection.CollectionType;
        }

        public static CollectionGeneralType GetGeneralType<TCollection>()
            where TCollection: INestableCollection
        {
            Type typeCollection = typeof(TCollection);

            if (typeCollection.IsAssignableFrom(typeof(INestableArray)))
                return CollectionGeneralType.Array;
            else if (typeCollection.IsAssignableFrom(typeof(INestableDictionary)))
                return CollectionGeneralType.Dictionary;
            else if (typeCollection.IsAssignableFrom(typeof(INestableList)))
                return CollectionGeneralType.List;

            return CollectionGeneralType.Unknown;
        }
        public static CollectionGeneralType GetGeneralType(INestableCollection collection)
        {
            if (collection is INestableArray)
                return CollectionGeneralType.Array;
            else if (collection is INestableDictionary)
                return CollectionGeneralType.Dictionary;
            else if (collection is INestableList)
                return CollectionGeneralType.List;

            return CollectionGeneralType.Unknown;
        }



        public static string ToStringRepresent<TValue>(NestedElement<TValue> value)
        {
            switch (value.Type)
            {
                case NestedType.Element:
                    return ToStringRepresent(value.GetElement());
                case NestedType.Array:
                    return ToStringRepresent(value.GetArray());
                case NestedType.Collection:
                    return ToStringRepresent(value.GetCollection());
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение поля Type в [NestedElement] для создания строкового представления", nameof(value));
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }
        }
        private static string ToStringRepresent<TValue>(TValue value)
        {
            if (value == null)
                return "null";

            if (ReferenceEquals(value, DBNull.Value)
                || value.ToString() == "db_null")
            {
                return "db_null";
            }

            string valueString;

            try
            {
                valueString = Convert.ChangeType(
                        value,
                        typeof(string),
                        CultureInfo.InvariantCulture)
                    .ToString();
            }
            catch (Exception)
            {
                valueString = value
                    .ToString();
            }

            if (valueString == null)
                return "null";

            valueString = valueString
                .Replace("null", "/null/")
                .Replace("|", "/|/")
                .Replace(":", "/:/")
                .Replace("\"", "/\"/")
                .Replace(",", "/,/")
                .Replace("[", "/[/")
                .Replace("]", "/]/")
                .Replace("{", "/{/")
                .Replace("}", "/}/");

            return valueString;
        }
        private static string ToStringRepresent<TValue>(TValue[] value)
        {
            if (value.Length == 0)
                return "[]";

            StringBuilder result = new StringBuilder(value.Length);

            result.Append('[');

            for (int i = 0; i < value.Length; ++i)
            {
                result.Append('"');
                result.Append(ToStringRepresent(value[i]));
                result.Append("\",");
            }

            if (result[result.Length - 1] == ',')
                result.Remove(result.Length - 1, 1);

            result.Append(']');

            return result.ToString();
        }
        public static string ToStringRepresent<TValue>(INestableCollection<TValue> value)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();

            switch (GetGeneralType(value))
            {
                case CollectionGeneralType.Array:
                case CollectionGeneralType.List:
                    break;
                case CollectionGeneralType.Dictionary:
                    return ToStringRepresentDictionary((INestableDictionary<TValue>)value);
                case CollectionGeneralType.Unknown:
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение CollectionGeneralType у коллекции", nameof(value));
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }

            if (value.Length == 0)
                return $"{{{GetCollectionType(value)}||}}";

            StringBuilder result = new StringBuilder(value.Length);

            result.Append('{');

            result.Append(GetCollectionType(value));
            result.Append("||");

            for (int i = 0; i < value.Length; ++i)
            {
                switch (value[i].Type)
                {
                    case NestedType.Element:
                        result.Append('"');
                        result.Append(ToStringRepresent(value[i].GetElement()));
                        result.Append("\",");
                        break;
                    case NestedType.Array:
                        result.Append(ToStringRepresent(value[i].GetArray()));
                        result.Append(',');
                        break;
                    case NestedType.Collection:
                        result.Append(ToStringRepresent(value[i].GetCollection()));
                        result.Append(',');
                        break;
                    default:
                        break;
                }
            }

            if (result[result.Length - 1] == ',')
                result.Remove(result.Length - 1, 1);

            result.Append('}');

            return result.ToString();
        }

        private static string ToStringRepresentDictionary(string key)
        {
            if (key == null)
                return "null";

            key = key?
                .Replace("null", "/null/")
                .Replace("|", "/|/")
                .Replace(":", "/:/")
                .Replace("\"", "/\"/")
                .Replace(",", "/,/")
                .Replace("[", "/[/")
                .Replace("]", "/]/")
                .Replace("{", "/{/")
                .Replace("}", "/}/");

            return key;
        }
        private static string ToStringRepresentDictionary<TValue>(string key, TValue value)
        {
            key = ToStringRepresentDictionary(key);
            string valueString = ToStringRepresent(value);

            return $"{key}::{valueString}";
        }
        private static string ToStringRepresentDictionary<TValue>(string key, TValue[] value)
        {
            if (value.Length == 0)
                return $"[{ToStringRepresentDictionary(key)}::]";

            StringBuilder result = new StringBuilder(value.Length);

            result.Append('[');

            result.Append(ToStringRepresentDictionary(key));
            result.Append("::");

            for (int i = 0; i < value.Length; ++i)
            {
                result.Append('"');
                result.Append(ToStringRepresent(value[i]));
                result.Append("\",");
            }

            if (result[result.Length - 1] == ',')
                result.Remove(result.Length - 1, 1);

            result.Append(']');

            return result.ToString();
        }
        private static string ToStringRepresentDictionary<TValue>(string key, INestableCollection<TValue> value)
        {
            switch (GetGeneralType(value))
            {
                case CollectionGeneralType.Array:
                case CollectionGeneralType.List:
                    break;
                case CollectionGeneralType.Dictionary:
                    return ToStringRepresentDictionary(key, (INestableDictionary<TValue>)value);
                case CollectionGeneralType.Unknown:
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение CollectionGeneralType у коллекции", nameof(value));
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }

            if (value.Length == 0)
                return $"{{{ToStringRepresentDictionary(key)}::{GetCollectionType(value)}||}}";

            StringBuilder result = new StringBuilder(value.Length);

            result.Append('{');

            result.Append(ToStringRepresentDictionary(key));
            result.Append("::");

            result.Append(GetCollectionType(value));
            result.Append("||");

            for (int i = 0; i < value.Length; ++i)
            {
                switch (value[i].Type)
                {
                    case NestedType.Element:
                        result.Append('"');
                        result.Append(ToStringRepresent(value[i].GetElement()));
                        result.Append("\",");
                        break;
                    case NestedType.Array:
                        result.Append(ToStringRepresent(value[i].GetArray()));
                        result.Append(',');
                        break;
                    case NestedType.Collection:
                        result.Append(ToStringRepresent(value[i].GetCollection()));
                        result.Append(',');
                        break;
                    default:
                        break;
                }
            }

            if (result[result.Length - 1] == ',')
                result.Remove(result.Length - 1, 1);

            result.Append('}');

            return result.ToString();
        }
        private static string ToStringRepresentDictionary<TValue>(INestableDictionary<TValue> value)
        {
            return ToStringRepresentDictionary(ToStringRepresentDictionary(value.Key), value);
        }
        private static string ToStringRepresentDictionary<TValue>(string key, INestableDictionary<TValue> value)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();

            if (value.Length == 0)
                return $"{{{ToStringRepresentDictionary(key)}::{GetCollectionType(value)}||}}";

            StringBuilder result = new StringBuilder(value.Length);

            result.Append('{');

            result.Append(ToStringRepresentDictionary(key));
            result.Append("::");

            result.Append(GetCollectionType(value));
            result.Append("||");

            for (int i = 0; i < value.Length; ++i)
            {
                switch (value[i].Type)
                {
                    case NestedType.Element:
                        result.Append('"');
                        result.Append(ToStringRepresentDictionary(value.GetKey(i), value[i].GetElement()));
                        result.Append("\",");
                        break;
                    case NestedType.Array:
                        result.Append(ToStringRepresentDictionary(value.GetKey(i), value[i].GetArray()));
                        result.Append(',');
                        break;
                    case NestedType.Collection:
                        result.Append(ToStringRepresentDictionary(value.GetKey(i), value[i].GetCollection()));
                        result.Append(',');
                        break;
                    default:
                        break;
                }
            }

            if (result[result.Length - 1] == ',')
                result.Remove(result.Length - 1, 1);

            result.Append('}');

            return result.ToString();
        }



        // ReSharper disable once RedundantAssignment
        private static TValue FromStringRepresent<TValue>(string represent, ref TValue value)
        {
            if (represent == "null")
            {
                value = default;

                return value;
            }

            if (represent == "db_null")
            {
                if (typeof(TValue) == typeof(string))
                {
                    value = (TValue)Convert.ChangeType("db_null",
                        typeof(TValue), CultureInfo.InvariantCulture);

                    return value;
                }

                value = (TValue)Convert.ChangeType(DBNull.Value,
                    typeof(TValue), CultureInfo.InvariantCulture);

                return value;
            }

            string valueString = represent
                .Replace("/null/", "null")
                .Replace("/|/", "|")
                .Replace("/:/", ":")
                .Replace("/\"/", "\"")
                .Replace("/,/", ",")
                .Replace("/[/", "[")
                .Replace("/]/", "]")
                .Replace("/{/", "{")
                .Replace("/}/", "}");

            value = (TValue)Convert.ChangeType(valueString,
                typeof(TValue), CultureInfo.InvariantCulture);

            return value;
        }
        private static TValue[] FromStringRepresent<TValue>(string represent, ref TValue[] value)
        {
            if (represent[0] != '[' || represent[represent.Length - 1] != ']')
            {
                var exception =
                    new ArgumentException("Неверный формат строки для преобразования в массив" + " " + represent, nameof(represent));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (represent == "[]")
            {
                value = Array.Empty<TValue>();
                return value;
            }

            string[] values = represent.Substring(2, represent.Length - 4).Split(new[] { "\",\"" }, StringSplitOptions.None);

            value = Array.ConvertAll(values, new Converter<string, TValue>((string stringValue) =>
            {
                TValue result = default(TValue);
                return FromStringRepresent(stringValue, ref result);
            }));

            return value;
        }
        public static INestableCollection<TValue> FromStringRepresent<TValue>(string represent)
        {
            if (represent[0] != '{' || represent[represent.Length - 1] != '}')
            {
                var exception =
                    new ArgumentException("Неверный формат строки для преобразования в коллекцию с поддержкой вложенности", nameof(represent));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            int typeDivide;
            string type;

            if (represent.Contains("::"))
            {
                int resultKeyDivide = represent.IndexOf("::", 1, StringComparison.Ordinal);
                typeDivide = represent.IndexOf("||", resultKeyDivide + 2, StringComparison.Ordinal);
                type = represent.Substring(resultKeyDivide + 2, typeDivide - resultKeyDivide - 2);
            }
            else
            {
                typeDivide = represent.IndexOf("||", 1, StringComparison.Ordinal);
                type = represent.Substring(1, typeDivide - 1);
            }

            (INestableCollection<TValue> collection, CollectionGeneralType generalType) =
                CreateCollectionByType<TValue>(GetCollectionType(type));

            return FromStringRepresent(represent, collection);
        }
        public static INestableCollection<TValue> FromStringRepresent<TValue>(string represent, INestableCollection<TValue> value)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();

            switch (GetGeneralType(value))
            {
                case CollectionGeneralType.Array:
                case CollectionGeneralType.List:
                    break;
                case CollectionGeneralType.Dictionary:
                    return FromStringRepresentDictionary<TValue>(represent, (INestableDictionary<TValue>)value);
                case CollectionGeneralType.Unknown:
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение CollectionGeneralType у коллекции", nameof(value));
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }

            if (represent[0] != '{' || represent[represent.Length - 1] != '}')
            {
                var exception =
                    new ArgumentException("Неверный формат строки для преобразования в коллекцию с поддержкой вложенности", nameof(represent));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            int typeDivide;
            string type;

            if (represent.Contains("::"))
            {
                int resultKeyDivide = represent.IndexOf("::", 1, StringComparison.Ordinal);
                typeDivide = represent.IndexOf("||", resultKeyDivide + 2, StringComparison.Ordinal);
                type = represent.Substring(resultKeyDivide + 2, typeDivide - resultKeyDivide - 2);
            }
            else
            {
                typeDivide = represent.IndexOf("||", 1, StringComparison.Ordinal);
                type = represent.Substring(1, typeDivide - 1);
            }

            if (GetCollectionType(type) != GetCollectionType(value))
            {
                var exception =
                    new ArgumentException("Тип переданной коллекции не соответствует типу коллекции из строкового представления", nameof(value));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            value.Clear();

            if (represent.EndsWith("||}", StringComparison.Ordinal))
                return value;

            int divideIndex = typeDivide + 1;

            do
            {
                if (represent[divideIndex + 1] == '\"')
                {
                    var (partRepresent, partEndIndex) = GetRepresentPart(
                        represent, divideIndex + 1);

                    TValue result = default(TValue);
                    value.Add(FromStringRepresent(partRepresent, ref result));

                    divideIndex = partEndIndex;
                }
                else if (represent[divideIndex + 1] == '[')
                {
                    var (partRepresent, partEndIndex) = GetRepresentPart(
                        represent, divideIndex + 1);

                    TValue[] result = Array.Empty<TValue>();
                    value.Add(FromStringRepresent(partRepresent, ref result));

                    divideIndex = partEndIndex;
                }
                else if (represent[divideIndex + 1] == '{')
                {
                    var (partRepresent, partEndIndex) = GetRepresentPart(
                        represent, divideIndex + 1);

                    int resultTypeDivide;
                    string resultType;

                    if (partRepresent.Contains("::"))
                    {
                        int resultKeyDivide = partRepresent.IndexOf("::", 1, StringComparison.Ordinal);
                        resultTypeDivide = partRepresent.IndexOf("||", resultKeyDivide + 2, StringComparison.Ordinal);
                        resultType = partRepresent.Substring(resultKeyDivide + 2, resultTypeDivide - resultKeyDivide - 2);
                    }
                    else
                    {
                        resultTypeDivide = partRepresent.IndexOf("||", 1, StringComparison.Ordinal);
                        resultType = partRepresent.Substring(1, resultTypeDivide - 1);
                    }

                    (INestableCollection<TValue> result, CollectionGeneralType generalType) =
                        CreateCollectionByType<TValue>(GetCollectionType(resultType));

                    switch (generalType)
                    {
                        case CollectionGeneralType.Array:
                        case CollectionGeneralType.List:
                            value.Add(FromStringRepresent<TValue>(partRepresent, result));
                            break;
                        case CollectionGeneralType.Dictionary:
                            value.Add(FromStringRepresentDictionary<TValue>(partRepresent, (INestableDictionary<TValue>)result));
                            break;
                        case CollectionGeneralType.Unknown:
                        default:
                            var exception =
                                new ArgumentException("Недопустимое значение CollectionGeneralType у коллекции", nameof(value));
                            Events.OnError(new RErrorEventArgs(exception, exception.Message));
                            OnError(new RErrorEventArgs(exception, exception.Message));
                            throw exception;
                    }

                    divideIndex = partEndIndex;
                }
            } while (divideIndex != -1
                     && divideIndex != represent.Length - 1
                     && (divideIndex = represent.IndexOf(',', divideIndex + 1)) != -1
                     );

            return value;
        }

        private static string FromStringRepresentDictionary(string key)
        {
            if (key == "null")
                return null;

            key = key?
                .Replace("/null/", "null")
                .Replace("/|/", "|")
                .Replace("/:/", ":")
                .Replace("/\"/", "\"")
                .Replace("/,/", ",")
                .Replace("/[/", "[")
                .Replace("/]/", "]")
                .Replace("/{/", "{")
                .Replace("/}/", "}");

            return key;
        }
        private static INestableCollection<TValue> FromStringRepresentDictionary<TValue>(string represent, INestableDictionary<TValue> value)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();

            if (represent[0] != '{' || represent[represent.Length - 1] != '}')
            {
                var exception =
                    new ArgumentException("Неверный формат строки для преобразования в коллекцию с поддержкой вложенности", nameof(represent));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            int typeDivide;
            string type;
            string key = string.Empty;

            if (represent.Contains("::"))
            {
                int resultKeyDivide = represent.IndexOf("::", 1, StringComparison.Ordinal);
                key = FromStringRepresentDictionary(represent.Substring(1, resultKeyDivide - 1));
                typeDivide = represent.IndexOf("||", resultKeyDivide + 2, StringComparison.Ordinal);
                type = represent.Substring(resultKeyDivide + 2, typeDivide - resultKeyDivide - 2);

                represent = $"{{{represent.Substring(resultKeyDivide + 2)}";
            }
            else
            {
                typeDivide = represent.IndexOf("||", 1, StringComparison.Ordinal);
                type = represent.Substring(1, typeDivide - 1);
            }

            if (GetCollectionType(type) != GetCollectionType(value))
            {
                var exception =
                    new ArgumentException("Тип переданной коллекции не соответствует типу коллекции из строкового представления", nameof(value));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            value.Key = key;

            value.Clear();

            if (represent.EndsWith("||}", StringComparison.Ordinal))
                return value;

            int divideIndex = typeDivide + 1;
            divideIndex -= key.Length != 0 ? key.Length + 2 : 0 ;

            do
            {
                if (represent[divideIndex + 1] == '\"')
                {
                    var (partRepresent, partEndIndex) = GetRepresentPart(
                        represent, divideIndex + 1);

                    TValue result = default(TValue);

                    if (partRepresent.Contains("::"))
                    {
                        int resultKeyDivide = partRepresent.IndexOf("::", 0, StringComparison.Ordinal);
                        string resultKey = FromStringRepresentDictionary(partRepresent.Substring(0, resultKeyDivide));

                        partRepresent = $"{partRepresent.Substring(resultKeyDivide + 2)}";

                        value.Add(resultKey, FromStringRepresent(partRepresent, ref result));
                    }
                    else
                    {
                        value.Add(FromStringRepresent(partRepresent, ref result));
                    }

                    divideIndex = partEndIndex;
                }
                else if (represent[divideIndex + 1] == '[')
                {
                    var (partRepresent, partEndIndex) = GetRepresentPart(
                        represent, divideIndex + 1);

                    TValue[] result = Array.Empty<TValue>();

                    if (partRepresent.Contains("::"))
                    {
                        int resultKeyDivide = partRepresent.IndexOf("::", 1, StringComparison.Ordinal);
                        string resultKey = FromStringRepresentDictionary(partRepresent.Substring(1, resultKeyDivide - 1));

                        partRepresent = $"[{partRepresent.Substring(resultKeyDivide + 2)}";

                        value.Add(resultKey, FromStringRepresent(partRepresent, ref result));
                    }
                    else
                    {
                        value.Add(FromStringRepresent(partRepresent, ref result));
                    }

                    divideIndex = partEndIndex;
                }
                else if (represent[divideIndex + 1] == '{')
                {
                    var (partRepresent, partEndIndex) = GetRepresentPart(
                        represent, divideIndex + 1);

                    if (partRepresent.Contains("::"))
                    {
                        int resultKeyDivide = partRepresent.IndexOf("::", 1, StringComparison.Ordinal);
                        string resultKey = FromStringRepresentDictionary(partRepresent.Substring(1, resultKeyDivide - 1));
                        int resultTypeDivide = partRepresent.IndexOf("||", resultKeyDivide + 2, StringComparison.Ordinal);
                        string resultType = partRepresent.Substring(resultKeyDivide + 2, resultTypeDivide - resultKeyDivide - 2);

                        (INestableCollection<TValue> result, CollectionGeneralType generalType) =
                            CreateCollectionByType<TValue>(GetCollectionType(resultType));
                        value.Add(resultKey, FromStringRepresent<TValue>(partRepresent, result));
                    }
                    else
                    {
                        int collectionTypeDivide = partRepresent.IndexOf("||", 1, StringComparison.Ordinal);
                        string collectionType = partRepresent.Substring(1, collectionTypeDivide - 1);

                        (INestableCollection<TValue> result, CollectionGeneralType generalType) =
                            CreateCollectionByType<TValue>(GetCollectionType(collectionType));
                        value.Add(FromStringRepresent<TValue>(partRepresent, result));
                    }

                    divideIndex = partEndIndex;
                }
            } while (divideIndex != -1
                     && divideIndex != represent.Length - 1
                     && (divideIndex = represent.IndexOf(',', divideIndex + 1)) != -1);

            return value;
        }



        public static IEnumerable<TValue> Enumerate<TValue>(NestedElement<TValue> value)
        {
            switch (value.Type)
            {
                case NestedType.Element:
                    return Enumerate(value.GetElement());
                case NestedType.Array:
                    return Enumerate(value.GetArray());
                case NestedType.Collection:
                    return Enumerate(value.GetCollection());
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение поля Type в [NestedElement] для старта перечисления", nameof(value));
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }
        }
        private static IEnumerable<TValue> Enumerate<TValue>(TValue value)
        {
            yield return value;
        }
        private static IEnumerable<TValue> Enumerate<TValue>(TValue[] value)
        {
            for (int i = 0; i < value.Length; ++i)
            {
                yield return value[i];
            }
        }
        public static IEnumerable<TValue> Enumerate<TValue>(INestableCollection<TValue> value)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();

            for (int i = 0; i < value.Length; ++i)
            {
                switch (value[i].Type)
                {
                    case NestedType.Element:
                        foreach (var element in Enumerate(value[i].GetElement()))
                        {
                            yield return element;
                        }
                        break;
                    case NestedType.Array:
                        foreach (var element in Enumerate(value[i].GetArray()))
                        {
                            yield return element;
                        }
                        break;
                    case NestedType.Collection:
                        foreach (var element in Enumerate(value[i].GetCollection()))
                        {
                            yield return element;
                        }
                        break;
                    default:
                        var exception =
                            new Exception("Недопустимое значение поля Type в [NestedElement]");
                        Events.OnError(new RErrorEventArgs(exception, exception.Message));
                        OnError(new RErrorEventArgs(exception, exception.Message));
                        throw exception;
                }
            }
        }
    }
}
