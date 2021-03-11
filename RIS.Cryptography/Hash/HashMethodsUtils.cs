﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RIS.Randomizing;
using RIS.Text.Generating;

namespace RIS.Cryptography.Hash
{
    public static class HashMethodsUtils
    {
        private static readonly FastSecureRandom RandomGenerator;

        public static ReadOnlyDictionary<string, Type> HashMethods { get; }

        static HashMethodsUtils()
        {
            RandomGenerator = new FastSecureRandom();

            var hashMethodsTypes = GetHashMethods();
            var hashMethods = new Dictionary<string, Type>(
                hashMethodsTypes.Length);

            foreach (var hashMethodType in hashMethodsTypes)
            {
                hashMethods[hashMethodType.Name] =
                    hashMethodType;
            }

            HashMethods = new ReadOnlyDictionary<string, Type>(
                hashMethods);
        }

        private static Type[] GetHashMethods()
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
                Events.OnError(null, new RErrorEventArgs(ex, ex.Message));

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

        public static IHashMethod Create(DefaultHashMethod method)
        {
            return Create(method.ToString());
        }
        public static IHashMethod Create(string methodName)
        {
            return (IHashMethod)Activator.CreateInstance(
                HashMethods[methodName]);
        }



        public static string GenerateSalt(ushort length)
        {
            if (length < 1)
            {
                var exception = new ArgumentOutOfRangeException(nameof(length),
                    "Salt length cannot be less than 1");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message));
                throw exception;
            }

            return StringGenerator.GenerateString(length);
        }
        public static byte[] GenerateSaltBytes(ushort length)
        {
            byte[] salt = new byte[length];

            RandomGenerator.GetBytes(salt);

            return salt;
        }
    }
}
