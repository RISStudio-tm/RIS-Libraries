// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace RIS.Localization
{
    public interface ILocalizationDictionary : IDictionary<object, object>
    {
        ReadOnlyCollection<ILocalizationDictionary> MergedDictionaries { get; }



        bool AddMergedDictionary(ILocalizationDictionary dictionary);

        bool InsertMergedDictionary(int index, ILocalizationDictionary dictionary);

        bool RemoveMergedDictionary(ILocalizationDictionary dictionary);

        bool RemoveAtMergedDictionary(int index);
    }

    public interface ILocalizationProvider
    {
        Dictionary<string, ILocalizationModule> GetLocalizations(
            string defaultLocalizationsDirectoryName, string customLocalizationsDirectoryName);
    }

    public interface ILocalizationFile : IEquatable<ILocalizationFile>
    {
        string Path { get; }
        string Name { get; }
        string Extension { get; }

        ILocalizationDictionary Dictionary { get; }

        CultureInfo Culture { get; }
        string CultureName { get; }
        string CultureNativeName
        {
            get
            {
                var name = Culture.NativeName;

                if (name.Length > 1)
                {
                    name = char.ToUpper(name[0], Culture ?? CultureInfo.InvariantCulture)
                           + name.Remove(0, 1);
                }

                return name;
            }
        }
    }

    public interface ILocalizationModule : IEquatable<ILocalizationModule>
    {
        ReadOnlyCollection<ILocalizationFile> Files { get; }

        ILocalizationDictionary Dictionary { get; }

        CultureInfo Culture { get; }
        string CultureName { get; }
        string CultureNativeName
        {
            get
            {
                var name = Culture.NativeName;

                if (name.Length > 1)
                {
                    name = char.ToUpper(name[0], Culture ?? CultureInfo.InvariantCulture)
                           + name.Remove(0, 1);
                }

                return name;
            }
        }



        void Merge(string filePath)
        {
            Merge(new[]
            {
                filePath
            });
        }
        void Merge(IEnumerable<string> filesPaths);
        void Merge(ILocalizationFile file)
        {
            Merge(new[]
            {
                file
            });
        }
        void Merge(IEnumerable<ILocalizationFile> files);

        void Remove(string filePath)
        {
            Remove(new[]
            {
                filePath
            });
        }
        void Remove(IEnumerable<string> filesPaths);
        void Remove(ILocalizationFile file)
        {
            Remove(new[]
            {
                file
            });
        }
        void Remove(IEnumerable<ILocalizationFile> files);
    }
}
