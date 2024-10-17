// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
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

        public static byte[] GetManifestResourceAsBytes(this Assembly assembly,
            string resourceName)
        {
            if (assembly == null)
                return null;

            var resourceInfo = assembly.GetManifestResourceInfo(resourceName);

            if (resourceInfo == null)
                return null;

            using (var resourceStream = assembly
                       .GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                    return null;

                using (var reader = new BinaryReader(
                           resourceStream))
                {
                    return reader.ReadBytes(
                        (int)reader.BaseStream.Length);
                }
            }
        }

        public static string GetManifestResourceAsString(this Assembly assembly,
            string resourceName)
        {
            if (assembly == null)
                return null;

            var resourceInfo = assembly.GetManifestResourceInfo(resourceName);

            if (resourceInfo == null)
                return null;

            using (var resourceStream = assembly
                       .GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                    return null;

                using (var reader = new StreamReader(
                           resourceStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
