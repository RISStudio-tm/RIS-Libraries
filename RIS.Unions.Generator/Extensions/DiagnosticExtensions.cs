using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace RIS.Unions.Generator.Extensions
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
