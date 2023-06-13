﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RIS.Localization.LocalizedList.Generator
{
    [Generator]
    public class LocalizedListGenerator : ISourceGenerator
    {
        private const string LocalizedListBaseTypeNamespace = "RIS.Localization";
        private const string LocalizedListBaseTypeName = "LocalizedListBase";



        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() =>
                new LocalizedListSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not LocalizedListSyntaxReceiver receiver)
                return;
            if ((context.Compilation as CSharpCompilation)?.SyntaxTrees[0].Options is not CSharpParseOptions options)
                return;

            var compilation =
                context.Compilation;

            List<(INamedTypeSymbol, ClassDeclarationSyntax?)> classSymbols = new();

            foreach (var classDeclaration in receiver.CandidateClasses)
            {
                var model = compilation.GetSemanticModel(
                    classDeclaration.SyntaxTree);
                var classSymbol = model.GetDeclaredSymbol(
                    classDeclaration);
                var baseClassSymbol = classSymbol;

                do
                {
                    baseClassSymbol = baseClassSymbol?.BaseType;
                } while (baseClassSymbol?.BaseType != null
                         && baseClassSymbol.BaseType?.SpecialType != SpecialType.System_Object);

                if (classSymbol is null
                    || classSymbol.IsAbstract
                    || baseClassSymbol is null
                    || baseClassSymbol.Name != LocalizedListBaseTypeName
                    || baseClassSymbol.ContainingNamespace.ToString() != LocalizedListBaseTypeNamespace)
                {
                    continue;
                }

                classSymbols.Add((classSymbol!, classDeclaration));
            }

            foreach (var (classSymbol, classDeclaration) in classSymbols)
            {
                var classSource = ProcessClass(
                    classSymbol, context, classDeclaration!);

                if (classSource is null)
                    continue;

                context.AddSource($"{classSymbol.ContainingNamespace}_{classSymbol.Name}.g.cs",
                    SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private static string? ProcessClass(INamedTypeSymbol classSymbol,
            GeneratorExecutionContext context, ClassDeclarationSyntax classDeclaration)
        {
            if (!context.ValidateClass(classSymbol, classDeclaration))
                return null;

            return GenerateClassSource(classSymbol);
        }

        private static string GenerateClassSource(INamedTypeSymbol classSymbol)
        {
            var classNameWithGenericTypes =
                $"{classSymbol.Name}{GetOpenGenericPart(classSymbol)}";

            StringBuilder source = new($@"// <auto-generated />

#pragma warning disable 1591

using System;
using System.ComponentModel;
using {LocalizedListBaseTypeNamespace};

namespace {classSymbol.ContainingNamespace.ToDisplayString()}
{{
    partial class {classNameWithGenericTypes}
    {{
        {RoslynFactory.CreateEventStaticPropertyChanged()}
");

            source.Append($@"


        {RoslynFactory.CreateInstanceProperty(classSymbol)}
");

            source.Append(@"    }
}");

            return source.ToString();
        }

        private static string GetGenericPart(
            ImmutableArray<ITypeSymbol> typeArguments)
        {
            return string.Join(", ",
                typeArguments.Select(x => x.ToDisplayString()));
        }

        private static string? GetOpenGenericPart(
            INamedTypeSymbol classSymbol)
        {
            if (!classSymbol.TypeArguments.Any())
                return null;

            return $"<{GetGenericPart(classSymbol.TypeArguments)}>";
        }
    }
}
