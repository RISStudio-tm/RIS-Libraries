// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<char, char> ElementParenthesesMap;
        private static readonly Dictionary<char, char> ArrayParenthesesMap;
        private static readonly Dictionary<char, char> CollectionParenthesesMap;

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

            ElementParenthesesMap = new Dictionary<char, char>
            {
                { '\"', '\"' }
            };
            ArrayParenthesesMap = new Dictionary<char, char>
            {
                { ']', '[' }
            };
            CollectionParenthesesMap = new Dictionary<char, char>
            {
                { '}', '{' }
            };
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



        private static Dictionary<char, char> GetParenthesesMap(NestedType type)
        {
            switch (type)
            {
                case NestedType.Element:
                    return ElementParenthesesMap;
                case NestedType.Array:
                    return ArrayParenthesesMap;
                case NestedType.Collection:
                    return CollectionParenthesesMap;
                default:
                    var exception =
                        new ArgumentException($"Invalid value for the {nameof(type)} parameter", nameof(type));
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }
        }

        private static (string Represent, int EndIndex) GetRepresentPart(string represent, int startIndex)
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

            NestedType partType;
            string[] partEndValues;
            int partExcludedStart;
            int partExcludedEnd;

            switch (represent[startIndex])
            {
                case '\"':
                    partType = NestedType.Element;
                    partEndValues = new[] { "\",", "\"}" };
                    partExcludedStart = 1;
                    partExcludedEnd = 1;
                    break;
                case '[':
                    partType = NestedType.Array;
                    partEndValues = new[] { "],", "]}" };
                    partExcludedStart = 0;
                    partExcludedEnd = -1;
                    break;
                case '{':
                    partType = NestedType.Collection;
                    partEndValues = new[] { "},", "}}" };
                    partExcludedStart = 0;
                    partExcludedEnd = -1;
                    break;
                default:
                    var exception =
                        new ArgumentException($"{nameof(startIndex)}[{startIndex}] indicates an unknown character of the beginning of the representation", nameof(startIndex));
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }

            var partParenthesesMap = GetParenthesesMap(partType);
            var skippedOccurrencesCount = 0;

            if (partType == NestedType.Collection)
            {
                Stack<char> parentheses = new Stack<char>();

                for (int i = startIndex; i < represent.Length; ++i)
                {
                    var ch = represent[i];

                    if ((i > 0 && represent[i - 1] == '/')
                        && (i < represent.Length && represent[i + 1] == '/'))
                    {
                        continue;
                    }

                    if (partParenthesesMap.ContainsValue(ch))
                    {
                        parentheses.Push(ch);
                    }
                    else if (partParenthesesMap.TryGetValue(ch, out var chMapped))
                    {
                        if (parentheses.Pop() != chMapped)
                        {
                            var exception =
                                new FormatException($"expected parentheses character '{chMapped}', but obtained character '{ch}'");
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
                    .IndexOfAny(partEndValues, startIndexOccurrence)
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
                startIndex + partExcludedStart,
                endIndex - (startIndex + partExcludedEnd));

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

            typeCollection = CollectionsInfo[type].MakeGenericType(typeof(TValue));
            object collection = typeCollection.GetConstructor(Array.Empty<Type>())?.Invoke(Array.Empty<object>());
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

            typeCollection = CollectionsInfo[type].MakeGenericType(typeof(TValue));
            object collection = typeCollection.GetConstructor(new Type[] { typeof(int) })?.Invoke(new object[] { length });
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
        public static NestableCollectionType GetCollectionType<TValue>(INestableCollection<TValue> collection)
        {
            return collection.CollectionType;
        }

        public static CollectionGeneralType GetGeneralType<TValue, TCollection>()
            where TCollection: INestableCollection<TValue>
        {
            Type typeTCollection = typeof(TCollection);

            if (typeTCollection.IsAssignableFrom(typeof(INestableArray<TValue>)))
                return CollectionGeneralType.Array;
            else if (typeTCollection.IsAssignableFrom(typeof(INestableDictionary<TValue>)))
                return CollectionGeneralType.Dictionary;
            else if (typeTCollection.IsAssignableFrom(typeof(INestableList<TValue>)))
                return CollectionGeneralType.List;

            return CollectionGeneralType.Unknown;
        }
        public static CollectionGeneralType GetGeneralType<TValue>(INestableCollection<TValue> collection)
        {
            if (collection is INestableArray<TValue>)
                return CollectionGeneralType.Array;
            else if (collection is INestableDictionary<TValue>)
                return CollectionGeneralType.Dictionary;
            else if (collection is INestableList<TValue>)
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
        public static string ToStringRepresent<TValue>(TValue value)
        {
            if (value == null)
                return "null";

            if (ReferenceEquals(value, DBNull.Value)
                || value.ToString() == "db_null")
            {
                return "db_null";
            }

            string valueString = value.ToString()?
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
        public static string ToStringRepresent<TValue>(TValue[] value)
        {
            if (value.Length == 0)
                return "[]";

            StringBuilder result = new StringBuilder();

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

            StringBuilder result = new StringBuilder();

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
                }
            }

            if (result[result.Length - 1] == ',')
                result.Remove(result.Length - 1, 1);

            result.Append('}');

            return result.ToString();
        }

        public static string ToStringRepresentDictionary<TValue>(string key)
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
        public static string ToStringRepresentDictionary<TValue>(string key, TValue value)
        {
            key = ToStringRepresentDictionary<TValue>(key);
            string valueString = ToStringRepresent(value);

            return $"{key}::{valueString}";
        }
        public static string ToStringRepresentDictionary<TValue>(string key, TValue[] value)
        {
            if (value.Length == 0)
                return $"[{ToStringRepresentDictionary<TValue>(key)}::]";

            StringBuilder result = new StringBuilder();

            result.Append('[');

            result.Append(ToStringRepresentDictionary<TValue>(key));
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
        public static string ToStringRepresentDictionary<TValue>(string key, INestableCollection<TValue> value)
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
                return $"{{{ToStringRepresentDictionary<TValue>(key)}::{GetCollectionType(value)}||}}";

            StringBuilder result = new StringBuilder();

            result.Append('{');

            result.Append(ToStringRepresentDictionary<TValue>(key));
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
                }
            }

            if (result[result.Length - 1] == ',')
                result.Remove(result.Length - 1, 1);

            result.Append('}');

            return result.ToString();
        }
        public static string ToStringRepresentDictionary<TValue>(INestableDictionary<TValue> value)
        {
            return ToStringRepresentDictionary(ToStringRepresentDictionary<TValue>(value.Key), value);
        }
        public static string ToStringRepresentDictionary<TValue>(string key, INestableDictionary<TValue> value)
        {
            if (value.Length == 0)
                return $"{{{ToStringRepresentDictionary<TValue>(key)}::{GetCollectionType(value)}||}}";

            StringBuilder result = new StringBuilder();

            result.Append('{');

            result.Append(ToStringRepresentDictionary<TValue>(key));
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
                }
            }

            if (result[result.Length - 1] == ',')
                result.Remove(result.Length - 1, 1);

            result.Append('}');

            return result.ToString();
        }


        public static TValue FromStringRepresent<TValue>(string represent, ref TValue value)
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
                    value = (TValue)Convert.ChangeType("db_null", typeof(TValue));

                    return value;
                }

                value = (TValue)Convert.ChangeType(DBNull.Value, typeof(TValue));

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

            value = (TValue)Convert.ChangeType(valueString, typeof(TValue));

            return value;
        }
        public static TValue[] FromStringRepresent<TValue>(string represent, ref TValue[] value)
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
            string type = string.Empty;

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
            string type = string.Empty;

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

        public static string FromStringRepresentDictionary<TValue>(string key)
        {
            if (key == "null")
            {
                key = default;

                return key;
            }

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
        public static INestableCollection<TValue> FromStringRepresentDictionary<TValue>(string represent, INestableDictionary<TValue> value)
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
            string type = string.Empty;
            string key = string.Empty;

            if (represent.Contains("::"))
            {
                int resultKeyDivide = represent.IndexOf("::", 1, StringComparison.Ordinal);
                key = FromStringRepresentDictionary<TValue>(represent.Substring(1, resultKeyDivide - 1));
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
                        string resultKey = FromStringRepresentDictionary<TValue>(partRepresent.Substring(0, resultKeyDivide));

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
                        string resultKey = FromStringRepresentDictionary<TValue>(partRepresent.Substring(1, resultKeyDivide - 1));

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
                        string resultKey = FromStringRepresentDictionary<TValue>(partRepresent.Substring(1, resultKeyDivide - 1));
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
        public static IEnumerable<TValue> Enumerate<TValue>(TValue value)
        {
            yield return value;
        }
        public static IEnumerable<TValue> Enumerate<TValue>(TValue[] value)
        {
            for (int i = 0; i < value.Length; ++i)
            {
                yield return value[i];
            }
        }
        public static IEnumerable<TValue> Enumerate<TValue>(INestableCollection<TValue> value)
        {
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
