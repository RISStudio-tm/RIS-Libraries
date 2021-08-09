// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace RIS.Localization.Entities
{
    public class XamlLocalizationModule : ILocalizationModule, IEquatable<XamlLocalizationModule>
    {
        private readonly List<ILocalizationFile> _files;
        public ReadOnlyCollection<ILocalizationFile> Files
        {
            get
            {
                return new ReadOnlyCollection<ILocalizationFile>(
                    _files);
            }
        }

        public ILocalizationDictionary Dictionary { get; }

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
                if (value != null && value.Length > 1)
                {
                    value = char.ToUpper(value[0], Culture ?? CultureInfo.InvariantCulture)
                            + value.Remove(0, 1);
                }

                _cultureNativeName = value;
            }
        }



        private XamlLocalizationModule()
        {
            _files = new List<ILocalizationFile>();
            Dictionary = LocalizationResourceDictionary.From(
                new ResourceDictionary());
        }
        public XamlLocalizationModule(IEnumerable<string> filesPaths)
            : this()
        {
            Load(filesPaths.ToArray());
        }
        public XamlLocalizationModule(IEnumerable<XamlLocalizationFile> files)
            : this()
        {
            Load(files.ToArray());
        }



        private void ValidateFile(XamlLocalizationFile file)
        {
            if (_files.Count == 0)
            {
                Culture = file.Culture;
                CultureName = file.CultureName;
                CultureNativeName = Culture.NativeName;

                return;
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
        }

        private void Load(string[] filesPaths)
        {
            foreach (var filePath in filesPaths)
            {
                var file = new XamlLocalizationFile(
                    filePath);

                ValidateFile(file);

                _files.Add(
                    file);
                Dictionary.MergedDictionaries.Add(
                    file.Dictionary);
            }
        }
        private void Load(XamlLocalizationFile[] files)
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
            var targetFilesPathsArray = filesPaths
                .ToArray();

            if (targetFilesPathsArray.Length == 0)
                return;

            Load(targetFilesPathsArray);

            LocalizationManager.OnLocalizationUpdated();
        }
        public void Merge(ILocalizationFile file)
        {
            Merge(new[]
            {
                file
            });
        }
        public void Merge(IEnumerable<ILocalizationFile> files)
        {
            Merge(files.OfType<XamlLocalizationFile>());
        }
        public void Merge(XamlLocalizationFile file)
        {
            Merge(new[]
            {
                file
            });
        }
        public void Merge(IEnumerable<XamlLocalizationFile> files)
        {
            var targetFilesArray = files
                .ToArray();

            if (targetFilesArray.Length == 0)
                return;

            Load(targetFilesArray);

            LocalizationManager.OnLocalizationUpdated();
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

            if (targetFilesPathsArray.Length == 0)
                return;

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

            LocalizationManager.OnLocalizationUpdated();
        }
        public void Remove(ILocalizationFile file)
        {
            Remove(new[]
            {
                file
            });
        }
        public void Remove(IEnumerable<ILocalizationFile> files)
        {
            Remove(files.OfType<XamlLocalizationFile>());
        }
        public void Remove(XamlLocalizationFile file)
        {
            Remove(new[]
            {
                file
            });
        }
        public void Remove(IEnumerable<XamlLocalizationFile> files)
        {
            var targetFilesArray = files
                .ToArray();

            if (targetFilesArray.Length == 0)
                return;

            foreach (var file in Files)
            {
                foreach (var targetFile in targetFilesArray)
                {
                    if (file == null || !file.Equals(targetFile))
                        continue;

                    _files.Remove(
                        file);
                    Dictionary.MergedDictionaries.Remove(
                        file.Dictionary);
                }
            }

            LocalizationManager.OnLocalizationUpdated();
        }



#pragma warning disable SS008 // GetHashCode() refers to mutable, static, or constant member
        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode()
        {
            return HashCode.Combine(CultureName);
        }
        // ReSharper enable NonReadonlyMemberInGetHashCode
#pragma warning restore SS008 // GetHashCode() refers to mutable, static, or constant member

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (GetType() != obj.GetType())
                return false;

            return Equals((XamlLocalizationModule)obj);
        }
        public bool Equals(ILocalizationModule obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (GetType() != obj.GetType())
                return false;

            return Equals((XamlLocalizationModule)obj);
        }
        public bool Equals(XamlLocalizationModule obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            if (_files.Count != obj._files.Count)
                return false;
            for (int i = 0; i < _files.Count; ++i)
            {
                if (_files[i] == null || !_files[i].Equals(obj._files[i]))
                    return false;
            }
            if (CultureName != obj.CultureName)
                return false;

            return true;
        }



        public static bool operator ==(XamlLocalizationModule obj1, XamlLocalizationModule obj2)
        {
            return obj1?.Equals(obj2) ?? false;
        }
        public static bool operator !=(XamlLocalizationModule obj1, XamlLocalizationModule obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
