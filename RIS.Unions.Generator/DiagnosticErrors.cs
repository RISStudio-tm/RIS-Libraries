using System;
using Microsoft.CodeAnalysis;

namespace RIS.Unions.Generator
{
    internal static class DiagnosticErrors
    {
        public static readonly DiagnosticDescriptor TopLevelError = new(
            "UNIONGEN001",
            "Class must be top level",
            "Class '{0}' using OneOfGenerator must be top level",
            "OneOfGenerator",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor WrongBaseType = new(
            "UNIONGEN002",
            "Class must inherit from OneOfBase",
            "Class '{0}' does not inherit from OneOfBase",
            "OneOfGenerator",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor ClassIsNotPublic = new(
            "UNIONGEN003",
            "Class must be public",
            "Class '{0}' is not public",
            "OneOfGenerator",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor ObjectIsOneOfType = new(
            "UNIONGEN004",
            "Object is not a valid type parameter",
            "Defined conversions to or from a base type are not allowed for class '{0}'",
            "OneOfGenerator",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor WrongNumberOfTypeNames = new(
            "UNIONGEN005",
            "Wrong number of names",
            "Expected zero or {0} names but got {1}",
            "OneOfGenerator",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor TypeNameCannotBeNullOrEmpty = new(
            "UNIONGEN006",
            "Name cannot be null or empty",
            "Name at position {0} cannot be null or empty",
            "OneOfGenerator",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor InvalidTypeName = new(
            "UNIONGEN007",
            "Invalid name",
            "The name '{0}' cannot produce valid property or method names",
            "OneOfGenerator",
            DiagnosticSeverity.Error,
            true);
    }
}
