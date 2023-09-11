// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RIS.Unions.Generator.Extensions;

namespace RIS.Unions.Generator
{
    internal static class Validator
    {
        private const string UnionBaseTypeNamespace = "RIS.Unions";
        private const string UnionBaseTypeName = "UnionBase";



        public static bool ValidateClass(
            this SourceProductionContext context,
            ITypeSymbol classSymbol)
        {
            var diagnostics = new List<Diagnostic>();
            var location = classSymbol.Locations.FirstOrDefault()
                          ?? Location.None;

            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
                diagnostics.Add(Diagnostic.Create(DiagnosticErrors.TopLevelError, location, classSymbol.Name));

            if (classSymbol.BaseType is null || classSymbol.BaseType.Name != UnionBaseTypeName || classSymbol.BaseType.ContainingNamespace.ToString() != UnionBaseTypeNamespace)
                diagnostics.Add(Diagnostic.Create(DiagnosticErrors.WrongBaseType, location, classSymbol.Name));

            //if (classSymbol.DeclaredAccessibility != Accessibility.Public)
            //    diagnostics.Add(Diagnostic.Create(DiagnosticErrors.ClassIsNotPublic, location, classSymbol.Name));

            return diagnostics.ReportIfAny(
                context);
        }

        public static bool ValidateTypeArguments(
            this SourceProductionContext context,
            ITypeSymbol classSymbol)
        {
            var diagnostics = new List<Diagnostic>();
            var typeArguments =
                classSymbol.BaseType!.TypeArguments;
            var location = classSymbol.Locations.FirstOrDefault()
                           ?? Location.None;

            foreach (var typeSymbol in typeArguments)
            {
                if (typeSymbol.Name == nameof(Object))
                    diagnostics.Add(Diagnostic.Create(DiagnosticErrors.ObjectIsUnionType, location, classSymbol.Name));

                if (typeSymbol.TypeKind == TypeKind.Interface)
                    diagnostics.Add(Diagnostic.Create(DiagnosticErrors.UserDefinedConversionsToOrFromAnInterfaceAreNotAllowed, location, classSymbol.Name));
            }

            return diagnostics.ReportIfAny(
                context);
        }

        public static bool ValidateTypeNames(
            this SourceProductionContext context,
            ITypeSymbol classSymbol,
            AttributeData attributeData)
        {
            var diagnostics = new List<Diagnostic>();
            var typeArguments =
                classSymbol.BaseType!.TypeArguments;
            var typeNames =
                attributeData.ConstructorArguments
                    .First()
                    .Values;
            var attributeSyntax =
                attributeData
                    .GetSyntax();
            var location = classSymbol.Locations.FirstOrDefault()
                           ?? Location.None;

            if (typeNames.Length > 0 && typeNames.Length != typeArguments.Length)
                diagnostics.Add(Diagnostic.Create(DiagnosticErrors.WrongNumberOfTypeNames, location, typeArguments.Length, typeNames.Length));

            var typeNameLocations = attributeSyntax?.ArgumentList?.Arguments
                .Where(x => x.NameEquals == null)
                .Select(x => x.Expression.GetLocation())
                .ToArray();

            foreach (var (name, nameIndex) in typeNames.Select((x, i) => (x, i)))
            {
                var typeNameLocation = nameIndex < typeNameLocations?.Length
                    ? typeNameLocations[nameIndex]
                    : location;

                if (string.IsNullOrEmpty(name.Value as string))
                    diagnostics.Add(Diagnostic.Create(DiagnosticErrors.TypeNameCannotBeNullOrEmpty, typeNameLocation, nameIndex + 1));

                if (!SyntaxFacts.IsValidIdentifier("_" + name.Value))
                    diagnostics.Add(Diagnostic.Create(DiagnosticErrors.InvalidTypeName, typeNameLocation, name.Value));
            }

            return diagnostics.ReportIfAny(
                context);
        }
    }
}
