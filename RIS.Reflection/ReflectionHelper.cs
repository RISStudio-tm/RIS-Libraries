// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RIS.Reflection
{
    public static class ReflectionHelper
    {
        public static MethodInfo GetMethod(Expression<Action> expression)
        {
            return GetMethodInternal(expression);
        }
        public static MethodInfo GetMethod<TInstance>(Expression<Action<TInstance>> expression)
        {
            return GetMethodInternal(expression);
        }
        private static MethodInfo GetMethodInternal(LambdaExpression expression)
        {
            if (expression == null)
            {
                var exception = new ArgumentNullException(nameof(expression), $"{nameof(expression)} must not be null");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (!(expression.Body is MethodCallExpression methodCall))
            {
                var exception = new ArgumentException($"{nameof(expression)}: body of the lambda expression must be a method call. Found: {expression.Body.NodeType}");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return methodCall.Method;
        }

        public static PropertyInfo GetProperty<TProperty>(Expression<Func<TProperty>> expression)
        {
            return GetPropertyInternal(expression);
        }
        public static PropertyInfo GetProperty<TInstance, TProperty>(Expression<Func<TInstance, TProperty>> expression)
        {
            return GetPropertyInternal(expression);
        }
        private static PropertyInfo GetPropertyInternal(LambdaExpression expression)
        {
            if (expression == null)
            {
                var exception = new ArgumentNullException(nameof(expression), $"{nameof(expression)} must not be null");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (!(expression.Body is MemberExpression property)
                || property.Member.MemberType != MemberTypes.Property)
            {
                var exception = new ArgumentException($"{nameof(expression)}: body of the lambda expression must be a property access. Found: {expression.Body.NodeType}");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return (PropertyInfo)property.Member;
        }
    }
}
