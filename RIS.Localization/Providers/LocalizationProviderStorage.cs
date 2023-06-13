// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;

namespace RIS.Localization.Providers
{
    internal static class LocalizationProviderStorage
    {
        private static readonly Dictionary<Type, ILocalizationProvider> Storage;



        static LocalizationProviderStorage()
        {
            Storage = new Dictionary<Type, ILocalizationProvider>();
        }



        private static ILocalizationProvider FindProvider(
            Type type)
        {
            if (!typeof(ILocalizationProvider).IsAssignableFrom(type))
            {
                var exception =
                    new ArgumentException(
                        $"{nameof(type)} must be derived from the {nameof(ILocalizationProvider)}",
                        nameof(type));
                Events.OnError(null, new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            Storage.TryGetValue(
                type, out var provider);

            return provider ?? CreateProvider(type);
        }

        private static ILocalizationProvider CreateProvider(
            Type type)
        {
            if (!typeof(ILocalizationProvider).IsAssignableFrom(type))
            {
                var exception =
                    new ArgumentException(
                        $"{nameof(type)} must be derived from the {nameof(ILocalizationProvider)}",
                        nameof(type));
                Events.OnError(null, new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            ILocalizationProvider provider;

            try
            {
                provider = Activator
                    .CreateInstance(type, true) as ILocalizationProvider;

                if (provider == null)
                    throw new TypeLoadException();
            }
            catch (Exception)
            {
                var exception = new TypeLoadException(
                    "Failed to create a provider");
                Events.OnError(null, new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            Storage.Add(
                type, provider);

            return provider;
        }



        public static ILocalizationProvider GetProvider<T>()
            where T : ILocalizationProvider
        {
            return FindProvider(typeof(T));
        }
    }
}
