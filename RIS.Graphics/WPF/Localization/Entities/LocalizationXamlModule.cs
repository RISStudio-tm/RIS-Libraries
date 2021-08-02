// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace RIS.Graphics.WPF.Localization.Entities
{
    public class LocalizationXamlModule
    {
        private readonly List<LocalizationXamlFile> _files;
        public ReadOnlyCollection<LocalizationXamlFile> Files
        {
            get
            {
                return new ReadOnlyCollection<LocalizationXamlFile>(
                    _files);
            }
        }

        public string ElementName { get; private set; }

        public ResourceDictionary Dictionary { get; }

        public CultureInfo Culture { get; private set; }
        public string CultureName { get; private set; }
        private string _cultureNativeName;
        public string CultureNativeName
        {
            get
            {
                return _cultureNativeName;
            }
            private set
            {
                if (value.Length > 1)
                {
                    value = char.ToUpper(value[0], Culture)
                            + value.Remove(0, 1);
                }

                _cultureNativeName = value;
            }
        }

        public LocalizationXamlModule(IEnumerable<string> filesPaths,
            string elementName)
        {
            _files = new List<LocalizationXamlFile>();
            Dictionary = new ResourceDictionary();

            Load(filesPaths.ToArray(),
                elementName);
        }

        private void ValidateFile(LocalizationXamlFile file)
        {
            if (_files.Count == 0)
            {
                ElementName = file.ElementName;
                Culture = file.Culture;
                CultureName = file.CultureName;
                CultureNativeName = file.CultureNativeName;

                return;
            }

            if (ElementName != file.ElementName)
            {
                var exception = new Exception(
                    $"Element name '{ElementName}' in base dictionary file['{_files[0].Path}'] is not equal to '{file.ElementName}' in dictionary file['{file.Path}']");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message));
                throw exception;
            }
            if (!Equals(Culture, file.Culture))
            {
                var exception = new Exception(
                    $"Culture '{Culture}' in base dictionary file['{_files[0].Path}'] is not equal to '{file.Culture}' in dictionary file['{file.Path}']");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message));
                throw exception;
            }
            if (CultureName != file.CultureName)
            {
                var exception = new Exception(
                    $"Culture name '{CultureName}' in base dictionary file['{_files[0].Path}'] is not equal to '{file.CultureName}' in dictionary file['{file.Path}']");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message));
                throw exception;
            }
            if (CultureNativeName != file.CultureNativeName)
            {
                var exception = new Exception(
                    $"Culture native name '{CultureNativeName}' in base dictionary file['{_files[0].Path}'] is not equal to '{file.CultureNativeName}' in dictionary file['{file.Path}']");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message));
                throw exception;
            }
        }

        private void Load(string[] filesPaths,
            string elementName)
        {
            foreach (var filePath in filesPaths)
            {
                var file = new LocalizationXamlFile(
                    filePath, elementName);

                ValidateFile(file);

                _files.Add(
                    file);
                Dictionary.MergedDictionaries.Add(
                    file.Dictionary);
            }
        }
        private void Load(LocalizationXamlFile[] files)
        {
            foreach (var file in files)
            {
                ValidateFile(file);

                _files.Add(
                    file);
                Dictionary.MergedDictionaries.Add(
                    file.Dictionary);
            }
        }

        public void Merge(string filePath)
        {
            Merge(new[]
            {
                filePath
            });
        }
        public void Merge(IEnumerable<string> filesPaths)
        {
            Load(filesPaths.ToArray(),
                ElementName);
        }
        public void Merge(LocalizationXamlFile file)
        {
            Merge(new[]
            {
                file
            });
        }
        public void Merge(IEnumerable<LocalizationXamlFile> files)
        {
            Load(files.ToArray());
        }

        public void Remove(string filePath)
        {
            Remove(new[]
            {
                filePath
            });
        }
        public void Remove(IEnumerable<string> filesPaths)
        {
            var targetFilesPathsArray = filesPaths
                .ToArray();

            foreach (var file in Files)
            {
                foreach (var targetFilePath in targetFilesPathsArray)
                {
                    if (file == null || file.Path != targetFilePath)
                        continue;

                    _files.Remove(
                        file);
                    Dictionary.MergedDictionaries.Remove(
                        file.Dictionary);
                }
            }
        }
        public void Remove(LocalizationXamlFile file)
        {
            Remove(new[]
            {
                file
            });
        }
        public void Remove(IEnumerable<LocalizationXamlFile> files)
        {
            var targetFilesArray = files
                .ToArray();

            foreach (var file in Files)
            {
                foreach (var targetFile in targetFilesArray)
                {
                    if (file == null || file != targetFile)
                        continue;

                    _files.Remove(
                        file);
                    Dictionary.MergedDictionaries.Remove(
                        file.Dictionary);
                }
            }
        }
    }
}
