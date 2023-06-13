// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Reflection;

namespace RIS.Extensions
{
    public static class MethodInfoExtensions
    {
        public static bool IsOverride(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                var exception = new ArgumentNullException(nameof(methodInfo));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType;
        }
    }
}
