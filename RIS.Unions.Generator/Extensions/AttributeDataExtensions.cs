// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RIS.Unions.Generator.Extensions
{
    internal static class AttributeDataExtensions
    {
        public static AttributeSyntax? GetSyntax(
            this AttributeData attributeData)
        {
            return attributeData
                .ApplicationSyntaxReference?
                .GetSyntax() as AttributeSyntax;
        }
    }
}
