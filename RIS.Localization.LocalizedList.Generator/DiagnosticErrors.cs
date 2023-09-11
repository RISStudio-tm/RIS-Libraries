// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using Microsoft.CodeAnalysis;

namespace RIS.Localization.LocalizedList.Generator
{
    internal static class DiagnosticErrors
    {
        public static readonly DiagnosticDescriptor TopLevelError = new(
            "LOCLISTGEN001",
            "Class must be top level",
            "Class '{0}' using LocalizedListGenerator must be top level",
            "LocalizedListGenerator",
            DiagnosticSeverity.Error,
            true);

        //public static readonly DiagnosticDescriptor ClassIsNotPublic = new(
        //    "LOCLISTGEN002",
        //    "Class must be public",
        //    "Class '{0}' is not public",
        //    "LocalizedListGenerator",
        //    DiagnosticSeverity.Error,
        //    true);

        //public static readonly DiagnosticDescriptor ClassIsNotPartial = new(
        //    "LOCLISTGEN003",
        //    "Class must be partial",
        //    "Class '{0}' is not partial",
        //    "LocalizedListGenerator",
        //    DiagnosticSeverity.Error,
        //    true);

        public static readonly DiagnosticDescriptor ClassShouldHaveDefaultConstructor = new(
            "LOCLISTGEN004",
            "Class should have a default (parameterless) constructor",
            "Class '{0}' does not have a default (parameterless) constructor",
            "LocalizedListGenerator",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor DefaultConstructorIsNotPrivate = new(
            "LOCLISTGEN005",
            "Class default (parameterless) constructor must be private",
            "'{0}' class default (parameterless) constructor is not private",
            "LocalizedListGenerator",
            DiagnosticSeverity.Error,
            true);
    }
}
