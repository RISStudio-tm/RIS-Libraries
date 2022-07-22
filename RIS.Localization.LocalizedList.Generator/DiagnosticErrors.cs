using System;
using Microsoft.CodeAnalysis;

namespace RIS.Localization.LocalizedList.Generator
{
    internal static class DiagnosticErrors
    {
        public static readonly DiagnosticDescriptor TopLevelError = new(
            "LOCLISTGEN001",
            "Class must be top level",
            "Class '{0}' using OneOfGenerator must be top level",
            "LocalizedListGenerator",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor WrongBaseType = new(
            "LOCLISTGEN002",
            "Class must inherit from OneOfBase",
            "Class '{0}' does not inherit from OneOfBase",
            "LocalizedListGenerator",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor ClassIsNotPublic = new(
            "LOCLISTGEN003",
            "Class must be public",
            "Class '{0}' is not public",
            "LocalizedListGenerator",
            DiagnosticSeverity.Error,
            true);
    }
}
