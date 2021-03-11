// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RIS.Cryptography.Cipher
{
    public static class CipherMethodsUtils
    {
        public static ReadOnlyDictionary<string, Type> CipherMethods { get; }

        static CipherMethodsUtils()
        {
            var cipherMethodsTypes = GetCipherMethods();
            var cipherMethods = new Dictionary<string, Type>(
                cipherMethodsTypes.Length);

            foreach (var cipherMethodType in cipherMethodsTypes)
            {
                cipherMethods[cipherMethodType.Name] =
                    cipherMethodType;
            }

            CipherMethods = new ReadOnlyDictionary<string, Type>(
                cipherMethods);
        }

        private static Type[] GetCipherMethods()
        {
            try
            {
                var cipherMethods = new List<Type>();

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var types = assembly.GetTypes()
                        .Where(type => type.IsClass && typeof(ICipherMethod).IsAssignableFrom(type));

                    foreach (var type in types)
                    {
                        cipherMethods.Add(type);
                    }
                }

                return cipherMethods.ToArray();
            }
            catch (Exception ex)
            {
                Events.OnError(null, new RErrorEventArgs(ex, ex.Message));

                return Array.Empty<Type>();
            }
        }

        public static string[] GetNames()
        {
            return CipherMethods.Keys.ToArray();
        }

        public static Type[] GetTypes()
        {
            return CipherMethods.Values.ToArray();
        }

        public static int GetCount()
        {
            return CipherMethods.Count;
        }

        public static ICipherMethod Create(DefaultCipherMethod method)
        {
            return Create(method.ToString());
        }
        public static ICipherMethod Create(string methodName)
        {
            return (ICipherMethod)Activator.CreateInstance(
                CipherMethods[methodName]);
        }
    }
}
