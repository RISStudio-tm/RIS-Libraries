// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using RIS.Localization.Providers;
using RIS.Synchronization;

namespace RIS.Localization
{
    public class LocalizationFactory
    {
        public event EventHandler<LocalizationChangedEventArgs> DefaultLocalizationChanged;
        public event EventHandler<LocalizationChangedEventArgs> LocalizationChanged;
        public event EventHandler<LocalizationLoadedEventArgs> LocalizationsLoaded;
        public event EventHandler<LocalizationFileNotFoundEventArgs> LocalizationFileNotFound;
        public event EventHandler<LocalizedCultureNotFoundEventArgs> LocalizedCultureNotFound;
        public event EventHandler<LocalizationEventArgs> LocalizationsNotFound;

        public event EventHandler<LocalizationEventArgs> LocalizationUpdated;



        private string DefaultLocalizationDirectoryPath { get; }
        private string CustomLocalizationDirectoryPath { get; }

        public AsyncLock SyncRoot { get; }
        public string AssemblyName { get; }
        public string Name { get; }
        private ILocalizationModule _currentDefaultLocalization;
        public ILocalizationModule CurrentDefaultLocalization
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
        private ILocalizationModule _currentLocalization;
        public ILocalizationModule CurrentLocalization
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
        public ReadOnlyDictionary<string, ILocalizationModule> Localizations { get; private set; }

        public CultureInfo DefaultCulture { get; private set; }



        private LocalizationFactory()
        {
            SyncRoot = new AsyncLock();
            CurrentDefaultLocalization = null;
            CurrentLocalization = null;
            Localizations = new ReadOnlyDictionary<string, ILocalizationModule>(
                new Dictionary<string, ILocalizationModule>());

            DefaultCulture = new CultureInfo("en-US");
        }
        private LocalizationFactory(
            string assemblyName, string factoryName)
            : this()
        {
            var baseAppDirectory = Environment.ExecAppDirectoryName;
            var baseProcessDirectory = Environment.ExecProcessDirectoryName;

            if (string.IsNullOrEmpty(baseAppDirectory) || baseAppDirectory == "Unknown")
                baseAppDirectory = string.Empty;
            if (string.IsNullOrEmpty(baseProcessDirectory) || baseProcessDirectory == "Unknown")
                baseProcessDirectory = string.Empty;

            AssemblyName = assemblyName;
            Name = factoryName;
            DefaultLocalizationDirectoryPath = Path.Combine(
                baseAppDirectory, "localizations", factoryName, "default");
            CustomLocalizationDirectoryPath = Path.Combine(
                baseProcessDirectory, "localizations", factoryName, "custom");

            if (!Directory.Exists(DefaultLocalizationDirectoryPath))
                Directory.CreateDirectory(DefaultLocalizationDirectoryPath);
            if (!Directory.Exists(CustomLocalizationDirectoryPath))
                Directory.CreateDirectory(CustomLocalizationDirectoryPath);

            LocalizationManager.AddLocalizationFactory(assemblyName, this);
        }

        ~LocalizationFactory()
        {
            LocalizationManager.RemoveLocalizationFactory(AssemblyName, this);
        }



        private void OnDefaultLocalizationChanged(
            ILocalizationModule newLocalization)
        {
            var oldLocalization = Interlocked.Exchange(
                ref _currentDefaultLocalization, newLocalization);

            if (oldLocalization != null && oldLocalization.Equals(newLocalization))
                return;

            DefaultLocalizationChanged?.Invoke(this,
                new LocalizationChangedEventArgs(this,
                    oldLocalization, newLocalization));
            LocalizationManager.OnDefaultLocalizationChanged(this,
                new LocalizationChangedEventArgs(this,
                    oldLocalization, newLocalization));
        }

        private void OnLocalizationChanged(
            ILocalizationModule newLocalization)
        {
            var oldLocalization = Interlocked.Exchange(
                ref _currentLocalization, newLocalization);

            if (oldLocalization != null && oldLocalization.Equals(newLocalization))
                return;

            LocalizationChanged?.Invoke(this,
                new LocalizationChangedEventArgs(this,
                    oldLocalization, newLocalization));
            LocalizationManager.OnLocalizationChanged(this,
                new LocalizationChangedEventArgs(this,
                    oldLocalization, newLocalization));

            Thread.CurrentThread.CurrentCulture = newLocalization?.Culture ?? DefaultCulture;
            Thread.CurrentThread.CurrentUICulture = newLocalization?.Culture ?? DefaultCulture;

            SynchronizationContext.Current?.Send((_) =>
            {
                Thread.CurrentThread.CurrentCulture = newLocalization?.Culture ?? DefaultCulture;
                Thread.CurrentThread.CurrentUICulture = newLocalization?.Culture ?? DefaultCulture;
            }, null);
        }

        private void OnLocalizationsLoaded(
            Dictionary<string, ILocalizationModule> localizations)
        {
            LocalizationsLoaded?.Invoke(this,
                new LocalizationLoadedEventArgs(this,
                    localizations));
            LocalizationManager.OnLocalizationsLoaded(this,
                new LocalizationLoadedEventArgs(this,
                    localizations));
        }

