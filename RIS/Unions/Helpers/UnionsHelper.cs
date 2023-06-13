// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Unions
{
    internal static class UnionsHelper
    {
        internal static string FormatValue<T>(
            T value)
        {
            return $"{typeof(T).FullName}: {value?.ToString()}";
        }
        internal static string FormatValue<T>(
            object @this, object @base, T value)
        {
            return ReferenceEquals(@this, value)
                ? @base.ToString()
                : $"{typeof(T).FullName}: {value?.ToString()}";
        }
    }
}
