// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace RIS.Extensions
{
    public static class AssemblyExtensions
    {
        public static bool IsDebugBuild(this Assembly assembly)
        {
            if (assembly == null)
            {
                var exception = new ArgumentNullException(nameof(assembly));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return assembly.GetCustomAttributes(false)
                .OfType<DebuggableAttribute>()
                .Select(attribute => attribute.IsJITTrackingEnabled)
                .FirstOrDefault();
        }
    }
}
