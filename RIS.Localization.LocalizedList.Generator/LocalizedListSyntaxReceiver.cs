// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RIS.Localization.LocalizedList.Generator
{
    internal class LocalizedListSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax { BaseList: { Types: { Count: > 0 } } } classDeclarationSyntax
                && classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                CandidateClasses.Add(
                    classDeclarationSyntax);
            }
        }
    }
}
