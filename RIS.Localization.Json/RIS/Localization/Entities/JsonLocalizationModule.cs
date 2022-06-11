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
    public class JsonLocalizationModule : ILocalizationModule, IEquatable<JsonLocalizationModule>
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



        private JsonLocalizationModule()
        {
            _files = new List<ILocalizationFile>();
            Dictionary = LocalizationDictionary.From(
                new Dictionary<object, object>());
        }
        public JsonLocalizationModule(IEnumerable<string> filesPaths)
            : this()
        {
            Load(filesPaths.ToArray());
        }
        public JsonLocalizationModule(IEnumerable<JsonLocalizationFile> files)
            : this()
        {
            Load(files.ToArray());
        }



        private void ValidateFile(JsonLocalizationFile file)
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
                var file = new JsonLocalizationFile(
                    filePath);

                ValidateFile(file);

                _files.Add(
                    file);
                Dictionary.AddMergedDictionary(
                    file.Dictionary);
            }
        }
        private void Load(JsonLocalizationFile[] files)
        {
            foreach (var file in files)
            {
                ValidateFile(file);

                _files.Add(
                    file);
                Dictionary.AddMergedDictionary(
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

            LocalizationManager.OnLocalizationUpdated(this);
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
            Merge(files.OfType<JsonLocalizationFile>());
        }
        public void Merge(JsonLocalizationFile file)
        {
            Merge(new[]
            {
                file
            });
        }
        public void Merge(IEnumerable<JsonLocalizationFile> files)
        {
            var targetFilesArray = files
                .ToArray();

            if (targetFilesArray.Length == 0)
                return;

            Load(targetFilesArray);

            LocalizationManager.OnLocalizationUpdated(this);
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
                    Dictionary.RemoveMergedDictionary(
                        file.Dictionary);
                }
            }

            LocalizationManager.OnLocalizationUpdated(this);
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
            Remove(files.OfType<JsonLocalizationFile>());
        }
        public void Remove(JsonLocalizationFile file)
        {
            Remove(new[]
            {
                file
            });
        }
        public void Remove(IEnumerable<JsonLocalizationFile> files)
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
                    Dictionary.RemoveMergedDictionary(
                        file.Dictionary);
                }
            }

            LocalizationManager.OnLocalizationUpdated(this);
        }



#pragma warning disable SS008 // GetHashCode() refers to mutable, static, or constant member
        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode()
        {
            return HashCode.Combine(CultureName);
        }
        // ReSharper restore NonReadonlyMemberInGetHashCode
#pragma warning restore SS008 // GetHashCode() refers to mutable, static, or constant member

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (GetType() != obj.GetType())
                return false;

            return Equals((JsonLocalizationModule)obj);
        }
        public bool Equals(ILocalizationModule obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (GetType() != obj.GetType())
                return false;

            return Equals((JsonLocalizationModule)obj);
        }
        public bool Equals(JsonLocalizationModule obj)
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



        public static bool operator ==(JsonLocalizationModule obj1, JsonLocalizationModule obj2)
        {
            return obj1?.Equals(obj2) ?? false;
        }
        public static bool operator !=(JsonLocalizationModule obj1, JsonLocalizationModule obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
