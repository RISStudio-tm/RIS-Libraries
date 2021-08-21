// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using RIS.Synchronization;

namespace RIS.Localization
{
    public static class LocalizationManager
    {
        public static event EventHandler<LocalizationChangedEventArgs> DefaultLocalizationChanged;
        public static event EventHandler<LocalizationChangedEventArgs> LocalizationChanged;
        public static event EventHandler<LocalizationLoadedEventArgs> LocalizationsLoaded;
        public static event EventHandler<LocalizationFileNotFoundEventArgs> LocalizationFileNotFound;
        public static event EventHandler<LocalizedCultureNotFoundEventArgs> LocalizedCultureNotFound;
        public static event EventHandler<LocalizationEventArgs> LocalizationsNotFound;

        public static event EventHandler<LocalizationEventArgs> LocalizationUpdated;



        private static readonly Dictionary<string, Dictionary<string, LocalizationFactory>> FactoriesInternal;
        public static ReadOnlyDictionary<string, ReadOnlyDictionary<string, LocalizationFactory>> Factories { get; private set; }
        private static readonly Dictionary<string, LocalizationFactory> CurrentFactoriesInternal;
        public static ReadOnlyDictionary<string, LocalizationFactory> CurrentFactories
        {
            get
            {
                return new ReadOnlyDictionary<string, LocalizationFactory>(
                    CurrentFactoriesInternal);
            }
        }
        public static LocalizationFactory CurrentFactory
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                var assemblyName = Assembly.GetCallingAssembly()
                    .GetName().Name;

