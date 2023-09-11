// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using RIS.Localization.LocalizedList.Generator.Extensions;

namespace RIS.Localization.LocalizedList.Generator
{
    internal static class Validator
    {
        public static bool ValidateClass(
            this SourceProductionContext context,
            ITypeSymbol classSymbol)
        {
            var diagnostics = new List<Diagnostic>();
            var location = classSymbol.Locations.FirstOrDefault()
                           ?? Location.None;

            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
                diagnostics.Add(Diagnostic.Create(DiagnosticErrors.TopLevelError, location, classSymbol.Name));

            //if (classSymbol.DeclaredAccessibility != Accessibility.Public)
            //    diagnostics.Add(Diagnostic.Create(DiagnosticErrors.ClassIsNotPublic, location, classSymbol.Name));

            var defaultConstructor = classSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .FirstOrDefault(symbol =>
                    symbol.MethodKind == MethodKind.Constructor
                    && symbol.Parameters.IsEmpty);

            if (defaultConstructor is null)
                diagnostics.Add(Diagnostic.Create(DiagnosticErrors.ClassShouldHaveDefaultConstructor, location, classSymbol.Name));

            if (defaultConstructor is not null && defaultConstructor.DeclaredAccessibility != Accessibility.Private)
                diagnostics.Add(Diagnostic.Create(DiagnosticErrors.DefaultConstructorIsNotPrivate, location, classSymbol.Name));

            return diagnostics.ReportIfAny(
                context);
        }
    }
}
