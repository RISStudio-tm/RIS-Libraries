// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace RIS.Localization.LocalizedList.Generator.Extensions
{
    internal static class DiagnosticExtensions
    {
        public static bool ReportIfAny(
            this IEnumerable<Diagnostic> diagnostics,
            GeneratorExecutionContext context)
        {
            var diagnosticsArray =
                diagnostics as Diagnostic[] ?? diagnostics.ToArray();

            if (!diagnosticsArray.Any())
                return true;

            foreach (var diagnostic in diagnosticsArray)
            {
                context.ReportDiagnostic(
                    diagnostic);
            }

            return false;
        }
    }
}
