// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RIS.Cryptography.Cipher
{
    public static class SignMethodsUtils
    {
        public static ReadOnlyDictionary<string, Type> SignMethods { get; }

        static SignMethodsUtils()
        {
            var signMethodsTypes = GetSignMethods();
            var signMethods = new Dictionary<string, Type>(
                signMethodsTypes.Length);

            foreach (var signMethodType in signMethodsTypes)
            {
                signMethods[signMethodType.Name] =
                    signMethodType;
            }

            SignMethods = new ReadOnlyDictionary<string, Type>(
                signMethods);
        }

        private static Type[] GetSignMethods()
        {
            try
            {
                var signMethods = new List<Type>();

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var types = assembly.GetTypes()
                        .Where(type => type.IsClass && typeof(ISignMethod).IsAssignableFrom(type));

                    foreach (var type in types)
                    {
                        signMethods.Add(type);
                    }
                }

                return signMethods.ToArray();
            }
            catch (Exception ex)
            {
                Events.OnError(null, new RErrorEventArgs(ex, ex.Message));

                return Array.Empty<Type>();
            }
        }

        public static string[] GetNames()
        {
            return SignMethods.Keys.ToArray();
        }

        public static Type[] GetTypes()
        {
            return SignMethods.Values.ToArray();
        }

        public static int GetCount()
        {
            return SignMethods.Count;
        }

        public static ISignMethod Create(DefaultSignMethod method)
        {
            return Create(method.ToString());
        }
        public static ISignMethod Create(string methodName)
        {
            return (ISignMethod)Activator.CreateInstance(
                SignMethods[methodName]);
        }
    }
}
