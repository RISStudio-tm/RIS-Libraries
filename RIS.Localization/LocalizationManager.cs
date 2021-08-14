// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using RIS.Localization.Providers;
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
        public static event EventHandler<EventArgs> LocalizationsNotFound;

        public static event EventHandler<EventArgs> LocalizationUpdated;



        public static AsyncLock SyncRoot { get; }
        public static string DefaultLocalizationsDirectoryName { get; }
        public static string CustomLocalizationsDirectoryName { get; }
        private static ILocalizationModule _currentDefaultLocalization;
        public static ILocalizationModule CurrentDefaultLocalization
        {
            get
            {
                return _currentDefaultLocalization;
            }
            private set
            {
                Interlocked.Exchange(
                    ref _currentDefaultLocalization, value);
            }
        }
        private static ILocalizationModule _currentLocalization;
        public static ILocalizationModule CurrentLocalization
        {
            get
            {
                return _currentLocalization;
            }
            private set
            {
                Interlocked.Exchange(
                    ref _currentLocalization, value);
            }
        }
        public static ReadOnlyDictionary<string, ILocalizationModule> Localizations { get; private set; }

        public static CultureInfo DefaultCulture { get; private set; }



        static LocalizationManager()
        {
            var baseAppDirectory = Environment.ExecAppDirectoryName;
            var baseProcessDirectory = Environment.ExecProcessDirectoryName;

            if (string.IsNullOrEmpty(baseAppDirectory) || baseAppDirectory == "Unknown")
                return;
            if (string.IsNullOrEmpty(baseProcessDirectory) || baseProcessDirectory == "Unknown")
                return;

            SyncRoot = new AsyncLock();
            DefaultLocalizationsDirectoryName = Path.Combine(
                baseAppDirectory, "localizations", "default");
            CustomLocalizationsDirectoryName = Path.Combine(
                baseProcessDirectory, "localizations");
            CurrentLocalization = null;
            Localizations = new ReadOnlyDictionary<string, ILocalizationModule>(
                new Dictionary<string, ILocalizationModule>());

            DefaultCulture = new CultureInfo("en-US");

            if (!Directory.Exists(DefaultLocalizationsDirectoryName))
                Directory.CreateDirectory(DefaultLocalizationsDirectoryName);
            if (!Directory.Exists(CustomLocalizationsDirectoryName))
                Directory.CreateDirectory(CustomLocalizationsDirectoryName);
        }



        private static void OnDefaultLocalizationChanged(
            ILocalizationModule newLocalization)
        {
            var oldLocalization = Interlocked.Exchange(
                ref _currentDefaultLocalization, newLocalization);

            if (oldLocalization != null && oldLocalization.Equals(newLocalization))
                return;

            DefaultLocalizationChanged?.Invoke(null,
                new LocalizationChangedEventArgs(oldLocalization, newLocalization));
        }

        private static void OnLocalizationChanged(
            ILocalizationModule newLocalization)
        {
            var oldLocalization = Interlocked.Exchange(
                ref _currentLocalization, newLocalization);

            if (oldLocalization != null && oldLocalization.Equals(newLocalization))
                return;

            LocalizationChanged?.Invoke(null,
                new LocalizationChangedEventArgs(oldLocalization, newLocalization));

            Thread.CurrentThread.CurrentCulture = newLocalization?.Culture ?? DefaultCulture;
            Thread.CurrentThread.CurrentUICulture = newLocalization?.Culture ?? DefaultCulture;

            SynchronizationContext.Current?.Send((_) =>
            {
                Thread.CurrentThread.CurrentCulture = newLocalization?.Culture ?? DefaultCulture;
                Thread.CurrentThread.CurrentUICulture = newLocalization?.Culture ?? DefaultCulture;
            }, null);
        }

        private static void OnLocalizationsLoaded(
            Dictionary<string, ILocalizationModule> localizations)
        {
            LocalizationsLoaded?.Invoke(null,
                new LocalizationLoadedEventArgs(localizations));
        }

        private static void OnLocalizationFileNotFound(
            string filePath)
        {
            LocalizationFileNotFound?.Invoke(null,
                new LocalizationFileNotFoundEventArgs(filePath));

            var exception = new FileNotFoundException(
                $"Localization file['{filePath}'] not found");
            Events.OnError(new RErrorEventArgs(
                exception, exception.Message));
        }

        private static void OnLocalizedCultureNotFound(
            string cultureName)
        {
            LocalizedCultureNotFound?.Invoke(null,
                new LocalizedCultureNotFoundEventArgs(cultureName));

            var exception = new CultureNotFoundException(
                $"Localized culture['{cultureName}'] not found");
            Events.OnError(new RErrorEventArgs(
                exception, exception.Message));
        }

        private static void OnLocalizationsNotFound()
        {
            LocalizationsNotFound?.Invoke(null,
                new EventArgs());

            var exception = new Exception(
                "Localizations not found");
            Events.OnError(new RErrorEventArgs(
                exception, exception.Message));
        }


        public static void OnLocalizationUpdated(
            ILocalizationModule localization)
        {
            using var @lock = SyncRoot.Lock();

            if ((CurrentLocalization == null || !CurrentLocalization.Equals(localization)) &&
                (CurrentDefaultLocalization == null || !CurrentDefaultLocalization.Equals(localization)))
            {
                return;
            }

            OnLocalizationUpdated();
        }
        public static void OnLocalizationUpdated()
        {
            LocalizationUpdated?.Invoke(null,
                new EventArgs());
        }



        private static bool IsValidLocalizationModule(
            ILocalizationModule module)
        {
            var result = true;

            foreach (var localizationFile in module.Files)
            {
                if (File.Exists(localizationFile.Path))
                    continue;

                OnLocalizationFileNotFound(localizationFile.Path);

                result = false;
            }

            return result;
        }



        private static void LoadLocalizations<T>()
            where T: ILocalizationProvider
        {
            using var @lock = SyncRoot.Lock();

            var localizations = LocalizationProviderStorage
                .GetProvider<T>()
                .GetLocalizations(
                    DefaultLocalizationsDirectoryName,
                    CustomLocalizationsDirectoryName);

            if (localizations == null)
                return;

            Localizations = new ReadOnlyDictionary<string, ILocalizationModule>(
                localizations);

            OnLocalizationsLoaded(localizations);
        }

        private static ILocalizationModule GetLocalizationModule(
            string cultureName)
        {
            using var @lock = SyncRoot.Lock();

            if (string.IsNullOrEmpty(cultureName))
                return null;

            if (Localizations.TryGetValue(cultureName, out var localizationModule))
            {
                if (IsValidLocalizationModule(localizationModule))
                    return localizationModule;

                return null;
            }

            OnLocalizedCultureNotFound(cultureName);

            return null;
        }



        private static bool SetDefaultLocalization(
            ILocalizationModule module)
        {
            using var @lock = SyncRoot.Lock();

            if (!IsValidLocalizationModule(module))
                return false;

            return true;
        }

        private static bool SetLocalization(
            ILocalizationModule module)
        {
            using var @lock = SyncRoot.Lock();

            if (!IsValidLocalizationModule(module))
                return false;

            return true;
        }



        public static void SetDefaultCulture(string cultureName)
        {
            if (string.IsNullOrEmpty(cultureName))
                return;

            CultureInfo culture;

            try
            {
                culture = CultureInfo.GetCultureInfo(cultureName);
            }
            catch (Exception)
            {
                var exception = new ArgumentException(
                    $"Culture named '{cultureName}' not found",
                    nameof(cultureName));
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message));
                throw exception;
            }

            SetDefaultCulture(culture);
        }
        public static void SetDefaultCulture(CultureInfo culture)
        {
            if (culture == null)
                return;
            if (Equals(culture, CultureInfo.InvariantCulture))
                return;

            DefaultCulture = CultureInfo.ReadOnly(
                culture);

            SwitchDefaultLocalization(GetDefaultCultureName());
        }


        public static void ReloadLocalizations<T>()
            where T : ILocalizationProvider
        {
            LoadLocalizations<T>();
        }

        public static string GetDefaultCultureName()
        {
            using var @lock = SyncRoot.Lock();

            if (Localizations.Count == 0)
            {
                OnLocalizationsNotFound();

                return null;
            }

            string cultureName;

            if (Localizations.ContainsKey(DefaultCulture.Name))
                cultureName = DefaultCulture.Name;
            else if (Localizations.ContainsKey("en-US"))
                cultureName = "en-US";
            else if (CurrentLocalization != null && Localizations.ContainsKey(CurrentLocalization.CultureName))
                cultureName = CurrentLocalization.CultureName;
            else
                cultureName = Localizations.Keys.FirstOrDefault();

            if (string.IsNullOrEmpty(cultureName))
            {
                OnLocalizedCultureNotFound(cultureName);

                return null;
            }

            return cultureName;
        }


        public static bool SwitchDefaultLocalization(
            string cultureName)
        {
            using var @lock = SyncRoot.Lock();

            if (Localizations.Count == 0)
            {
                OnLocalizationsNotFound();

                return false;
            }

            if (CurrentDefaultLocalization != null && CurrentDefaultLocalization.CultureName == cultureName)
                return true;

            var defaultModule = GetLocalizationModule(
                cultureName);

            if (defaultModule == null)
                return false;

            var setDefaultLocalizationSuccess = SetDefaultLocalization(
                defaultModule);

            if (!setDefaultLocalizationSuccess)
                return false;

            OnDefaultLocalizationChanged(defaultModule);

            if (CurrentLocalization == null)
            {
                var setLocalizationSuccess = SetLocalization(
                    defaultModule);

                if (!setLocalizationSuccess)
                    return false;

                OnLocalizationChanged(defaultModule);
            }

            OnLocalizationUpdated();

            return true;
        }

        public static bool SwitchLocalization(
            string cultureName)
        {
            using var @lock = SyncRoot.Lock();

            if (Localizations.Count == 0)
            {
                OnLocalizationsNotFound();

                return false;
            }

            if (CurrentLocalization != null && CurrentLocalization.CultureName == cultureName)
                return true;

            var module = GetLocalizationModule(
                cultureName);

            if (module == null)
                return false;

            var setLocalizationSuccess = SetLocalization(
                module);

            if (!setLocalizationSuccess)
                return false;

            OnLocalizationChanged(module);

            if (CurrentDefaultLocalization == null)
            {
                var defaultModule = GetLocalizationModule(
                    GetDefaultCultureName());

                if (defaultModule == null)
                    defaultModule = module;

                var setDefaultLocalizationSuccess = SetDefaultLocalization(
                    defaultModule);

                if (!setDefaultLocalizationSuccess)
                    return false;

                OnDefaultLocalizationChanged(defaultModule);
            }

            OnLocalizationUpdated();

            return true;
        }


        public static string GetLocalized(string key)
        {
            if (key == null)
                return null;

            if (CurrentLocalization != null
                && CurrentLocalization.Dictionary.Contains(key))
            {
                return (string)CurrentLocalization.Dictionary[key];
            }

            if (CurrentDefaultLocalization != null
                && CurrentDefaultLocalization.Dictionary.Contains(key))
            {
                return (string)CurrentDefaultLocalization.Dictionary[key];
            }

            return null;
        }

        public static bool TryGetLocalized(string key, out string value)
        {
            value = null;

            if (key == null)
                return false;

            if (CurrentLocalization != null
                && CurrentLocalization.Dictionary.Contains(key))
            {
                try
                {
                    value = (string)CurrentLocalization.Dictionary[key];

                    return true;
                }
                catch (Exception)
                {

                }
            }

            if (CurrentDefaultLocalization != null
                && CurrentDefaultLocalization.Dictionary.Contains(key))
            {
                try
                {
                    value = (string)CurrentDefaultLocalization.Dictionary[key];

                    return true;
                }
                catch (Exception)
                {

                }
            }

            value = null;

            return false;
        }
    }
}
