// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.IO;
using RIS.Localization.Entities;

namespace RIS.Localization.Providers
{
    public class XamlLocalizationProvider : ILocalizationProvider
    {
        private XamlLocalizationProvider()
        {

        }



        private static Dictionary<string, List<string>> GetLocalizationsPaths(
            string directoryBasePath)
        {
            var localizationsPaths = new Dictionary<string, List<string>>(10);
            var directory = directoryBasePath;

            if (string.IsNullOrEmpty(directory)
                || directory == "Unknown"
                || !Directory.Exists(directory))
            {
                return localizationsPaths;
            }

            if (!Directory.Exists(directory))
                return localizationsPaths;

            foreach (var filePath in Directory.EnumerateFiles(directory))
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var fileExtension = Path.GetExtension(filePath);

                    if (!fileName.StartsWith("Localization.")
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



        public Dictionary<string, ILocalizationModule> GetLocalizations(
            string defaultLocalizationsDirectoryName, string customLocalizationsDirectoryName)
        {
            var localizationsPaths = new Dictionary<string, List<string>>(10);

            void AddLocalizationPaths(KeyValuePair<string, List<string>> localizationPaths)
            {
                if (!localizationsPaths.ContainsKey(localizationPaths.Key))
                {
                    localizationsPaths.Add(
                        localizationPaths.Key,
                        new List<string>(5));
                }

                localizationsPaths[localizationPaths.Key].AddRange(
                    localizationPaths.Value);
            }

            if (!string.IsNullOrEmpty(defaultLocalizationsDirectoryName))
            {
                foreach (var localizationPaths in GetLocalizationsPaths(
                    defaultLocalizationsDirectoryName))
                {
                    AddLocalizationPaths(localizationPaths);
                }
            }

            if (!string.IsNullOrEmpty(customLocalizationsDirectoryName))
            {
                foreach (var localizationPaths in GetLocalizationsPaths(
                    customLocalizationsDirectoryName))
                {
                    AddLocalizationPaths(localizationPaths);
                }
            }

            var localizations = new Dictionary<string, ILocalizationModule>(10);

            foreach (var localizationPaths in localizationsPaths)
            {
                try
                {
                    var localization = new XamlLocalizationModule(
                        localizationPaths.Value);

                    localizations[localization.CultureName] = localization;
                }
                catch (Exception ex)
                {
                    Events.OnError(new RErrorEventArgs(ex, ex.Message));
                }
            }

            return localizations;
        }
    }
}
