// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RIS.Localization
{
    public class LocalizationEventArgs : EventArgs
    {
        public LocalizationFactory Factory { get; }

        public LocalizationEventArgs(
            LocalizationFactory factory)
        {
            Factory = factory;
        }
    }

    public class LocalizationChangedEventArgs : LocalizationEventArgs
    {
        public ILocalizationModule OldLocalization { get; }
        public ILocalizationModule NewLocalization { get; }

        public LocalizationChangedEventArgs(
            LocalizationFactory factory,
            ILocalizationModule oldLocalization,
            ILocalizationModule newLocalization)
            : base(factory)
        {
            OldLocalization = oldLocalization;
            NewLocalization = newLocalization;
        }
    }

    public class LocalizationLoadedEventArgs : LocalizationEventArgs
    {
        public ReadOnlyDictionary<string, ILocalizationModule> Localizations { get; }

        public LocalizationLoadedEventArgs(
            LocalizationFactory factory,
            IDictionary<string, ILocalizationModule> localizations)
            : base(factory)
        {
            Localizations = new ReadOnlyDictionary<string, ILocalizationModule>(
                localizations);
        }
    }

    public class LocalizationFileNotFoundEventArgs : LocalizationEventArgs
    {
        public string FilePath { get; }

        public LocalizationFileNotFoundEventArgs(
            LocalizationFactory factory,
            string filePath)
            : base(factory)
        {
            FilePath = filePath;
        }
    }

    public class LocalizedCultureNotFoundEventArgs : LocalizationEventArgs
    {
        public string CultureName { get; }

        public LocalizedCultureNotFoundEventArgs(
            LocalizationFactory factory,
            string cultureName)
            : base(factory)
        {
            CultureName = cultureName;
        }
    }
}
