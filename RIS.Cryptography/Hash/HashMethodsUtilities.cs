// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;

namespace RIS.Cryptography.Hash
{
    public static class HashMethodsUtilities
    {
        private static RNGCryptoServiceProvider RNGProvider { get; }

        public static ReadOnlyDictionary<string, Type> HashMethods { get; }

        static HashMethodsUtilities()
        {
            RNGProvider = new RNGCryptoServiceProvider();

            var cipherMethodsTypes = GetCipherMethods();
            var cipherMethods = new Dictionary<string, Type>(
                cipherMethodsTypes.Length);

            foreach (var cipherMethodType in cipherMethodsTypes)
            {
                cipherMethods.Add(
                    cipherMethodType.Name,
                    cipherMethodType);
            }

            HashMethods = new ReadOnlyDictionary<string, Type>(
                cipherMethods);
        }

        private static Type[] GetCipherMethods()
        {
            try
            {
                var hashMethods = new List<Type>();

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var types = assembly.GetTypes()
                        .Where(type => type.IsClass && typeof(IHashMethod).IsAssignableFrom(type));

                    foreach (var type in types)
                    {
                        hashMethods.Add(type);
                    }
                }

                return hashMethods.ToArray();
            }
            catch (Exception ex)
            {
                Events.OnError(null, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));

                return Array.Empty<Type>();
            }
        }

        public static string[] GetNames()
        {
            return HashMethods.Keys.ToArray();
        }

        public static Type[] GetTypes()
        {
            return HashMethods.Values.ToArray();
        }

        public static int GetCount()
        {
            return HashMethods.Count;
        }

        public static string GenerateSalt(ushort length)
        {
            if (length < 1)
            {
                var exception = new ArgumentOutOfRangeException(nameof(length), "Salt length cannot be less than 1");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            byte[] salt = new byte[length];
            RNGProvider.GetBytes(salt);

            return Convert.ToBase64String(salt);
        }
    }
}
