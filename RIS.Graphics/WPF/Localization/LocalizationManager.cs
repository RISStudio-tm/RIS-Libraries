// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using RIS.Graphics.WPF.Localization.Entities;
using RIS.Synchronization;

namespace RIS.Graphics.WPF.Localization
{
    public static class LocalizationManager
    {
        public static event EventHandler<LocalizationChangedEventArgs> LocalizationChanged;
        public static event EventHandler<LocalizationLoadedEventArgs> LocalizationsLoaded;
        public static event EventHandler<LocalizationFileNotFoundEventArgs> LocalizationFileNotFound;
        public static event EventHandler<LocalizedCultureNotFoundEventArgs> LocalizedCultureNotFound;
        public static event EventHandler<EventArgs> LocalizationsNotFound;


        private static LocalizationSource Source { get; set; }


        public static AsyncLock SyncRoot { get; }
        public static string GeneralElementName { get; }
        public static string DefaultLocalizationsDirectoryName { get; }
        public static string CustomLocalizationsDirectoryName { get; }
        private static LocalizationXamlModule _currentLocalization;
        public static LocalizationXamlModule CurrentLocalization
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
        public static ReadOnlyDictionary<string, LocalizationXamlModule> Localizations { get; private set; }

        public static CultureInfo DefaultCulture { get; private set; }
        public static FrameworkElement SourceElement
        {
            get
            {
                return Source.Element;
            }
        }



        static LocalizationManager()
        {
            var baseAppDirectory = Environment.ExecAppDirectoryName;
            var baseProcessDirectory = Environment.ExecProcessDirectoryName;

            if (string.IsNullOrEmpty(baseAppDirectory) || baseAppDirectory == "Unknown")
                return;
            if (string.IsNullOrEmpty(baseProcessDirectory) || baseProcessDirectory == "Unknown")
                return;

            SyncRoot = new AsyncLock();
            GeneralElementName = "General";
            DefaultLocalizationsDirectoryName = Path.Combine(baseAppDirectory,
                "localizations", "default");
            CustomLocalizationsDirectoryName = Path.Combine(baseProcessDirectory,
                "localizations");
            CurrentLocalization = null;
            Localizations = new ReadOnlyDictionary<string, LocalizationXamlModule>(
                new Dictionary<string, LocalizationXamlModule>());

            DefaultCulture = new CultureInfo("en-US");
            Source = LocalizationSource.From(
                Application.Current.MainWindow);

            if (!Directory.Exists(DefaultLocalizationsDirectoryName))
                Directory.CreateDirectory(DefaultLocalizationsDirectoryName);
            if (!Directory.Exists(CustomLocalizationsDirectoryName))
                Directory.CreateDirectory(CustomLocalizationsDirectoryName);

            ReloadLocalizations();
        }



        private static void OnLocalizationChanged(
            LocalizationXamlModule newLocalization)
        {
            var oldLocalization = Interlocked.Exchange(
                ref _currentLocalization, newLocalization);

            if (oldLocalization == newLocalization)
                return;

            LocalizationChanged?.Invoke(null,
                new LocalizationChangedEventArgs(oldLocalization, newLocalization));
        }

        private static void OnLocalizationsLoaded(
            Dictionary<string, LocalizationXamlModule> localizations)
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



        private static bool IsValidLocalizationModule(
            LocalizationXamlModule localizationModule)
        {
            var result = true;

            foreach (var localizationFile in localizationModule.Files)
            {
                if (File.Exists(localizationFile.Path))
                    continue;

                OnLocalizationFileNotFound(localizationFile.Path);

                result = false;
            }

            return result;
        }


        private static Dictionary<string, List<string>> GetLocalizationsPaths(
            string directoryBasePath, string directoryRelativePath = null)
        {
            using var @lock = SyncRoot.Lock();

            var localizationsPaths = new Dictionary<string, List<string>>(10);
            var directory = directoryBasePath;

            if (string.IsNullOrEmpty(directory)
                || directory == "Unknown"
                || !Directory.Exists(directory))
            {
                return localizationsPaths;
            }

            if (!string.IsNullOrEmpty(directoryRelativePath))
            {
                directory = Path.Combine(directory,
                    directoryRelativePath);
            }

            if (!Directory.Exists(directory))
                return localizationsPaths;

            var elementName = GeneralElementName;

            foreach (var filePath in Directory.EnumerateFiles(directory))
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var fileExtension = Path.GetExtension(filePath);

                    if (!fileName.StartsWith($"{elementName}.")
                        || fileExtension != ".xaml")
                    {
                        continue;
                    }

                    var separatorIndex = fileName.IndexOf('.');
                    var cultureName = fileName[(separatorIndex + 1)..];

                    if (!localizationsPaths.TryGetValue(cultureName, out _))
                    {
                        localizationsPaths.Add(
                            cultureName,
                            new List<string>());
                    }

                    localizationsPaths[cultureName].Add(
                        filePath);
                }
                catch (Exception ex)
                {
                    Events.OnError(new RErrorEventArgs(ex, ex.Message));
                }
            }

