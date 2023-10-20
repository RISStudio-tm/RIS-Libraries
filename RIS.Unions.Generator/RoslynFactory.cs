// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RIS.Unions.Generator
{
    internal class RoslynFactory
    {
        internal static PropertyDeclarationSyntax CreatePropertyIsX(
            ITypeParameterSymbol param,
            ITypeSymbol arg,
            TypedConstant name)
        {
            return PropertyDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.BoolKeyword)),
                    Identifier("Is" + name.Value))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName("Is" + param.Name))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken))
                .NormalizeWhitespace();
        }

        internal static PropertyDeclarationSyntax CreatePropertyAsX(
            ITypeParameterSymbol param,
            ITypeSymbol arg,
            TypedConstant name)
        {
            return PropertyDeclaration(
                    IdentifierName(arg.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                    Identifier("As" + name.Value))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName("As" + param.Name))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken))
                .NormalizeWhitespace();
        }

        internal static MethodDeclarationSyntax CreateMethodTryPickX(
            IEnumerable<(ITypeParameterSymbol param, ITypeSymbol arg)> paramArgsPairs,
            ITypeParameterSymbol param,
            ITypeSymbol arg,
            TypedConstant name)
        {
            var remainderArgs = paramArgsPairs
                .Except(new[] { (param, arg) })
                .Select(x => x.arg.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                .Select(x => IdentifierName(x))
                .ToArray();
            TypeSyntax remainderType = remainderArgs.Count() > 1
                ? SyntaxFactory.GenericName(SyntaxFactory.Identifier("Union"))
                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SeparatedList<TypeSyntax>(remainderArgs)))
                : remainderArgs.Single();

            return MethodDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.BoolKeyword)),
                    Identifier("TryPick" + name.Value))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]{
                                Parameter(Identifier("value"))
                                    .WithModifiers(
                                        TokenList(
                                            Token(SyntaxKind.OutKeyword)))
                                    .WithType(IdentifierName(arg.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))),
                                Token(SyntaxKind.CommaToken),
                                Parameter(Identifier("remainder"))
                                    .WithModifiers(
                                        TokenList(
                                            Token(SyntaxKind.OutKeyword)))
                                    .WithType(remainderType)})))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName("TryPick" + param.Name)))
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList<ArgumentSyntax>(
                                    new SyntaxNodeOrToken[]{
                            Argument(
                                IdentifierName("value"))
                            .WithRefOrOutKeyword(
                                Token(SyntaxKind.OutKeyword)),
                            Token(SyntaxKind.CommaToken),
                            Argument(IdentifierName("remainder"))
                            .WithRefOrOutKeyword(
                                Token(SyntaxKind.OutKeyword))})))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken))
                .NormalizeWhitespace();
        }
    }
}
