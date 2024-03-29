﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static System.IO.Path;
using static System.Reflection.Assembly;
using static System.Linq.Enumerable;



var sourceRoot = GetFullPath(Combine(
    GetDirectoryName(GetExecutingAssembly().Location)!,
    @"..\..\..\..\.."));

for (var i = 1; i < 10; ++i)
{
    var content1 = GetContent(
        true, i);
    var path1 = Combine(sourceRoot,
        $@"RIS\Unions\UnionT{i - 1}.generated.cs");

    File.WriteAllText(
        path1, content1,
        Encoding.UTF8);

    var content2 = GetContent(
        false, i);
    var path2 = Combine(sourceRoot,
        $@"RIS\Unions\UnionBaseT{i - 1}.generated.cs");

    File.WriteAllText(
        path2, content2,
        Encoding.UTF8);
}

for (var i = 10; i < 33; ++i)
{
    var content1 = GetContent(
        true, i);
    var path1 = Combine(sourceRoot,
        $@"RIS\Unions\UnionT{i - 1}.generated.cs");

    File.WriteAllText(
        path1, content1,
        Encoding.UTF8);

    var content2 = GetContent(
        false, i);
    var path2 = Combine(sourceRoot,
        $@"RIS\Unions\UnionBaseT{i - 1}.generated.cs");

    File.WriteAllText(
        path2, content2,
        Encoding.UTF8);
}



