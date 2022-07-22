using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RIS.Localization.LocalizedList.Generator.Extensions;

namespace RIS.Localization.LocalizedList.Generator
{
    internal static class Validator
    {
        private const string LocalizedListBaseTypeNamespace = "RIS.Localization";
        private const string LocalizedListBaseTypeName = "LocalizedListBase";



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

            if (classSymbol.BaseType is null || classSymbol.BaseType.Name != LocalizedListBaseTypeName || classSymbol.BaseType.ContainingNamespace.ToString() != LocalizedListBaseTypeNamespace)
                diagnostics.Add(Diagnostic.Create(DiagnosticErrors.WrongBaseType, classLocation, classSymbol.Name));

            if (classSymbol.DeclaredAccessibility != Accessibility.Public)
                diagnostics.Add(Diagnostic.Create(DiagnosticErrors.ClassIsNotPublic, classLocation, classSymbol.Name));

            return diagnostics.ReportIfAny(
                context);
        }
    }
}
