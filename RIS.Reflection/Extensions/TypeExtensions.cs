// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using RIS.Reflection.Conversion;

namespace RIS.Reflection.Extensions
{
    public static class TypeExtensions
    {
        public static bool CanBeNull(this Type type)
        {
            if (type == null)
            {
                var exception = new ArgumentNullException(nameof(type), $"{nameof(type)} must not be null");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return !type.IsValueType
                   || Nullable.GetUnderlyingType(type) != null;
        }

        public static object GetDefaultValue(this Type type)
        {
            if (type == null)
            {
                var exception = new ArgumentNullException(nameof(type), $"{nameof(type)} must not be null");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return type.IsValueType
                ? Activator.CreateInstance(type)
                : null;
        }

        public static bool IsExplicitlyCastableTo(this Type from, Type to)
        {
            if (from == null)
            {
                var exception = new ArgumentNullException(nameof(from), $"{nameof(from)} must not be null");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
            if (to == null)
            {
                var exception = new ArgumentNullException(nameof(to), $"{nameof(to)} must not be null");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return ConversionHelper
                .CanExplicitCast(from, to);
        }

        public static bool IsImplicitlyCastableTo(this Type from, Type to)
        {
            if (from == null)
            {
                var exception = new ArgumentNullException(nameof(from), $"{nameof(from)} must not be null");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
            if (to == null)
            {
                var exception = new ArgumentNullException(nameof(to), $"{nameof(to)} must not be null");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return ConversionHelper
                .CanImplicitCast(from, to);
        }
    }
}
