using System;
using System.Collections.Generic;
using System.Text;

namespace RIS.Collections.NestableCollections
{
    public static class NestableCollectionHelper
    {
        public static string ToStringRepresent<TV>(NestedElement<TV> value)
        {
            switch (value.Type)
            {
                case NestedType.Element:
                    return ToStringRepresent(value.GetElement());
                case NestedType.Array:
                    return ToStringRepresent(value.GetArray());
                case NestedType.NestableCollection:
                    return ToStringRepresent(value.GetNestableCollection());
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение поля Type в [NestedElement] для создания строкового представления", nameof(value));
                    Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
            }
        }
        public static string ToStringRepresent<TV>(TV value)
        {
            if (value == null)
                return string.Empty;

            string valueString = value.ToString()
                .Replace("\"", "/\"/")
                .Replace("[", "/[/")
                .Replace("]", "/]/")
                .Replace("{", "/{/")
                .Replace("}", "/}/");

            return valueString;
        }
        public static string ToStringRepresent<TV>(TV[] value)
        {
            if (value.Length == 0)
                return "[]";

            StringBuilder result = new StringBuilder();

            result.Append("[");

            for (int i = 0; i < value.Length; ++i)
            {
                result.Append("\"");
                result.Append(ToStringRepresent<TV>(value[i]));
                result.Append("\",");
            }

            if (result[result.Length - 1] == ',')
                result.Remove(result.Length - 1, 1);

            result.Append("]");

            return result.ToString();
        }
        public static string ToStringRepresent<TV>(INestableCollection<TV> value)
        {
            if (value.Length == 0)
                return "{}";

            StringBuilder result = new StringBuilder();

            result.Append("{");

            for (int i = 0; i < value.Length; ++i)
            {
                switch (value[i].Type)
                {
                    case NestedType.Element:
                        result.Append("\"");
                        result.Append(ToStringRepresent<TV>(value[i].GetElement()));
                        result.Append("\",");
                        break;
                    case NestedType.Array:
                        result.Append(ToStringRepresent<TV>(value[i].GetArray()));
                        result.Append(",");
                        break;
                    case NestedType.NestableCollection:
                        result.Append(ToStringRepresent<TV>(value[i].GetNestableCollection()));
                        result.Append(",");
                        break;
                }
            }

            if (result[result.Length - 1] == ',')
                result.Remove(result.Length - 1, 1);

            result.Append("}");

            return result.ToString();
        }


        public static TV FromStringRepresent<TV>(string represent, ref TV value)
        {
            //if (represent == string.Empty)
            //{
            //    value = default(TV);
            //    return value;
            //}

            string valueString = represent
                .Replace( "/\"/", "\"")
                .Replace("/[/", "[")
                .Replace("/]/", "]")
                .Replace("/{/", "{")
                .Replace("/}/", "}");

            value = (TV)Convert.ChangeType(valueString, typeof(TV));

            return value;
        }
        public static TV[] FromStringRepresent<TV>(string represent, ref TV[] value)
        {
            if (represent == "[]")
            {
                value = Array.Empty<TV>();
                return value;
            }

            if (represent[0] != '[' || represent[represent.Length - 1] != ']')
            {
                var exception =
                    new ArgumentException("Неверный формат строки для преобразования в массив" + " " + represent, nameof(represent));
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            string[] values = represent.Substring(2, represent.Length - 4).Split(new[] { "\",\"" }, StringSplitOptions.None);

            value = Array.ConvertAll<string, TV>(values, new Converter<string, TV>(delegate (string stringValue)
            {
                //return (TV)Convert.ChangeType(stringValue, typeof(TV));
                TV result = default(TV);
                return FromStringRepresent<TV>(stringValue, ref result);
            }));

            return value;
        }
        public static INestableCollection<TV> FromStringRepresent<TV>(string represent, INestableCollection<TV> value)
        {
            return FromStringRepresent<TV, NestableListL<TV>>(represent, value);
        }
        public static INestableCollection<TV> FromStringRepresent<TV, TC>(string represent, INestableCollection<TV> value) 
            where TC: INestableCollection<TV>, new()
        {
            if (represent[0] != '{' || represent[represent.Length - 1] != '}')
            {
                var exception =
                    new ArgumentException("Неверный формат строки для преобразования в коллекцию с поддержкой вложенности", nameof(represent));
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            value.Clear();

            if (represent == "{}")
                return value;

            int divideIndex = 0;
            do
            {
                if (represent[divideIndex + 1] == '\"')
                {
                    int startIndex = represent.IndexOf("\",", divideIndex + 1, StringComparison.Ordinal);

                    if (startIndex == -1)
                        startIndex = represent.IndexOf("\"}", divideIndex + 1, StringComparison.Ordinal);

                    string representSub = represent.Substring(divideIndex + 2, startIndex - (divideIndex + 2));

                    TV result = default(TV);
                    value.Add(FromStringRepresent<TV>(representSub, ref result));

                    divideIndex = represent.IndexOf("\",", divideIndex + 2, StringComparison.Ordinal);
                }
                else if (represent[divideIndex + 1] == '[')
                {
                    int startIndex = represent.IndexOf("],", divideIndex + 1, StringComparison.Ordinal);

                    if (startIndex == -1)
                        startIndex = represent.IndexOf("]}", divideIndex + 1, StringComparison.Ordinal);

                    string representSub = represent.Substring(divideIndex + 1, startIndex - divideIndex);

                    TV[] result = Array.Empty<TV>();
                    value.Add(FromStringRepresent<TV>(representSub, ref result));

                    divideIndex = represent.IndexOf("],", divideIndex + 2, StringComparison.Ordinal);
                }
                else if (represent[divideIndex + 1] == '{')
                {
                    int startIndex = represent.IndexOf("},", divideIndex + 1, StringComparison.Ordinal);

                    if (startIndex == -1)
                        startIndex = represent.IndexOf("}}", divideIndex + 1, StringComparison.Ordinal);

                    string representSub = represent.Substring(divideIndex + 1, startIndex - divideIndex);

                    INestableCollection < TV> result = new TC();
                    value.Add(FromStringRepresent<TV>(representSub, result));

                    divideIndex = represent.IndexOf("},", divideIndex + 2, StringComparison.Ordinal);
                }
            } while (divideIndex != -1
                     && divideIndex != represent.Length - 1
                     && (divideIndex = represent.IndexOf(',', divideIndex + 1)) != 0);

            return value;
        }


        public static IEnumerable<TV> Enumerate<TV>(NestedElement<TV> value)
        {
            switch (value.Type)
            {
                case NestedType.Element:
                    return Enumerate(value.GetElement());
                case NestedType.Array:
                    return Enumerate(value.GetArray());
                case NestedType.NestableCollection:
                    return Enumerate(value.GetNestableCollection());
                default:
                    var exception =
                        new ArgumentException("Недопустимое значение поля Type в [NestedElement] для старта перечисления", nameof(value));
                    Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
            }
        }
        public static IEnumerable<TV> Enumerate<TV>(TV value)
        {
            yield return value;
        }
        public static IEnumerable<TV> Enumerate<TV>(TV[] value)
        {
            for (int i = 0; i < value.Length; ++i)
            {
                yield return value[i];
            }
        }
        public static IEnumerable<TV> Enumerate<TV>(INestableCollection<TV> value)
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
                    case NestedType.NestableCollection:
                        foreach (var element in Enumerate(value[i].GetNestableCollection()))
                        {
                            yield return element;
                        }
                        break;
                }
            }
        }
    }
}
