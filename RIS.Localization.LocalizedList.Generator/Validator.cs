using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RIS.Localization.LocalizedList.Generator.Extensions;

namespace RIS.Localization.LocalizedList.Generator
{
    internal static class Validator
    {
        public static bool ValidateClass(
            this GeneratorExecutionContext context,
            ITypeSymbol classSymbol,
            ClassDeclarationSyntax classDeclaration)
        {
            var diagnostics = new List<Diagnostic>();
            var classLocation =
                classDeclaration
                    .GetLocation();

            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
                diagnostics.Add(Diagnostic.Create(DiagnosticErrors.TopLevelError, classLocation, classSymbol.Name));

            if (classSymbol.DeclaredAccessibility != Accessibility.Public)
                diagnostics.Add(Diagnostic.Create(DiagnosticErrors.ClassIsNotPublic, classLocation, classSymbol.Name));

            if (!classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                diagnostics.Add(Diagnostic.Create(DiagnosticErrors.ClassIsNotPartial, classLocation, classSymbol.Name));

            var defaultConstructor = classSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .FirstOrDefault(symbol =>
                    symbol.MethodKind == MethodKind.Constructor
                    && symbol.Parameters.IsEmpty);

            if (defaultConstructor is null)
                diagnostics.Add(Diagnostic.Create(DiagnosticErrors.ClassShouldHaveDefaultConstructor, classLocation, classSymbol.Name));

            if (defaultConstructor is not null && defaultConstructor.DeclaredAccessibility != Accessibility.Private)
                diagnostics.Add(Diagnostic.Create(DiagnosticErrors.DefaultConstructorIsNotPrivate, classLocation, classSymbol.Name));

            return diagnostics.ReportIfAny(
                context);
        }
    }
}
