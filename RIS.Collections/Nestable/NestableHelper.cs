// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using RIS.Collections.Chunked;
using RIS.Collections.Extensions;
using RIS.Collections.Nestable.Entities;
using RIS.Collections.Nestable.Frames;

namespace RIS.Collections.Nestable
{
    public static class NestableHelper
    {
        public static event EventHandler<RInformationEventArgs> Information;
        public static event EventHandler<RWarningEventArgs> Warning;
        public static event EventHandler<RErrorEventArgs> Error;



        private static readonly Dictionary<(Type CollectionType, Type ElementType, Type ConstructorParameterType), Func<object, INestableCollection>> CollectionsConstructorsCache;
        private static readonly Dictionary<string, NestableCollectionType> CollectionsTypes;
        private static readonly Dictionary<NestableCollectionType, Type> CollectionsInfo;



        private static readonly char[] Replacers = { 'n', '|', ':', ',', '\"', '[', ']', '{', '}' };
        private static readonly string[] ReplacersFull = { "null" };
        private static readonly char Delimiter = '/';

        private static readonly string TypeDivide = "||";
        private static readonly string KeyDivide = "::";



        static NestableHelper()
        {
            CollectionsConstructorsCache = new Dictionary<(Type CollectionType, Type ElementType, Type ConstructorParameterType), Func<object, INestableCollection>>(10);

            var names = Enum.GetNames(typeof(NestableCollectionType));

            CollectionsTypes = new Dictionary<string, NestableCollectionType>(names.Length);
            CollectionsInfo = new Dictionary<NestableCollectionType, Type>(names.Length);

            for (int i = 0; i < names.Length; ++i)
            {
                ref var name = ref names[i];

                var type = Type.GetType(
                    $"RIS.Collections.Nestable.{name}`1, RIS.Collections");

                if (type == null)
                    continue;

                var collectionType = (NestableCollectionType)Enum.Parse(
                    typeof(NestableCollectionType), name);

                CollectionsTypes.Add(
                    name, collectionType);
                CollectionsInfo.Add(
                    collectionType, type);
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



        private static Func<object, INestableCollection> GetConstructorDelegate(
            Type collectionType, Type elementType,
            Type constructorParameterType = null)
        {
            if (CollectionsConstructorsCache.TryGetValue(
                    (collectionType, elementType, constructorParameterType),
                    out var constructorDelegate))
            {
                return constructorDelegate;
            }

            Expression<Func<object, INestableCollection>> constructor;

            if (constructorParameterType != null
                && constructorParameterType != typeof(void))
            {
                var constructorInfo = collectionType
                    .MakeGenericType(elementType)
                    .GetConstructor(new[] { constructorParameterType })!;
                var constructorParameter = Expression.Parameter(
                    typeof(object));

                constructor = Expression.Lambda<Func<object, INestableCollection>>(
                    Expression.New(
                        constructorInfo,
                        Expression.Convert(
                            constructorParameter,
                            constructorParameterType)),
                    constructorParameter);
            }
            else
            {
                var constructorInfo = collectionType
                    .MakeGenericType(elementType)
                    .GetConstructor(Array.Empty<Type>())!;
                var constructorParameter = Expression.Parameter(
                    typeof(object));

                constructor = Expression.Lambda<Func<object, INestableCollection>>(
                    Expression.New(
                        constructorInfo),
                    constructorParameter);
            }

            var expression = constructor.Compile();

            CollectionsConstructorsCache[(collectionType, elementType, constructorParameterType)] = expression;

            return expression;
        }

        private static INestableCollection<TValue> InvokeConstructor<TValue>(
            Type collectionType, Type constructorParameterType = null,
            object constructorParameter = null)
        {
            var constructorDelegate = GetConstructorDelegate(
                collectionType, typeof(TValue),
                constructorParameterType);

            return (INestableCollection<TValue>)constructorDelegate(
                constructorParameter);
        }
        private static INestableCollection InvokeConstructor(
            Type collectionType, Type elementType,
            Type constructorParameterType = null,
            object constructorParameter = null)
        {
            var constructorDelegate = GetConstructorDelegate(
                collectionType, elementType,
                constructorParameterType);

            return constructorDelegate(
                constructorParameter);
        }



        private static string EncodeRepresent(
            string source)
        {
            var preAllocatedLength =
                (int)(source.Length * 1.2) + 8;

            return EncodeRepresent(source,
                preAllocatedLength);
        }
        private static string EncodeRepresent(
            string source, int preAllocatedLength)
        {
            var result = new StringBuilder(
                preAllocatedLength);
            var skippedCount = 0;
            var buffer = '\0';

            int index;



            void AppendSkippedChars()
            {
                if (skippedCount <= 0)
                    return;

                result.Append(source,
                    index - skippedCount,
                    skippedCount);

                skippedCount = 0;
            }



            for (index = 0; index < source.Length; ++index)
            {
                var ch = source[index];

                switch (ch)
                {
                    case 'n':
                        if (index >= source.Length - 3
                            || source[index + 1] != 'u'
                            || source[index + 2] != 'l'
                            || source[index + 3] != 'l')
                        {
                            ++skippedCount;

                            break;
                        }

                        AppendSkippedChars();

                        result.Append(
                            $"{Delimiter}null{Delimiter}");

                        index += 3;

                        break;
                    case '|':
                        buffer = '|';
                        break;
                    case ':':
                        buffer = ':';
                        break;
                    case ',':
                        buffer = ',';
                        break;
                    case '\"':
                        buffer = '\"';
                        break;
                    case '[':
                        buffer = '[';
                        break;
                    case ']':
                        buffer = ']';
                        break;
                    case '{':
                        buffer = '{';
                        break;
                    case '}':
                        buffer = '}';
                        break;
                    default:
                        ++skippedCount;
                        break;
                }

                if (buffer != '\0')
                {
                    AppendSkippedChars();

                    result.Append(Delimiter);
                    result.Append(buffer);
                    result.Append(Delimiter);

                    buffer = '\0';
                }
            }

            AppendSkippedChars();

            return result.ToString();
        }

        private static string DecodeRepresent(
            ReadOnlySpan<char> source)
        {
            var preAllocatedLength =
                source.Length;

            return DecodeRepresent(source,
                preAllocatedLength);
        }
        private static string DecodeRepresent(
            ReadOnlySpan<char> source, int preAllocatedLength)
        {
            var prefixIndex = source.IndexOf(
                Delimiter);

            if (prefixIndex == -1)
                return new string(source);

            var result = new Span<char>(
                new char[preAllocatedLength]);

            var sourceSpan = source;
            var position = 0;

            do
            {
                var replaced = false;

                for (var i = 0; i < ReplacersFull.Length; ++i)
                {
                    ref var replacer = ref ReplacersFull[i];

                    var replacerLength = replacer.Length;
                    var postfixIndex = prefixIndex + replacerLength + 1;

                    if (sourceSpan.Length <= postfixIndex
                        || sourceSpan[postfixIndex] != Delimiter
                        || !sourceSpan.Slice(prefixIndex + 1, replacerLength).SequenceEqual(replacer))
                    {
                        continue;
                    }

                    sourceSpan[..(prefixIndex + 1)].CopyTo(
                        result[position..]);
                    replacer.CopyTo(
                        result[(position + prefixIndex)..]);

                    position += prefixIndex + replacerLength + 1;
                    sourceSpan = sourceSpan[(postfixIndex + 1)..];

                    replaced = true;

                    goto FinishReplacing;
                }

                for (var i = ReplacersFull.Length; i < Replacers.Length; ++i)
                {
                    ref var replacer = ref Replacers[i];

                    var postfixIndex = prefixIndex + 2;

                    if (sourceSpan.Length <= postfixIndex
                        || sourceSpan[postfixIndex] != Delimiter
                        || sourceSpan[prefixIndex + 1] != replacer)
                    {
                        continue;
                    }

                    sourceSpan[..(prefixIndex + 1)].CopyTo(
                        result[position..]);
                    result[position + prefixIndex] =
                        replacer;

                    position += prefixIndex + 2;
                    sourceSpan = sourceSpan[(postfixIndex + 1)..];

                    replaced = true;

                    goto FinishReplacing;
                }

                //Label
                FinishReplacing:



                if (replaced)
                    continue;

                sourceSpan[..(prefixIndex + 1)].CopyTo(
                    result[position..]);

                position += prefixIndex + 1;
                sourceSpan = sourceSpan[(prefixIndex + 1)..];
            } while ((prefixIndex = sourceSpan.IndexOf(Delimiter)) != -1);

            sourceSpan.CopyTo(
                result[position..]);

            return new string(result[..(position + sourceSpan.Length)]);
        }



        private static (NestedType Type, int StartIndex, int EndIndex, int Length) GetRepresentPart(
            ReadOnlySpan<char> represent, int startIndex)
        {
            if (represent == null)
            {
                var exception = new ArgumentException(
                    $"{nameof(startIndex)} cannot be null",
                    nameof(startIndex));
                Events.OnError(
                    new RErrorEventArgs(exception, exception.Message));
                OnError(
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
            if (startIndex < 0)
            {
                var exception = new ArgumentOutOfRangeException(
                    nameof(startIndex),
                    $"{nameof(startIndex)} cannot be less than zero");
                Events.OnError(
                    new RErrorEventArgs(exception, exception.Message));
                OnError(
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            var parseInfo = RepresentPartParseInfo.Get(
                represent[startIndex]);
            var endIndex = -1;

            if (parseInfo.Type == NestedType.Collection)
            {
                var parentheses = 0;
                var parenthesesMap = parseInfo.ParenthesesMap;

                for (var i = startIndex; i < represent.Length; ++i)
                {
                    var ch = represent[i];

                    if (ch == parenthesesMap.Key)
                    {
                        if (i > 0
                            && represent[i - 1] == '/')
                        {
                            continue;
                        }

                        ++parentheses;
                    }
                    else if (ch == parenthesesMap.Value)
                    {
                        if (i < represent.Length - 1
                            && represent[i + 1] == '/')
                        {
                            continue;
                        }

                        --parentheses;

                        if (parentheses == 0)
                        {
                            endIndex = i;

                            break;
                        }
                    }
                }
            }
            else
            {
                endIndex =
                    startIndex +
                    parseInfo.EndValuesTrie
                        .IndexOfAny(
                            represent[startIndex..])
                        .Index;
            }

            if (endIndex == -1)
            {
                var exception = new Exception(
                    $"Could not find the end of the representation part at the start index {startIndex}");
                Events.OnError(
                    new RErrorEventArgs(exception, exception.Message));
                OnError(
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return (parseInfo.Type, startIndex, endIndex,
                    endIndex - startIndex + 1);
        }



        public static (INestableCollection<TValue> Collection, CollectionGeneralType GeneralType) CreateCollectionByType<TValue>(
            NestableCollectionType type)
        {
            if (!CollectionsInfo.TryGetValue(type, out var collectionType))
                return (null, CollectionGeneralType.Unknown);

            var collection = InvokeConstructor<TValue>(
                collectionType);
            var generalType = GetGeneralType(collection);

            return (collection, generalType);
        }
        public static (INestableCollection<TValue> Collection, CollectionGeneralType GeneralType) CreateCollectionByType<TValue>(
            NestableCollectionType type, string represent)
        {
            if (!CollectionsInfo.TryGetValue(type, out var collectionType))
                return (null, CollectionGeneralType.Unknown);

            var collection = InvokeConstructor<TValue>(
                collectionType, typeof(string),
                represent);
            var generalType = GetGeneralType(collection);

            return (collection, generalType);
        }
        public static (INestableCollection<TValue> Collection, CollectionGeneralType GeneralType) CreateCollectionByType<TValue>(
            NestableCollectionType type, int length)
        {
            if (!CollectionsInfo.TryGetValue(type, out var collectionType))
                return (null, CollectionGeneralType.Unknown);

            var collection = InvokeConstructor<TValue>(
                collectionType, typeof(int),
                length);
            var generalType = GetGeneralType(collection);

            return (collection, generalType);
        }

        public static NestableCollectionType GetCollectionType(
            string typeName)
        {
            return GetCollectionType(
                typeName.AsSpan());
        }
        public static NestableCollectionType GetCollectionType(
            ReadOnlySpan<char> typeName)
        {
            if (typeName == null || typeName.IsEmpty)
                return NestableCollectionType.Unknown;

            if (typeName.IndexOf('`') >= 0)
                typeName = typeName.Slice(0, typeName.Length - 2);
            else if (typeName.IndexOf('[') >= 0)
                typeName = typeName.Slice(0, typeName.Length - 3);
            else if (typeName.IndexOf('<') >= 0)
                typeName = typeName.Slice(0, typeName.Length - 3);

            if (CollectionsTypes.TryGetValue(typeName.ToString(), out var type))
                return type;

            return NestableCollectionType.Unknown;

        }
        public static NestableCollectionType GetCollectionType(
            INestableCollection collection)
        {
            return collection?.CollectionType
                ?? NestableCollectionType.Unknown;
        }

        public static CollectionGeneralType GetGeneralType<TCollection>()
            where TCollection: INestableCollection
        {
            var collectionType = typeof(TCollection);

            if (collectionType.IsAssignableFrom(typeof(INestableArray)))
                return CollectionGeneralType.Array;
            else if (collectionType.IsAssignableFrom(typeof(INestableDictionary)))
                return CollectionGeneralType.Dictionary;
            else if (collectionType.IsAssignableFrom(typeof(INestableList)))
                return CollectionGeneralType.List;

            return CollectionGeneralType.Unknown;
        }
        public static CollectionGeneralType GetGeneralType(
            INestableCollection collection)
        {
            if (collection is INestableArray)
                return CollectionGeneralType.Array;
            else if (collection is INestableDictionary)
                return CollectionGeneralType.Dictionary;
            else if (collection is INestableList)
                return CollectionGeneralType.List;

            return CollectionGeneralType.Unknown;
        }



        public static string ToStringRepresent<TValue>(
            NestedElement<TValue> value)
        {
            var builder = new StringBuilder(30);

            switch (value.Type)
            {
                case NestedType.Element:
                    ToStringRepresent(ref builder, value.GetElement());
                    return builder.ToString();
                case NestedType.Array:
                    ToStringRepresent(ref builder, value.GetArray());
                    return builder.ToString();
                case NestedType.Collection:
                    return ToStringRepresent(value.GetCollection());
                case NestedType.Unknown:
                default:
                    var exception = new ArgumentException(
                        "Недопустимое значение поля Type в [NestedElement] для создания строкового представления",
                        nameof(value));
                    Events.OnError(
                        new RErrorEventArgs(exception, exception.Message));
                    OnError(
                        new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }
        }
        private static void ToStringRepresent(
            ref StringBuilder builder, string key)
        {
            if (key == null)
            {
                builder.Append("null");

                return;
            }
            else if (key.Length == 0)
            {
                return;
            }

            key = EncodeRepresent(
                key);

            builder.Append(key);
        }
        private static void ToStringRepresent<TValue>(
            ref StringBuilder builder, TValue value,
            string key = null)
        {
            builder.Append('"');

            if (key != null)
            {
                ToStringRepresent(ref builder, key);

                builder.Append("::");
            }

            if (value == null)
            {
                builder.Append("null");

                goto FinishElementProcessing;
            }
            else if (ReferenceEquals(value, DBNull.Value))
            {
                builder.Append("db_null");

                goto FinishElementProcessing;
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
            {
                builder.Append("null");

                goto FinishElementProcessing;
            }
            else if (valueString.Length == 0)
            {
                goto FinishElementProcessing;
            }
            else if (valueString == "db_null")
            {
                builder.Append("db_null");

                goto FinishElementProcessing;
            }

            valueString = EncodeRepresent(
                valueString);

            builder.Append(valueString);

            // Label
            FinishElementProcessing:



            builder.Append('"');
        }
        private static void ToStringRepresent<TValue>(
            ref StringBuilder builder, TValue[] value,
            string key = null)
        {
            builder.Append('[');

            if (key != null)
            {
                ToStringRepresent(ref builder, key);

                builder.Append("::");
            }

            if (value == null)
            {
                builder.Append("null");

                goto FinishArrayProcessing;
            }
            else if (value.Length == 0)
            {
                goto FinishArrayProcessing;
            }

            foreach (var element in value)
            {
                ToStringRepresent(ref builder, element);

                builder.Append(',');
            }

            builder.Remove(builder.Length - 1, 1);

            // Label
            FinishArrayProcessing:



            builder.Append(']');
        }
        // ReSharper disable PossibleNullReferenceException
        public static string ToStringRepresent<TValue>(
            INestableCollection<TValue> value)
        {
            var builder =
                new StringBuilder(value.Length * 20);
            var collectionFrames =
                new ChunkedArrayL<NestableCollectionStringifyFrame<TValue>>(0, 32);

            collectionFrames.Push(
                new NestableCollectionStringifyFrame<TValue>(value));

            ref var previousCollectionInfo = ref collectionFrames.PeekRef();
            ref var previousCollection = ref previousCollectionInfo.Collection;
            ref var previousIndex = ref previousCollectionInfo.Index;
            ref var previousGeneralType = ref previousCollectionInfo.GeneralType;

            var previousDictionary = previousGeneralType == CollectionGeneralType.Dictionary
                ? (INestableDictionary<TValue>)previousCollection
                : null;

            // Label
            StartNextCollectionProcessing:



            ref var currentCollectionInfo = ref collectionFrames.PeekRef();
            ref var currentCollection = ref currentCollectionInfo.Collection;
            ref var currentIndex = ref currentCollectionInfo.Index;
            ref var currentGeneralType = ref currentCollectionInfo.GeneralType;

            var currentCollectionIsDictionary = currentGeneralType == CollectionGeneralType.Dictionary;
            var currentDictionary = currentCollectionIsDictionary
                ? (INestableDictionary<TValue>)currentCollection
                : null;

            currentIndex = 0;

            switch (currentGeneralType)
            {
                case CollectionGeneralType.Array:
                case CollectionGeneralType.List:
                case CollectionGeneralType.Dictionary:
                    break;
                case CollectionGeneralType.Unknown:
                default:
                    var exception = new ArgumentException(
                        "Недопустимое значение CollectionGeneralType у коллекции",
                        nameof(currentCollection));
                    Events.OnError(
                        new RErrorEventArgs(exception, exception.Message));
                    OnError(
                        new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }

            var currentCollectionType = GetCollectionType(currentCollection);

            builder.Append('{');

            if (currentCollectionIsDictionary)
            {
                var key = currentDictionary.Key;

                ToStringRepresent(ref builder, key);

                builder.Append("::");
            }
            else if (previousGeneralType == CollectionGeneralType.Dictionary)
            {
                var key = previousDictionary.GetKey(
                    previousIndex - 1);

                ToStringRepresent(ref builder, key);

                builder.Append("::");
            }

            builder.Append(currentCollectionType);
            builder.Append("||");

            if (currentCollection.Length == 0)
                goto FinishCollectionProcessing;

            // Label
            ContinuePreviousCollectionProcessing:



            while (currentIndex < currentCollection.Length)
            {
                ref var currentElement = ref currentCollection.GetRef(currentIndex);

                if (currentElement.Type == NestedType.Element)
                {
                    if (currentCollectionIsDictionary)
                    {
                        ToStringRepresent(
                            ref builder,
                            currentElement.GetElement(),
                            currentDictionary.GetKey(currentIndex));
                    }
                    else
                    {
                        ToStringRepresent(
                            ref builder,
                            currentElement.GetElement());
                    }

                    builder.Append(',');
                }
                else if (currentElement.Type == NestedType.Array)
                {
                    if (currentCollectionIsDictionary)
                    {
                        ToStringRepresent(
                            ref builder,
                            currentElement.GetArray(),
                            currentDictionary.GetKey(currentIndex));
                    }
                    else
                    {
                        ToStringRepresent(
                            ref builder,
                            currentElement.GetArray());
                    }

                    builder.Append(',');
                }
                else if (currentElement.Type == NestedType.Collection)
                {
                    var collection = currentElement.GetCollection();

                    collectionFrames.Push(
                        new NestableCollectionStringifyFrame<TValue>(collection));

                    ++currentIndex;

                    previousCollectionInfo = ref currentCollectionInfo;
                    previousCollection = ref previousCollectionInfo.Collection;
                    previousIndex = ref previousCollectionInfo.Index;
                    previousGeneralType = ref previousCollectionInfo.GeneralType;

                    previousDictionary = previousGeneralType == CollectionGeneralType.Dictionary
                        ? (INestableDictionary<TValue>)previousCollection
                        : null;

                    goto StartNextCollectionProcessing;
                }

                ++currentIndex;
            }

            builder.Remove(builder.Length - 1, 1);

            // Label
            FinishCollectionProcessing:



            builder.Append('}');
            builder.Append(',');

            _ = collectionFrames.Pop();

            if (collectionFrames.IsEmpty())
            {
                if (builder[builder.Length - 1] == ',')
                    builder.Remove(builder.Length - 1, 1);

                return builder.ToString();
            }

            currentCollectionInfo = ref collectionFrames.PeekRef();
            currentCollection = ref currentCollectionInfo.Collection;
            currentIndex = ref currentCollectionInfo.Index;
            currentGeneralType = ref currentCollectionInfo.GeneralType;

            currentCollectionIsDictionary = currentGeneralType == CollectionGeneralType.Dictionary;
            currentDictionary = currentCollectionIsDictionary
                ? (INestableDictionary<TValue>)currentCollection
                : null;

            goto ContinuePreviousCollectionProcessing;
        }
        // ReSharper restore PossibleNullReferenceException



        private static string FromStringRepresent(
            ReadOnlySpan<char> key)
        {
            if (key == null)
                return null;
            else if (key.SequenceEqual("null"))
                return null;
            else if (key.Length == 0)
                return string.Empty;

            return DecodeRepresent(
                key);
        }
        // ReSharper disable RedundantAssignment
        private static (string Key, TValue Value) FromStringRepresent<TValue>(
            ReadOnlySpan<char> represent, ref TValue value,
            bool includingKey = false,
            bool includingQuotes = true)
        {
            if (includingQuotes)
            {
                if (represent[0] != '"' || represent[^1] != '"')
                {
                    var exception = new ArgumentException(
                        $"Неверный формат строки[{0}, {represent.Length - 1}] для преобразования в элемент",
                        nameof(represent));
                    Events.OnError(
                        new RErrorEventArgs(exception, exception.Message));
                    OnError(
                        new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }

                represent = represent
                    .Slice(1, represent.Length - 2);
            }

            string key = null;

            if (includingKey)
            {
                var keyDivide = represent
                    .IndexOf(KeyDivide);

                if (keyDivide != -1)
                {
                    key = FromStringRepresent(
                        represent[..keyDivide]);
                    represent = represent[(keyDivide + 2)..];
                }
            }

            string valueString;

            if (represent.Length == 0)
            {
                if (typeof(TValue) == typeof(string))
                {
                    valueString = string.Empty;

                    goto FinishElementProcessing;
                }

                value = default;

                return (key, value);
            }
            else if (represent.SequenceEqual("null"))
            {
                value = default;

                return (key, value);
            }
            else if (represent.SequenceEqual("db_null"))
            {
                if (typeof(TValue) == typeof(string))
                {
                    valueString = "db_null";

                    goto FinishElementProcessing;
                }

                value = (TValue)Convert.ChangeType(DBNull.Value,
                    typeof(TValue), CultureInfo.InvariantCulture);

                return (key, value);
            }

            valueString = DecodeRepresent(
                represent);

            // Label
            FinishElementProcessing:



            value = (TValue)Convert.ChangeType(valueString,
                typeof(TValue), CultureInfo.InvariantCulture);

            return (key, value);
        }
        // ReSharper restore RedundantAssignment
        private static (string Key, TValue[] Value) FromStringRepresent<TValue>(
            ReadOnlySpan<char> represent, ref TValue[] value,
            bool includingKey = false)
        {
            if (represent[0] != '[' || represent[^1] != ']')
            {
                var exception = new ArgumentException(
                    $"Неверный формат строки[{0}, {represent.Length - 1}] для преобразования в массив",
                    nameof(represent));
                Events.OnError(
                    new RErrorEventArgs(exception, exception.Message));
                OnError(
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            represent = represent
                .Slice(1, represent.Length - 2);

            string key = null;

            if (includingKey)
            {
                var keyDivide = represent
                    .IndexOf(KeyDivide);

                if (keyDivide != -1)
                {
                    key = FromStringRepresent(
                        represent[..keyDivide]);
                    represent = represent[(keyDivide + 2)..];
                }
            }

            if (represent.Length == 0)
            {
                value = Array.Empty<TValue>();

                return (key, value);
            }
            else if (represent.SequenceEqual("null"))
            {
                value = null;

                return (key, value);
            }

            var values = new ChunkedArrayL<TValue>();
            TValue result;

            var valueDivideIndex = represent
                .IndexOf("\",\"");

            while (valueDivideIndex != -1)
            {
                result = default;
                result = FromStringRepresent(represent[1..valueDivideIndex],
                    ref result, false, false).Value;

                values.Add(
                    result);

                represent = represent[(valueDivideIndex + 2)..];

                valueDivideIndex = represent
                    .IndexOf("\",\"");
            }

            result = default;
            result = FromStringRepresent(represent[1..^1],
                ref result, false, false).Value;

            values.Add(
                result);

            value = new TValue[values.Length];

            values.CopyTo(value, 0);

            return (key, value);
        }
        public static INestableCollection<TValue> FromStringRepresent<TValue>(
            ReadOnlySpan<char> represent)
        {
            if (represent[0] != '{' || represent[represent.Length - 1] != '}')
            {
                var exception = new ArgumentException(
                    $"Неверный формат строки[{0}, {represent.Length - 1}] для преобразования в коллекцию с поддержкой вложенности",
                    nameof(represent));
                Events.OnError(
                    new RErrorEventArgs(exception, exception.Message));
                OnError(
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            var typeDivideSpan = TypeDivide
                .AsSpan();
            var keyDivideSpan = KeyDivide
                .AsSpan();

            ReadOnlySpan<char> type;

            var typeDivide = represent[..^1]
                .IndexOf(typeDivideSpan);
            var keyDivide = represent[..(typeDivide - 1)]
                .IndexOf(keyDivideSpan);

            if (keyDivide != -1)
            {
                type = represent
                    .Slice(keyDivide + 2,
                        typeDivide - keyDivide - 2);
            }
            else
            {
                type = represent
                    .Slice(1,
                        typeDivide - 1);
            }

            var collection = CreateCollectionByType<TValue>(
                    GetCollectionType(type))
                .Collection;

            return FromStringRepresent(represent, collection);
        }
        // ReSharper disable PossibleNullReferenceException
        public static INestableCollection<TValue> FromStringRepresent<TValue>(
            ReadOnlySpan<char> represent, INestableCollection<TValue> value)
        {
            value.Clear();

            var typeDivideSpan = TypeDivide
                .AsSpan();
            var keyDivideSpan = KeyDivide
                .AsSpan();

            var collectionFrames =
                new ChunkedArrayL<NestableCollectionUnstringifyFrame<TValue>>(0, 32);

            collectionFrames.Push(
                new NestableCollectionUnstringifyFrame<TValue>(
                    value, 0, represent.Length - 1));

            // Label
            StartNextCollectionProcessing:



            ref var currentCollectionInfo = ref collectionFrames.PeekRef();
            ref var currentCollection = ref currentCollectionInfo.Collection;
            ref var currentStartIndex = ref currentCollectionInfo.StartIndex;
            ref var currentEndIndex = ref currentCollectionInfo.EndIndex;
            ref var currentLength = ref currentCollectionInfo.Length;
            ref var currentDivideIndex = ref currentCollectionInfo.DivideIndex;
            ref var currentGeneralType = ref currentCollectionInfo.GeneralType;

            var currentCollectionIsDictionary = currentGeneralType == CollectionGeneralType.Dictionary;
            var currentDictionary = currentCollectionIsDictionary
                ? (INestableDictionary<TValue>)currentCollection
                : null;

            if (represent[currentStartIndex] != '{' || represent[currentEndIndex] != '}')
            {
                var exception = new ArgumentException(
                    $"Неверный формат строки[{currentStartIndex}, {currentEndIndex}] для преобразования в коллекцию с поддержкой вложенности",
                    nameof(represent));
                Events.OnError(
                    new RErrorEventArgs(exception, exception.Message));
                OnError(
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            switch (currentGeneralType)
            {
                case CollectionGeneralType.Array:
                case CollectionGeneralType.List:
                case CollectionGeneralType.Dictionary:
                    break;
                case CollectionGeneralType.Unknown:
                default:
                    var exception = new ArgumentException(
                        "Недопустимое значение CollectionGeneralType у коллекции",
                        nameof(currentCollection));
                    Events.OnError(
                        new RErrorEventArgs(exception, exception.Message));
                    OnError(
                        new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }

            ReadOnlySpan<char> type;

            var typeDivide = represent
                .Slice(currentStartIndex,
                    currentLength - 1)
                .IndexOf(typeDivideSpan);
            var keyDivide = represent
                .Slice(currentStartIndex,
                    typeDivide - 1)
                .IndexOf(keyDivideSpan);

            if (keyDivide != -1)
            {
                var key = FromStringRepresent(represent
                    .Slice(currentStartIndex + 1,
                        keyDivide - 1));

                if (currentCollectionIsDictionary)
                    currentDictionary.Key = key;

                type = represent
                    .Slice(currentStartIndex + keyDivide + 2,
                        typeDivide - keyDivide - 2);
            }
            else
            {
                type = represent
                    .Slice(currentStartIndex + 1,
                        typeDivide - 1);
            }

            currentDivideIndex = currentStartIndex + typeDivide + 1;

            if (GetCollectionType(type) != GetCollectionType(currentCollection))
            {
                var exception = new ArgumentException(
                    "Тип переданной коллекции не соответствует типу коллекции из строкового представления",
                    nameof(currentCollection));
                Events.OnError(
                    new RErrorEventArgs(exception, exception.Message));
                OnError(
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (currentStartIndex + typeDivide + 2 == currentEndIndex)
                goto FinishCollectionProcessing;

            // Label
            ContinuePreviousCollectionProcessing:



            if (currentDivideIndex < currentStartIndex + 1
                || currentDivideIndex > currentEndIndex - 1)
            {
                goto FinishCollectionProcessing;
            }

            do
            {
                var (partType, partStartIndex, partEndIndex, partLength) = GetRepresentPart(
                    represent, currentDivideIndex + 1);

                if (partType == NestedType.Element)
                {
                    var partRepresent = represent.Slice(
                        partStartIndex + 1, partEndIndex - partStartIndex - 1);

                    var result = default(TValue);
                    var (resultKey, resultValue) = FromStringRepresent(
                        partRepresent, ref result,
                        currentCollectionIsDictionary,
                        false);

                    if (currentCollectionIsDictionary)
                    {
                        currentDictionary.Add(
                            resultKey,
                            resultValue);
                    }
                    else
                    {
                        currentCollection.Add(
                            resultValue);
                    }
                }
                else if (partType == NestedType.Array)
                {
                    var partRepresent = represent.Slice(
                        partStartIndex, partEndIndex - partStartIndex + 1);

                    var result = Array.Empty<TValue>();
                    var (resultKey, resultValue) = FromStringRepresent(
                        partRepresent, ref result,
                        currentCollectionIsDictionary);

                    if (currentCollectionIsDictionary)
                    {
                        currentDictionary.Add(
                            resultKey,
                            resultValue);
                    }
                    else
                    {
                        currentCollection.Add(
                            resultValue);
                    }
                }
                else if (partType == NestedType.Collection)
                {
                    string resultKey = null;
                    ReadOnlySpan<char> resultType;

                    var resultTypeDivide = represent
                        .Slice(partStartIndex,
                            partLength - 1)
                        .IndexOf(typeDivideSpan);
                    var resultKeyDivide = represent
                        .Slice(partStartIndex,
                            resultTypeDivide - 1)
                        .IndexOf(keyDivideSpan);

                    if (resultKeyDivide != -1)
                    {
                        resultKey = FromStringRepresent(represent
                            .Slice(partStartIndex + 1,
                                resultKeyDivide - 1));
                        resultType = represent
                            .Slice(partStartIndex + resultKeyDivide + 2,
                                resultTypeDivide - resultKeyDivide - 2);
                    }
                    else
                    {
                        resultType = represent
                            .Slice(partStartIndex + 1,
                                resultTypeDivide - 1);
                    }

                    var result = CreateCollectionByType<TValue>(
                            GetCollectionType(resultType))
                        .Collection;

                    if (currentCollectionIsDictionary)
                    {
                        currentDictionary.Add(
                            resultKey,
                            result);
                    }
                    else
                    {
                        currentCollection.Add(
                            result);
                    }

                    currentDivideIndex = partEndIndex + 1;

                    collectionFrames.Push(
                        new NestableCollectionUnstringifyFrame<TValue>(
                            result, partStartIndex, partEndIndex));

                    goto StartNextCollectionProcessing;
                }

                currentDivideIndex = partEndIndex + 1;
            } while (currentDivideIndex >= currentStartIndex + 1
                     && currentDivideIndex <= currentEndIndex - 1
                     && represent[currentDivideIndex] == ',');

            // Label
            FinishCollectionProcessing:



            var latestCollectionInfo = collectionFrames.Pop();

            if (collectionFrames.IsEmpty())
                return latestCollectionInfo.Collection;

            currentCollectionInfo = ref collectionFrames.PeekRef();
            currentCollection = ref currentCollectionInfo.Collection;
            currentStartIndex = ref currentCollectionInfo.StartIndex;
            currentEndIndex = ref currentCollectionInfo.EndIndex;
            currentLength = ref currentCollectionInfo.Length;
            currentDivideIndex = ref currentCollectionInfo.DivideIndex;
            currentGeneralType = ref currentCollectionInfo.GeneralType;

            currentCollectionIsDictionary = currentGeneralType == CollectionGeneralType.Dictionary;
            currentDictionary = currentCollectionIsDictionary
                ? (INestableDictionary<TValue>)currentCollection
                : null;

            goto ContinuePreviousCollectionProcessing;
        }
        // ReSharper restore PossibleNullReferenceException



        public static IEnumerable<TValue> Enumerate<TValue>(
            NestedElement<TValue> value)
        {
            switch (value.Type)
            {
                case NestedType.Element:
                    return new[] { value.GetElement() };
                case NestedType.Array:
                    var array = value.GetArray();

                    return array != null
                        ? value.GetArray()
                        : Array.Empty<TValue>();
                case NestedType.Collection:
                    return Enumerate(value.GetCollection());
                case NestedType.Unknown:
                default:
                    var exception = new ArgumentException(
                        "Недопустимое значение поля Type в [NestedElement] для старта перечисления",
                        nameof(value));
                    Events.OnError(
                        new RErrorEventArgs(exception, exception.Message));
                    OnError(
                        new RErrorEventArgs(exception, exception.Message));
                    throw exception;
            }
        }
        public static IEnumerable<TValue> Enumerate<TValue>(
            INestableCollection<TValue> value)
        {
            if (value == null)
                return Array.Empty<TValue>();

            var valueType =
                typeof(TValue);
            var result =
                new ChunkedArrayL<TValue>();
            var collectionFrames =
                new ChunkedArrayL<NestableCollectionEnumerateFrame<TValue>>(0, 32);

            collectionFrames.Push(
                new NestableCollectionEnumerateFrame<TValue>(value));

            // Label
            StartNextCollectionProcessing:



            ref var currentCollectionInfo = ref collectionFrames.PeekRef();
            ref var currentCollection = ref currentCollectionInfo.Collection;
            ref var currentIndex = ref currentCollectionInfo.Index;

            currentIndex = 0;

            if (currentCollection.Length == 0)
                goto FinishCollectionProcessing;

            // Label
            ContinuePreviousCollectionProcessing:



            while (currentIndex < currentCollection.Length)
            {
                ref var currentElement = ref currentCollection.GetRef(currentIndex);

                if (currentElement.Type == NestedType.Element)
                {
                    result.Add(currentElement.GetElement());
                }
                else if (currentElement.Type == NestedType.Array)
                {
                    var array = currentElement.GetArray();

                    if (array == null)
                    {
                        if (!valueType.IsValueType)
                            result.Add(default);

                        goto StartNewIteration;
                    }

                    foreach (var element in array)
                    {
                        result.Add(element);
                    }
                }
                else if (currentElement.Type == NestedType.Collection)
                {
                    var collection = currentElement.GetCollection();

                    if (collection == null)
                    {
                        if (!valueType.IsValueType)
                            result.Add(default);

                        goto StartNewIteration;
                    }

                    collectionFrames.Push(
                        new NestableCollectionEnumerateFrame<TValue>(collection));

                    ++currentIndex;

                    goto StartNextCollectionProcessing;
                }

                // Label
                StartNewIteration:



                ++currentIndex;
            }

            // Label
            FinishCollectionProcessing:



            _ = collectionFrames.Pop();

            if (collectionFrames.IsEmpty())
                return result;

            currentCollectionInfo = ref collectionFrames.PeekRef();
            currentCollection = ref currentCollectionInfo.Collection;
            currentIndex = ref currentCollectionInfo.Index;

            goto ContinuePreviousCollectionProcessing;
        }
    }
}
