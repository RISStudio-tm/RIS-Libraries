// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using Microsoft.CodeAnalysis;

namespace RIS.Unions.Generator
{
    internal static class DiagnosticErrors
    {
        public static readonly DiagnosticDescriptor TopLevelError = new(
            "UNIONGEN001",
            "Class must be top level",
            "Class '{0}' using UnionGenerator must be top level",
            "UnionGenerator",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor WrongBaseType = new(
            "UNIONGEN002",
            "Class must inherit from UnionBase",
            "Class '{0}' does not inherit from UnionBase",
            "UnionGenerator",
            DiagnosticSeverity.Error,
            true);

        //public static readonly DiagnosticDescriptor ClassIsNotPublic = new(
        //    "UNIONGEN003",
        //    "Class must be public",
        //    "Class '{0}' is not public",
        //    "UnionGenerator",
        //    DiagnosticSeverity.Error,
        //    true);

        public static readonly DiagnosticDescriptor ObjectIsUnionType = new(
            "UNIONGEN004",
            "Object is not a valid type parameter",
            "Defined conversions to or from a base type are not allowed for class '{0}'",
            "UnionGenerator",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor WrongNumberOfTypeNames = new(
            "UNIONGEN005",
            "Wrong number of names",
            "Expected zero or {0} names but got {1}",
            "UnionGenerator",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor TypeNameCannotBeNullOrEmpty = new(
            "UNIONGEN006",
            "Name cannot be null or empty",
            "Name at position {0} cannot be null or empty",
            "UnionGenerator",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor InvalidTypeName = new(
            "UNIONGEN007",
            "Invalid name",
            "The name '{0}' cannot produce valid property or method names",
            "UnionGenerator",
            DiagnosticSeverity.Error,
            true);
        public static readonly DiagnosticDescriptor UserDefinedConversionsToOrFromAnInterfaceAreNotAllowed = new(
            "UNIONGEN008",
            "User-defined conversions to or from an interface are not allowed",
            "User-defined conversions to or from an interface are not allowed",
            "UnionGenerator",
            DiagnosticSeverity.Error,
            true);
    }
}
