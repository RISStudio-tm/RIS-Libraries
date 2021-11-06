// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;

namespace RIS.Localization.Entities
{
    public class XamlLocalizationFile : ILocalizationFile, IEquatable<XamlLocalizationFile>
    {
        public string Path { get; private set; }
        public string Name { get; private set; }
        public string Extension { get; private set; }

        public ILocalizationDictionary Dictionary { get; private set; }

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



        public XamlLocalizationFile(string filePath)
        {
            Load(filePath);
        }



        private void Load(string path)
        {
            if (!System.IO.Path.IsPathRooted(path))
            {
                var exception = new ArgumentException(
                    $"Path['{path}'] must contain the root",
                    nameof(path));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }
            if (System.IO.Path.GetFileName(path) == null)
            {
                var exception = new ArgumentException(
                    $"Path['{path}'] must refer to the file",
                    nameof(path));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }
            if (!File.Exists(path))
            {
                var exception = new FileNotFoundException(
                    $"File['{path}'] not found");
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            Path = path;

            var extension = System.IO.Path.GetExtension(path);

            if (string.IsNullOrEmpty(extension))
            {
                var exception = new ArgumentException(
                    $"File['{path}'] extension must not be null or empty",
                    nameof(path));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }
            if (extension != ".xaml")
            {
                var exception = new ArgumentException(
                    $"File['{path}'] must have an extension '.xaml'",
                    nameof(path));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            Extension = extension;

            var name = System.IO.Path.GetFileNameWithoutExtension(path);

            if (string.IsNullOrEmpty(name))
            {
                var exception = new ArgumentException(
                    $"File['{path}'] name must not be null or empty",
                    nameof(path));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }
            if (!name.StartsWith("Localization."))
            {
                var exception = new ArgumentException(
                    $"File['{path}'] name must be in the format [element name + '.' + culture name] " +
                    "(In this case - ['Localization.' + culture name]) " +
                    "(culture name must be in the ISO 639 format (for example, 'en-US' or 'ru-RU'))",
                    nameof(path));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            Name = name;

            var separatorIndex = name.IndexOf('.');
            var cultureName = name[(separatorIndex + 1)..];
            CultureInfo culture;

            try
            {
                culture = CultureInfo.GetCultureInfo(cultureName);
            }
            catch (Exception)
            {
                var exception = new ArgumentException(
                    $"Culture named '{cultureName}' for file['{path}'] not found",
                    nameof(path));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            Culture = culture;
            CultureName = cultureName;
            CultureNativeName = culture.NativeName;

            ResourceDictionary dictionary;

            try
            {
                dictionary = new ResourceDictionary
                {
                    Source = new Uri(path)
                };
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(
                    ex, ex.Message));
                throw;
            }

            Dictionary = LocalizationResourceDictionary.From(
                dictionary);

            if (!dictionary.Contains("ResourceDictionaryName"))
            {
                var exception = new KeyNotFoundException(
                    $"Dictionary file['{path}'] does not contain 'ResourceDictionaryName' definition");
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            var dictionaryName = dictionary["ResourceDictionaryName"]
                .ToString();

            if (string.IsNullOrWhiteSpace(dictionaryName))
            {
                var exception = new Exception(
                    $"ResourceDictionaryName value in file['{path}'] must not be null or empty");
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }
            if (dictionaryName != "localization-xaml")
            {
                var exception = new Exception(
                    $"The dictionary file['{path}'] is not a localization dictionary " +
                    "(The ResourceDictionaryName value must be 'localization-xaml')");
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }
        }



#pragma warning disable SS008 // GetHashCode() refers to mutable, static, or constant member
        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode()
        {
            return HashCode.Combine(Path, Name,
                Extension, CultureName);
        }
        // ReSharper restore NonReadonlyMemberInGetHashCode
#pragma warning restore SS008 // GetHashCode() refers to mutable, static, or constant member

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (GetType() != obj.GetType())
                return false;

            return Equals((XamlLocalizationFile)obj);
        }
        public bool Equals(ILocalizationFile obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (GetType() != obj.GetType())
                return false;

            return Equals((XamlLocalizationFile)obj);
        }
        public bool Equals(XamlLocalizationFile obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return Path == obj.Path
                   && Name == obj.Name
                   && Extension == obj.Extension
                   && CultureName == obj.CultureName;
        }



        public static bool operator ==(XamlLocalizationFile obj1, XamlLocalizationFile obj2)
        {
            return obj1?.Equals(obj2) ?? false;
        }
        public static bool operator !=(XamlLocalizationFile obj1, XamlLocalizationFile obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