        private void OnLocalizationFileNotFound(
            string filePath)
        {
            LocalizationFileNotFound?.Invoke(this,
                new LocalizationFileNotFoundEventArgs(this,
                    filePath));
            LocalizationManager.OnLocalizationFileNotFound(this,
                new LocalizationFileNotFoundEventArgs(this,
                    filePath));

            var exception = new FileNotFoundException(
                $"Localization file['{filePath}'] not found");
            Events.OnError(new RErrorEventArgs(
                exception, exception.Message));
        }

        private void OnLocalizedCultureNotFound(
            string cultureName)
        {
            LocalizedCultureNotFound?.Invoke(this,
                new LocalizedCultureNotFoundEventArgs(this,
                    cultureName));
            LocalizationManager.OnLocalizedCultureNotFound(this,
                new LocalizedCultureNotFoundEventArgs(this,
                    cultureName));

            var exception = new CultureNotFoundException(
                $"Localized culture['{cultureName}'] not found");
            Events.OnError(new RErrorEventArgs(
                exception, exception.Message));
        }

        private void OnLocalizationsNotFound()
        {
            LocalizationsNotFound?.Invoke(this,
                new LocalizationEventArgs(this));
            LocalizationManager.OnLocalizationsNotFound(this,
                new LocalizationEventArgs(this));

            var exception = new Exception(
                "Localizations not found");
            Events.OnError(new RErrorEventArgs(
                exception, exception.Message));
        }


        public void OnLocalizationUpdated(
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
        public void OnLocalizationUpdated()
        {
            LocalizationUpdated?.Invoke(this,
                new LocalizationEventArgs(this));
            LocalizationManager.OnLocalizationUpdated(this,
                new LocalizationEventArgs(this));
        }



        private bool IsValidLocalizationModule(
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



        private void LoadLocalizations<T>()
            where T : ILocalizationProvider
        {
            using var @lock = SyncRoot.Lock();

            var localizations = LocalizationProviderStorage
                .GetProvider<T>()
                .GetLocalizations(
                    DefaultLocalizationDirectoryPath,
                    CustomLocalizationDirectoryPath);

            if (localizations == null)
                return;

            Localizations = new ReadOnlyDictionary<string, ILocalizationModule>(
                localizations);

            OnLocalizationsLoaded(localizations);
        }

        private ILocalizationModule GetLocalizationModule(
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



        private bool SetDefaultLocalization(
            ILocalizationModule module)
        {
            using var @lock = SyncRoot.Lock();

            if (!IsValidLocalizationModule(module))
                return false;

            return true;
        }

        private bool SetLocalization(
            ILocalizationModule module)
        {
            using var @lock = SyncRoot.Lock();

            if (!IsValidLocalizationModule(module))
                return false;

            return true;
        }


        public string GetLocalizationsDirectoryPath(
            LocalizationType localization)
        {
            return localization switch
            {
                LocalizationType.Default => DefaultLocalizationDirectoryPath,
                LocalizationType.Custom => CustomLocalizationDirectoryPath,
                _ => Path.Combine(Environment.ExecProcessDirectoryName, "localizations")
            };
        }


        public void SetDefaultCulture(
            string cultureName)
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
        public void SetDefaultCulture(
            CultureInfo culture)
        {
            if (culture == null)
                return;
            if (Equals(culture, CultureInfo.InvariantCulture))
                return;

            DefaultCulture = CultureInfo.ReadOnly(
                culture);

            SwitchDefaultLocalization(GetDefaultCultureName());
        }


        public void ReloadLocalizations<T>()
            where T : ILocalizationProvider
        {
            LoadLocalizations<T>();
        }

        public string GetDefaultCultureName()
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


        public bool SwitchDefaultLocalization(
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

        public bool SwitchLocalization(
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


        public string GetLocalized(
            string key)
        {
            if (key == null)
                return null;

            if (CurrentLocalization != null
                && CurrentLocalization.Dictionary.ContainsKey(key))
            {
                return (string)CurrentLocalization.Dictionary[key];
            }

            if (CurrentDefaultLocalization != null
                && CurrentDefaultLocalization.Dictionary.ContainsKey(key))
            {
                return (string)CurrentDefaultLocalization.Dictionary[key];
            }

            return null;
        }

        public bool TryGetLocalized(
            string key, out string value)
        {
            value = null;

            if (key == null)
                return false;

            if (CurrentLocalization != null
                && CurrentLocalization.Dictionary.ContainsKey(key))
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
                && CurrentDefaultLocalization.Dictionary.ContainsKey(key))
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



        public static LocalizationFactory Create()
        {
            var assemblyName = Assembly.GetCallingAssembly()
                .GetName().Name;
            var factoryName = Environment.Process
                .ProcessName;

            return CreateInternal(
                assemblyName, factoryName);
        }
        public static LocalizationFactory Create(
            string assemblyName)
        {
            var factoryName = Environment.Process
                .ProcessName;

            return CreateInternal(
                assemblyName, factoryName);
        }
        public static LocalizationFactory Create(
            string assemblyName, string factoryName)
        {
            return CreateInternal(
                assemblyName, factoryName);
        }
        private static LocalizationFactory CreateInternal(
            string assemblyName, string factoryName)
        {
            return new LocalizationFactory(
                assemblyName, factoryName);
        }
    }
}
