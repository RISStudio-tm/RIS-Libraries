// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RIS.Graphics.WPF.Localization.Entities;

namespace RIS.Graphics.WPF.Localization
{
    public class LocalizationChangedEventArgs : EventArgs
    {
        public LocalizationXamlModule OldLocalization { get; }
        public LocalizationXamlModule NewLocalization { get; }

        public LocalizationChangedEventArgs(
            LocalizationXamlModule oldLocalization,
            LocalizationXamlModule newLocalization)
        {
            OldLocalization = oldLocalization;
            NewLocalization = newLocalization;
        }
    }

    public class LocalizationLoadedEventArgs : EventArgs
    {
        public ReadOnlyDictionary<string, LocalizationXamlModule> Localizations { get; }

        public LocalizationLoadedEventArgs(
            Dictionary<string, LocalizationXamlModule> localizations)
        {
            Localizations = new ReadOnlyDictionary<string, LocalizationXamlModule>(
                localizations);
        }
    }

    public class LocalizationFileNotFoundEventArgs : EventArgs
    {
        public string FilePath { get; }

        public LocalizationFileNotFoundEventArgs(string filePath)
        {
            FilePath = filePath;
        }
    }

    public class LocalizedCultureNotFoundEventArgs : EventArgs
    {
        public string CultureName { get; }

        public LocalizedCultureNotFoundEventArgs(string cultureName)
        {
            CultureName = cultureName;
        }
    }
}