                return GetCurrentFactory(assemblyName);
            }
        }
        public static LocalizationFactory CurrentUIFactory
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                var assemblyName = Assembly.GetEntryAssembly()?
                    .GetName().Name;

                return GetCurrentFactory(assemblyName);
            }
        }


        public static AsyncLock SyncRoot
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                var assemblyName = Assembly.GetCallingAssembly()
                    .GetName().Name;

                return GetCurrentFactory(assemblyName)?
                    .SyncRoot ?? new AsyncLock();
            }
        }
        public static ILocalizationModule CurrentDefaultLocalization
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                var assemblyName = Assembly.GetCallingAssembly()
                    .GetName().Name;

                return GetCurrentFactory(assemblyName)?
                    .CurrentDefaultLocalization;
            }
        }
        public static ILocalizationModule CurrentLocalization
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                var assemblyName = Assembly.GetCallingAssembly()
                    .GetName().Name;

                return GetCurrentFactory(assemblyName)?
                    .CurrentLocalization;
            }
        }
        public static ReadOnlyDictionary<string, ILocalizationModule> Localizations
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                var assemblyName = Assembly.GetCallingAssembly()
                    .GetName().Name;

                return GetCurrentFactory(assemblyName)?
                    .Localizations;
            }
        }

        public static CultureInfo DefaultCulture
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                return CurrentFactory?.DefaultCulture;
            }
        }



        static LocalizationManager()
        {
            FactoriesInternal = new Dictionary<string, Dictionary<string, LocalizationFactory>>(5);
            Factories = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, LocalizationFactory>>(
                FactoriesInternal
                    .Select(item =>
                        new KeyValuePair<string, ReadOnlyDictionary<string, LocalizationFactory>>(
                            item.Key,
                            new ReadOnlyDictionary<string, LocalizationFactory>(item.Value)))
                    .ToDictionary(item => item.Key,
                        item => item.Value));
            CurrentFactoriesInternal = new Dictionary<string, LocalizationFactory>(5);
        }



        internal static void OnDefaultLocalizationChanged(
            object sender, LocalizationChangedEventArgs e)
        {
            DefaultLocalizationChanged?.Invoke(
                sender, e);
        }

        internal static void OnLocalizationChanged(
            object sender, LocalizationChangedEventArgs e)
        {
            LocalizationChanged?.Invoke(
                sender, e);
        }

        internal static void OnLocalizationsLoaded(
            object sender, LocalizationLoadedEventArgs e)
        {
            LocalizationsLoaded?.Invoke(
                sender, e);
        }

        internal static void OnLocalizationFileNotFound(
            object sender, LocalizationFileNotFoundEventArgs e)
        {
            LocalizationFileNotFound?.Invoke(
                sender, e);
        }

        internal static void OnLocalizedCultureNotFound(
            object sender, LocalizedCultureNotFoundEventArgs e)
        {
            LocalizedCultureNotFound?.Invoke(
                sender, e);
        }

        internal static void OnLocalizationsNotFound(
            object sender, LocalizationEventArgs e)
        {
            LocalizationsNotFound?.Invoke(
                sender, e);
        }

        internal static void OnLocalizationUpdated(
            object sender, LocalizationEventArgs e)
        {
            LocalizationUpdated?.Invoke(
                sender, e);
        }


        public static void OnLocalizationUpdated(
            ILocalizationModule localization)
        {
            foreach (var factories in FactoriesInternal)
            {
                foreach (var factory in factories.Value)
                {
                    factory.Value.OnLocalizationUpdated(
                        localization);
                }
            }
        }
        public static void OnLocalizationUpdated(
            string assemblyName, ILocalizationModule localization)
        {
            if (!FactoriesInternal.ContainsKey(assemblyName))
                return;

            foreach (var factory in FactoriesInternal[assemblyName])
            {
                factory.Value.OnLocalizationUpdated(
                    localization);
            }
        }
        public static void OnLocalizationUpdated(
            string assemblyName, string factoryName, ILocalizationModule localization)
        {
            if (!FactoriesInternal.ContainsKey(assemblyName))
                return;

            var factories = FactoriesInternal[assemblyName];

            if (!factories.ContainsKey(factoryName))
                return;

            factories[factoryName].OnLocalizationUpdated(
                localization);
        }
        public static void OnLocalizationUpdated()
        {
            foreach (var factories in FactoriesInternal)
            {
                foreach (var factory in factories.Value)
                {
                    factory.Value.OnLocalizationUpdated();
                }
            }
        }
        public static void OnLocalizationUpdated(
            string assemblyName)
        {
            if (!FactoriesInternal.ContainsKey(assemblyName))
                return;

            foreach (var factory in FactoriesInternal[assemblyName])
            {
                factory.Value.OnLocalizationUpdated();
            }
        }
        public static void OnLocalizationUpdated(
            string assemblyName, string factoryName)
        {
            if (!FactoriesInternal.ContainsKey(assemblyName))
                return;

            var factories = FactoriesInternal[assemblyName];

            if (!factories.ContainsKey(factoryName))
                return;

            factories[factoryName].OnLocalizationUpdated();
        }



        internal static void AddLocalizationFactory(
            string assemblyName, LocalizationFactory factory)
        {
            if (factory == null)
                return;
            if (string.IsNullOrEmpty(factory.Name))
                return;

            if (!FactoriesInternal.ContainsKey(assemblyName))
            {
                FactoriesInternal.Add(assemblyName,
                    new Dictionary<string, LocalizationFactory>(5));
            }

            var factories = FactoriesInternal[assemblyName];

            if (!factories.ContainsKey(factory.Name))
            {
                factories.Add(
                    factory.Name, factory);

                Factories = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, LocalizationFactory>>(
                    FactoriesInternal
                        .Select(item =>
                            new KeyValuePair<string, ReadOnlyDictionary<string, LocalizationFactory>>(
                                item.Key,
                                new ReadOnlyDictionary<string, LocalizationFactory>(item.Value)))
                        .ToDictionary(item => item.Key,
                            item => item.Value));

                if (factories.Count == 1)
                {
                    CurrentFactoriesInternal[assemblyName] = factories
                        .FirstOrDefault().Value; ;
                }
            }
        }

        internal static void RemoveLocalizationFactory(
            string assemblyName, LocalizationFactory factory)
        {
            if (factory == null)
                return;
            if (string.IsNullOrEmpty(factory.Name))
                return;

            if (!FactoriesInternal.ContainsKey(assemblyName))
            {
                FactoriesInternal.Add(assemblyName,
                    new Dictionary<string, LocalizationFactory>(5));
            }

            var factories = FactoriesInternal[assemblyName];

            if (factories.ContainsKey(factory.Name))
            {
                factories.Remove(
                    factory.Name);

                Factories = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, LocalizationFactory>>(
                    FactoriesInternal
                        .Select(item =>
                            new KeyValuePair<string, ReadOnlyDictionary<string, LocalizationFactory>>(
                                item.Key,
                                new ReadOnlyDictionary<string, LocalizationFactory>(item.Value)))
                        .ToDictionary(item => item.Key,
                            item => item.Value));

                if (CurrentFactory.Name == factory.Name)
                {
                    CurrentFactoriesInternal[assemblyName] = factories
                        .FirstOrDefault().Value;
                }
            }
        }



        [MethodImpl(MethodImplOptions.NoInlining)]
        public static LocalizationFactory GetCurrentFactory()
        {
            var assemblyName = Assembly.GetCallingAssembly()
                .GetName().Name;

            return GetCurrentFactory(
                assemblyName);
        }
        public static LocalizationFactory GetCurrentFactory(
            string assemblyName)
        {
            if (CurrentFactories.TryGetValue(assemblyName, out var factory))
                return factory;

            return null;
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool SetCurrentFactory(
            string factoryName)
        {
            var assemblyName = Assembly.GetCallingAssembly()
                .GetName().Name;

            return SetCurrentFactory(
                assemblyName, factoryName);
        }
        public static bool SetCurrentFactory(
            string assemblyName, string factoryName)
        {
            if (string.IsNullOrEmpty(factoryName))
                return false;

            if (!FactoriesInternal.ContainsKey(assemblyName))
            {
                FactoriesInternal.Add(assemblyName,
                    new Dictionary<string, LocalizationFactory>(5));
            }

            var factories = FactoriesInternal[assemblyName];

            if (!factories.TryGetValue(factoryName, out var factory))
                return false;

            CurrentFactoriesInternal[assemblyName] = factory;

            return true;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool SetCurrentFactory(
            LocalizationFactory factory)
        {
            var assemblyName = Assembly.GetCallingAssembly()
                .GetName().Name;

            return SetCurrentFactory(
                assemblyName, factory);
        }
        public static bool SetCurrentFactory(
            string assemblyName, LocalizationFactory factory)
        {
            if (factory == null)
                return false;
            if (string.IsNullOrEmpty(factory.Name))
                return false;

            AddLocalizationFactory(
                assemblyName, factory);

            CurrentFactoriesInternal[assemblyName] = factory;

            return true;
        }



        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetDefaultCulture(
            string cultureName)
        {
            var assemblyName = Assembly.GetCallingAssembly()
                .GetName().Name;

            SetDefaultCulture(
                assemblyName, cultureName);
        }
        public static void SetDefaultCulture(
            string assemblyName, string cultureName)
        {
            GetCurrentFactory(assemblyName)?
                .SetDefaultCulture(cultureName);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetDefaultCulture(
            CultureInfo culture)
        {
            var assemblyName = Assembly.GetCallingAssembly()
                .GetName().Name;

            SetDefaultCulture(
                assemblyName, culture);
        }
        public static void SetDefaultCulture(
            string assemblyName, CultureInfo culture)
        {
            GetCurrentFactory(assemblyName)?
                .SetDefaultCulture(culture);
        }



        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ReloadLocalizations<T>()
            where T : ILocalizationProvider
        {
            var assemblyName = Assembly.GetCallingAssembly()
                .GetName().Name;

            ReloadLocalizations<T>(
                assemblyName);
        }
        public static void ReloadLocalizations<T>(
            string assemblyName)
            where T : ILocalizationProvider
        {
            GetCurrentFactory(assemblyName)?
                .ReloadLocalizations<T>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetDefaultCultureName()
        {
            var assemblyName = Assembly.GetCallingAssembly()
                .GetName().Name;

            return GetDefaultCultureName(
                assemblyName);
        }
        public static string GetDefaultCultureName(
            string assemblyName)
        {
            return GetCurrentFactory(assemblyName)?
                .GetDefaultCultureName();
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool SwitchDefaultLocalization(
            string cultureName)
        {
            var assemblyName = Assembly.GetCallingAssembly()
                .GetName().Name;

            return SwitchDefaultLocalization(
                assemblyName, cultureName);
        }
        public static bool SwitchDefaultLocalization(
            string assemblyName, string cultureName)
        {
            return GetCurrentFactory(assemblyName)?
                .SwitchDefaultLocalization(cultureName) == true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool SwitchLocalization(
            string cultureName)
        {
            var assemblyName = Assembly.GetCallingAssembly()
                .GetName().Name;

            return SwitchLocalization(
                assemblyName, cultureName);
        }
        public static bool SwitchLocalization(
            string assemblyName, string cultureName)
        {
            return GetCurrentFactory(assemblyName)?
                .SwitchLocalization(cultureName) == true;
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetLocalized(
            string key)
        {
            var assemblyName = Assembly.GetCallingAssembly()
                .GetName().Name;

            return GetLocalized(
                assemblyName, key);
        }
        public static string GetLocalized(
            string assemblyName, string key)
        {
            return GetCurrentFactory(assemblyName)?
                .GetLocalized(key);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool TryGetLocalized(
            string key, out string value)
        {
            var assemblyName = Assembly.GetCallingAssembly()
                .GetName().Name;

            return TryGetLocalized(
                assemblyName, key, out value);
        }
        public static bool TryGetLocalized(
            string assemblyName, string key, out string value)
        {
            value = null;

            return GetCurrentFactory(assemblyName)?
                .TryGetLocalized(key, out value) == true;
        }
    }
}
