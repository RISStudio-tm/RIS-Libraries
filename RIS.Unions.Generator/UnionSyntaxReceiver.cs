using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RIS.Unions.Generator
{
    internal class UnionSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax { AttributeLists: { Count: > 0 } } classDeclarationSyntax
                && classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                CandidateClasses.Add(
                    classDeclarationSyntax);
            }
        }
    }
}
