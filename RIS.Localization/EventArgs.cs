// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RIS.Localization
{
    public class LocalizationChangedEventArgs : EventArgs
    {
        public ILocalizationModule OldLocalization { get; }
        public ILocalizationModule NewLocalization { get; }

        public LocalizationChangedEventArgs(
            ILocalizationModule oldLocalization,
            ILocalizationModule newLocalization)
        {
            OldLocalization = oldLocalization;
            NewLocalization = newLocalization;
        }
    }

    public class LocalizationLoadedEventArgs : EventArgs
    {
        public ReadOnlyDictionary<string, ILocalizationModule> Localizations { get; }

        public LocalizationLoadedEventArgs(
            IDictionary<string, ILocalizationModule> localizations)
        {
            Localizations = new ReadOnlyDictionary<string, ILocalizationModule>(
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
