﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RIS.Localization.LocalizedList.Generator
{
    internal class RoslynFactory
    {
        private const string StaticPropertyChangedEventName = "StaticPropertyChanged";
        private const string InstancePropertyName = "Instance";
        private const string GetInstanceMethodName = "GetInstance";



        internal static EventFieldDeclarationSyntax CreateEventStaticPropertyChanged()
        {
            return EventFieldDeclaration(
                VariableDeclaration(
                        IdentifierName(nameof(PropertyChangedEventHandler)))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                Identifier(StaticPropertyChangedEventName)))))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.StaticKeyword)))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken))
                .NormalizeWhitespace();
        }

        internal static PropertyDeclarationSyntax CreateInstanceProperty(
            ITypeSymbol arg)
        {
            return PropertyDeclaration(
                    IdentifierName(arg.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                    Identifier(InstancePropertyName))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.StaticKeyword)))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        InvocationExpression(
                            GenericName(
                                    Identifier(GetInstanceMethodName))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList<TypeSyntax>(
                                            IdentifierName(arg.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))))))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken))
                .NormalizeWhitespace();
        }
    }
}