string GetContent(bool isStruct, int i)
{
    string RangeJoined(string delimiter, Func<int, string> selector) =>
        Range(0, i).Joined(delimiter, selector);
    string IfStruct(string s1, string s2 = "") =>
        isStruct ? s1 : s2;

    var className =
        isStruct ? "Union" : "UnionBase";
    var genericArgs = Range(0, i)
        .Select(e => $"T{e}")
        .ToList();
    var genericArg = genericArgs
        .Joined(", ");
    var sb = new StringBuilder();

    sb.Append(@$"// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Threading.Tasks;
using static RIS.Unions.UnionsHelper;

namespace RIS.Unions
{{
    public {IfStruct("struct", "class")} {className}<{genericArg}> : IUnion
    {{
        {RangeJoined(@"
        ", j => $"readonly T{j} _value{j};")}
        readonly int _index;

        {IfStruct( // constructor
        $@"Union(int index, {RangeJoined(", ", j => $"T{j} value{j} = default")})
        {{
            _index = index;
            {RangeJoined(@"
            ", j => $"_value{j} = value{j};")}
        }}",
        $@"protected UnionBase(Union<{genericArg}> input)
        {{
            _index = input.Index;
            switch (_index)
            {{
                {RangeJoined($@"
                ", j => $"case {j}: _value{j} = input.AsT{j}; break;")}
                default: throw new InvalidOperationException();
            }}
        }}"
        )}

        public object Value =>
            _index switch
            {{
                {RangeJoined(@"
                ", j => $"{j} => _value{j},")}
                _ => throw new InvalidOperationException()
            }};

        public int Index => _index;

        {RangeJoined(@"
        ", j=> $"public bool IsT{j} => _index == {j};")}

        {RangeJoined(@"
        ", j => $@"public T{j} AsT{j} =>
            _index == {j} ?
                _value{j} :
                throw new InvalidOperationException($""Cannot return as T{j} as result is T{{_index}}"");")}

        {IfStruct(RangeJoined(@"
        ", j => $"public static implicit operator {className}<{genericArg}>(T{j} t) => new {className}<{genericArg}>({j}, value{j}: t);"))}

        public void Switch({RangeJoined(", ", e => $"Action<T{e}> f{e}")})
        {{
            {RangeJoined(@"
            ", j => @$"if (_index == {j} && f{j} != null)
            {{
                f{j}(_value{j});
                return;
            }}")}
            throw new InvalidOperationException();
        }}

        public Task Switch({RangeJoined(", ", e => $"Func<T{e}, Task> f{e}")})
        {{
            {RangeJoined(@"
            ", j => @$"if (_index == {j} && f{j} != null)
            {{
                return f{j}(_value{j});
            }}")}
            throw new InvalidOperationException();
        }}

        public TResult Match<TResult>({RangeJoined(", ", e => $"Func<T{e}, TResult> f{e}")})
        {{
            {RangeJoined(@"
            ", j => $@"if (_index == {j} && f{j} != null)
            {{
                return f{j}(_value{j});
            }}")}
            throw new InvalidOperationException();
        }}

        {IfStruct(genericArgs.Joined(@"
        ", bindToType => $@"public static Union<{genericArgs.Joined(", ")}> From{bindToType}({bindToType} input) => input;"))}

        {IfStruct(genericArgs.Joined(@"
            ", bindToType => {
            var resultArgsPrinted = genericArgs.Select(x => {
                return x == bindToType ? "TResult" : x;
            }).Joined(", ");
            return $@"
        public Union<{resultArgsPrinted}> Map{bindToType}<TResult>(Func<{bindToType}, TResult> mapFunc)
        {{
            if (mapFunc == null)
            {{
                throw new ArgumentNullException(nameof(mapFunc));
            }}
            return _index switch
            {{
                {genericArgs.Joined(@"
                ", (x, k) =>
                    x == bindToType ?
                        $"{k} => mapFunc(As{x})," :
                        $"{k} => As{x},")}
                _ => throw new InvalidOperationException()
            }};
        }}";
        }))}
");

    if (i > 1) {
        sb.AppendLine(
            RangeJoined(@"
        ", j => {
                var genericArgWithSkip = Range(0, i).ExceptSingle(j).Joined(", ", e => $"T{e}");
                var remainderType = i == 2 ? genericArgWithSkip : $"Union<{genericArgWithSkip}>";
                return $@"
        public bool TryPickT{j}(out T{j} value, out {remainderType} remainder)
        {{
            value = IsT{j} ? AsT{j} : default;
            remainder = _index switch
            {{
                {RangeJoined(@"
                ", k => 
                    k == j ?
                        $"{k} => default," :
                        $"{k} => AsT{k},")}
                _ => throw new InvalidOperationException()
            }};
            return this.IsT{j};
        }}";
            })
        );
    }

    sb.AppendLine($@"
        bool Equals({className}<{genericArg}> other) =>
            _index == other._index &&
            _index switch
            {{
                {RangeJoined(@"
                ", j => @$"{j} => Equals(_value{j}, other._value{j}),")}
                _ => false
            }};

        public override bool Equals(object obj)
        {{
            if (ReferenceEquals(null, obj))
            {{
                return false;
            }}

            {IfStruct(
            $"return obj is Union<{genericArg}> o && Equals(o);",
            $@"if (ReferenceEquals(this, obj)) {{
                    return true;
            }}

            return obj is UnionBase<{genericArg}> o && Equals(o);"
            )}
        }}

        public override string ToString() =>
            _index switch {{
                {RangeJoined(@"
                ", j => $"{j} => FormatValue(_value{j}),")}
                _ => throw new InvalidOperationException(""Unexpected index, which indicates a problem in the Union codegen."")
            }};

        public override int GetHashCode()
        {{
            unchecked
            {{
                int hashCode = _index switch
                {{
                    {RangeJoined(@"
                    ", j => $"{j} => _value{j}?.GetHashCode(),")}
                    _ => 0
                }} ?? 0;
                return (hashCode*397) ^ _index;
            }}
        }}
    }}
}}");

    return sb.ToString();
}



public static class Extensions
{
    public static string Joined<T>(this IEnumerable<T>? source,
        string delimiter, Func<T, string>? selector = null)
    {
        if (source == null)
            return "";
        if (selector == null)
            return string.Join(delimiter, source);

        return string.Join(delimiter,
            source.Select(selector));
    }

    public static string Joined<T>(this IEnumerable<T>? source,
        string delimiter, Func<T, int, string> selector)
    {
        if (source == null)
            return "";

        return string.Join(delimiter,
            source.Select(selector));
    }

    public static IEnumerable<T> ExceptSingle<T>(
        this IEnumerable<T>? source, T single)
    {
        if (source == null)
            return Empty<T>();

        return source.Except(
            Repeat(single, 1));
    }

    public static void AppendLineTo(
        this string? source, StringBuilder builder)
    {
        builder.AppendLine(
            source);
    }
}