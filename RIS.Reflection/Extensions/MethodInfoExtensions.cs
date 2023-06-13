// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RIS.Reflection.Extensions
{
    public static class MethodInfoExtensions
    {
        // ReSharper disable CoVariantArrayConversion
        public static Type GetDelegateType(this MethodInfo method)
        {
            if (method == null)
            {
                var exception = new ArgumentNullException(nameof(method), $"{nameof(method)} must not be null");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
            if (method.DeclaringType == null)
            {
                var exception = new ArgumentNullException(nameof(method), $"{nameof(method.DeclaringType)} must not be null");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            var parameters = method.GetParameters();

            if (parameters.Length < 16
                && !parameters.Any(p => p.ParameterType.IsByRef))
            {
                var isAction = method.ReturnType == typeof(void);
                var typeName = isAction
                    ? parameters.Length == 0
                        ? "System.Action"
                        : "System.Action`" + parameters.Length
                    : "System.Func`" + (parameters.Length + 1);
                var genericTypeDefinition = typeof(Action).Assembly
                    .GetType(typeName, true);


                if (genericTypeDefinition == null)
                {
                    var exception = new NullReferenceException($"{nameof(genericTypeDefinition)} must not be null");
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }

                if (!genericTypeDefinition.IsGenericTypeDefinition)
                    return genericTypeDefinition; // only true for Action

                var genericTypeArguments = new Type[parameters.Length + (isAction ? 1 : 0)];

                for (var i = 0; i < parameters.Length; ++i)
                {
                    genericTypeArguments[i] = parameters[i].ParameterType;
                }

                if (!isAction)
                    genericTypeArguments[genericTypeArguments.Length - 1] = method.ReturnType;

                return genericTypeDefinition
                    .MakeGenericType(genericTypeArguments);
            }

            var parametersExpressions = new ParameterExpression[parameters.Length];

            for (var i = 0; i < parameters.Length; ++i)
            {
                parametersExpressions[i] = Expression
                    .Parameter(parameters[i].ParameterType);
            }

            var lambda = Expression.Lambda(
                Expression.Call(
                    !method.IsStatic
                        ? Expression.Parameter(method.DeclaringType)
                        : null,
                    method,
                    parametersExpressions
                ),
                parametersExpressions
            );

            return lambda.Type;
        }
        // ReSharper restore CoVariantArrayConversion
    }
}
