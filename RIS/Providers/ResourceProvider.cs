// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace RIS.Providers
{
    public static class ResourceProvider
    {
        private static readonly Dictionary<string, EmbeddedFileProvider> CachedEmbeddedProviders;

        static ResourceProvider()
        {
            CachedEmbeddedProviders = new Dictionary<string, EmbeddedFileProvider>(50);
        }

        private static EmbeddedFileProvider GetEmbeddedProvider(
            Assembly assembly, string baseNamespace)
        {
            var key = $"{baseNamespace} ||| {assembly.GetName().FullName}";

            if (CachedEmbeddedProviders.TryGetValue(key, out var resourceProvider))
                return resourceProvider;

            if (CachedEmbeddedProviders.Count == 50)
                CachedEmbeddedProviders.Clear();

            resourceProvider = new EmbeddedFileProvider(
                assembly, baseNamespace);

            CachedEmbeddedProviders.Add(
                key, resourceProvider);

            return resourceProvider;
        }

        public static byte[] GetEmbeddedAsBytes(
           string filePath)
        {
            var assembly = Assembly.GetCallingAssembly();
            var baseNamespace = Path.GetFileNameWithoutExtension(
                assembly.Location);

            return GetEmbeddedAsBytes(assembly,
                baseNamespace, filePath);
        }
        public static byte[] GetEmbeddedAsBytes(
            string baseNamespace, string filePath)
        {
            var assembly = Assembly.GetCallingAssembly();

            return GetEmbeddedAsBytes(assembly,
                baseNamespace, filePath);
        }
        public static byte[] GetEmbeddedAsBytes(Assembly assembly,
            string filePath)
        {
            var baseNamespace = Path.GetFileNameWithoutExtension(
                assembly.Location);

            return GetEmbeddedAsBytes(assembly,
                baseNamespace, filePath);
        }
        public static byte[] GetEmbeddedAsBytes(Assembly assembly,
            string baseNamespace, string filePath)
        {
            var resourceProvider = GetEmbeddedProvider(
                assembly, baseNamespace);

            using (var stream = resourceProvider
                .GetFileInfo(filePath)
                .CreateReadStream())
            {
                if (stream == null)
                    return null;

                using (var reader = new BinaryReader(
                    stream))
                {
                    return reader.ReadBytes(
                        (int)reader.BaseStream.Length);
                }
            }
        }

        public static string GetEmbeddedAsString(
            string filePath)
        {
            var assembly = Assembly.GetCallingAssembly();
            var baseNamespace = Path.GetFileNameWithoutExtension(
                assembly.Location);

            return GetEmbeddedAsString(assembly,
                baseNamespace, filePath);
        }
        public static string GetEmbeddedAsString(
            string baseNamespace, string filePath)
        {
            var assembly = Assembly.GetCallingAssembly();

            return GetEmbeddedAsString(assembly,
                baseNamespace, filePath);
        }
        public static string GetEmbeddedAsString(Assembly assembly,
            string filePath)
        {
            var baseNamespace = Path.GetFileNameWithoutExtension(
                assembly.Location);

            return GetEmbeddedAsString(assembly,
                baseNamespace, filePath);
        }
        public static string GetEmbeddedAsString(Assembly assembly,
            string baseNamespace, string filePath)
        {
            var resourceProvider = GetEmbeddedProvider(
                assembly, baseNamespace);

            using (var stream = resourceProvider
                .GetFileInfo(filePath)
                .CreateReadStream())
            {
                if (stream == null)
                    return null;

                using (var reader = new StreamReader(
                    stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