            return localizationsPaths;
        }

        private static Dictionary<string, LocalizationXamlModule> GetLocalizations()
        {
            using var @lock = SyncRoot.Lock();

            var localizationsPaths = new Dictionary<string, List<string>>(10);

            void AddLocalizationPaths(KeyValuePair<string, List<string>> localizationPaths)
            {
                if (!localizationsPaths.TryGetValue(localizationPaths.Key, out _))
                {
                    localizationsPaths.Add(
                        localizationPaths.Key,
                        new List<string>(5));
                }

                localizationsPaths[localizationPaths.Key].AddRange(
                    localizationPaths.Value);
            }

            foreach (var localizationPaths in GetLocalizationsPaths(
                DefaultLocalizationsDirectoryName))
            {
                AddLocalizationPaths(localizationPaths);
            }

            foreach (var localizationPaths in GetLocalizationsPaths(
                CustomLocalizationsDirectoryName))
            {
                AddLocalizationPaths(localizationPaths);
            }

            var elementName = GeneralElementName;
            var localizations = new Dictionary<string, LocalizationXamlModule>();

            foreach (var localizationPaths in localizationsPaths)
            {
                try
                {
                    var localization = new LocalizationXamlModule(
                        localizationPaths.Value, elementName);

                    localizations[localization.CultureName] = localization;
                }
                catch (Exception ex)
                {
                    Events.OnError(new RErrorEventArgs(ex, ex.Message));
                }
            }

            return localizations;
        }

        private static void LoadLocalizations()
        {
            using var @lock = SyncRoot.Lock();

            var localizations = GetLocalizations();

            if (localizations == null)
                return;

            Localizations = new ReadOnlyDictionary<string, LocalizationXamlModule>(
                localizations);

            OnLocalizationsLoaded(localizations);
        }

        private static LocalizationXamlModule GetLocalizationModule(
            string cultureName)
        {
            using var @lock = SyncRoot.Lock();

            if (Localizations.TryGetValue(cultureName, out var localizationModule))
            {
                if (IsValidLocalizationModule(localizationModule))
                    return localizationModule;

                return null;
            }

            OnLocalizedCultureNotFound(cultureName);

            return null;
        }


        private static bool SetDefaultLocalization<T>(T source,
            LocalizationXamlModule localizationModule)
            where T : LocalizationSource
        {
            using var @lock = SyncRoot.Lock();

            if (!IsValidLocalizationModule(localizationModule))
                return false;

            var dictionaryIndex = -1;

            for (var i = 0; i < source.Element.Resources.MergedDictionaries.Count; ++i)
            {
                ResourceDictionary dictionary =
                    source.Element.Resources.MergedDictionaries[i];

                if (!dictionary.Contains("ResourceDictionaryName")
                    || dictionary["ResourceDictionaryName"].ToString() != "localization-xaml")
                {
                    continue;
                }

                dictionaryIndex = i;

                break;
            }

            if (dictionaryIndex == -1)
            {
                source.Element.Resources.MergedDictionaries.Add(
                    localizationModule.Dictionary);

                return true;
            }

            source.Element.Resources.MergedDictionaries[dictionaryIndex] =
                localizationModule.Dictionary;

            return true;
        }

        private static bool SetLocalization<T>(T source,
            LocalizationXamlModule localizationModule)
            where T: LocalizationSource
        {
            using var @lock = SyncRoot.Lock();

            if (!IsValidLocalizationModule(localizationModule))
                return false;

            var dictionaryIndex = -1;
            var defaultDictionaryFound = false;

            for (var i = 0; i < source.Element.Resources.MergedDictionaries.Count; ++i)
            {
                ResourceDictionary dictionary =
                    source.Element.Resources.MergedDictionaries[i];

                if (!dictionary.Contains("ResourceDictionaryName")
                    || dictionary["ResourceDictionaryName"].ToString() != "localization-xaml")
                {
                    continue;
                }

                if (!defaultDictionaryFound)
                {
                    defaultDictionaryFound = true;

                    continue;
                }

                dictionaryIndex = i;

                break;
            }

            if (dictionaryIndex == -1)
            {
                if (!defaultDictionaryFound)
                {
                    ResourceDictionary defaultDictionary = null;

                    var defaultCultureName = GetDefaultCultureName();

                    if (defaultCultureName != null)
                    {
                        defaultDictionary = GetLocalizationModule(defaultCultureName)?
                            .Dictionary;
                    }

                    defaultDictionary ??= localizationModule.Dictionary;

                    source.Element.Resources.MergedDictionaries.Add(
                        defaultDictionary);
                }

                source.Element.Resources.MergedDictionaries.Add(
                    localizationModule.Dictionary);

                return true;
            }

            source.Element.Resources.MergedDictionaries[dictionaryIndex] =
                localizationModule.Dictionary;

            return true;
        }



        public static void SetSourceElement<T>(T element)
            where T : FrameworkElement
        {
            using var @lock = SyncRoot.Lock();

            if (element == null)
                return;

            if (Equals(element, Source.Element))
                return;

            var dictionaries = new List<ResourceDictionary>(2);

            if (Source != null)
            {
                for (var i = 0; i < Source.Element.Resources.MergedDictionaries.Count; ++i)
                {
                    var dictionary =
                        Source.Element.Resources.MergedDictionaries[i];

                    if (!dictionary.Contains("ResourceDictionaryName")
                        || dictionary["ResourceDictionaryName"].ToString() != "localization-xaml")
                    {
                        continue;
                    }

                    dictionaries.Add(
                        dictionary);

                    Source.Element.Resources.MergedDictionaries.Remove(
                        dictionary);

                    break;
                }
            }

            Source = LocalizationSource.From(
                element);

            for (var i = 0; i < dictionaries.Count; ++i)
            {
                var dictionary = dictionaries[i];

                Source.Element.Resources.MergedDictionaries.Add(
                    dictionary);
            }
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
        }


        public static void ReloadLocalizations()
        {
            LoadLocalizations();
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

            var localizationModule = GetLocalizationModule(
                cultureName);

            if (localizationModule == null)
                return false;

            var currentCultureName = CurrentLocalization.CultureName;
            var currentLocalizationModule = GetLocalizationModule(
                currentCultureName);

            if (currentLocalizationModule == null)
                return false;

            var source = Source;

            var setDefaultLocalizationSuccess = SetDefaultLocalization(
                source, localizationModule);

            if (!setDefaultLocalizationSuccess)
                return false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Thread.CurrentThread.CurrentCulture = currentLocalizationModule.Culture;
                Thread.CurrentThread.CurrentUICulture = currentLocalizationModule.Culture;
            });

            OnLocalizationChanged(localizationModule);

            var setCurrentLocalizationSuccess = SetLocalization(
                source, currentLocalizationModule);

            if (!setCurrentLocalizationSuccess)
                return false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Thread.CurrentThread.CurrentCulture = currentLocalizationModule.Culture;
                Thread.CurrentThread.CurrentUICulture = currentLocalizationModule.Culture;
            });

            OnLocalizationChanged(currentLocalizationModule);

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

            var localizationModule = GetLocalizationModule(
                cultureName);

            if (localizationModule == null)
                return false;

            var source = Source;

            var setLocalizationSuccess = SetLocalization(
                source, localizationModule);

            if (!setLocalizationSuccess)
                return false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Thread.CurrentThread.CurrentCulture = localizationModule.Culture;
                Thread.CurrentThread.CurrentUICulture = localizationModule.Culture;
            });

            OnLocalizationChanged(localizationModule);

            return true;
        }


        public static string GetLocalized(string key)
        {
            return GetLocalized<string>(key);
        }
        public static TOut GetLocalized<TOut>(string key)
        {
            if (key == null)
                return default;
            if (CurrentLocalization == null)
                return default;
            if (!CurrentLocalization.Dictionary.Contains(key))
                return default;

            return (TOut)CurrentLocalization.Dictionary[key];
        }

        public static bool TryGetLocalized(string key, out string value)
        {
            return TryGetLocalized<string>(key, out value);
        }
        public static bool TryGetLocalized<TOut>(string key, out TOut value)
        {
            value = default;

            if (key == null)
                return false;
            if (CurrentLocalization == null)
                return false;
            if (!CurrentLocalization.Dictionary.Contains(key))
                return false;

            try
            {
                value = (TOut)CurrentLocalization.Dictionary[key];

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
