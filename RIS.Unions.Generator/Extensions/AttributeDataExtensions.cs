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
