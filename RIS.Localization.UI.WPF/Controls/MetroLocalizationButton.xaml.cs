// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace RIS.Localization.UI.WPF.Controls
{
    public partial class MetroLocalizationButton : SplitButton
    {
        public MetroLocalizationButton()
        {
            InitializeComponent();
            DataContext = this;

            LocalizationManager.LocalizationsLoaded += LocalizationManager_LocalizationsLoaded;
            LocalizationManager.LocalizationChanged += LocalizationManager_LocalizationChanged;
        }

        ~MetroLocalizationButton()
        {
            LocalizationManager.LocalizationsLoaded -= LocalizationManager_LocalizationsLoaded;
            LocalizationManager.LocalizationChanged -= LocalizationManager_LocalizationChanged;
        }



        private void LocalizationManager_LocalizationsLoaded(object sender, LocalizationLoadedEventArgs e)
        {
            using var @lock = LocalizationManager.SyncRoot.Lock();

            GetBindingExpression(ItemsSourceProperty)?
                .UpdateTarget();

            SelectionChanged -= Button_SelectionChanged;

            if (LocalizationManager.CurrentLocalization != null
                && e.Localizations.TryGetValue(LocalizationManager.CurrentLocalization.CultureName, out var localizationModule))
            {
                SelectedItem = new KeyValuePair<string, ILocalizationModule>(
                    LocalizationManager.CurrentLocalization.CultureName, localizationModule);
            }
            else if (e.Localizations.TryGetValue(LocalizationManager.DefaultCulture.Name, out localizationModule))
            {
                SelectedItem = new KeyValuePair<string, ILocalizationModule>(
                    LocalizationManager.DefaultCulture.Name, localizationModule);
            }
            else if (e.Localizations.TryGetValue("en-US", out localizationModule))
            {
                SelectedItem = new KeyValuePair<string, ILocalizationModule>(
                    "en-US", localizationModule);
            }
            else
            {
                SelectedItem = e.Localizations.FirstOrDefault();
            }

            SelectionChanged += Button_SelectionChanged;

            if (LocalizationManager.CurrentLocalization == null)
                return;

            string cultureName = null;

            if (SelectedItem != null)
                cultureName = ((KeyValuePair<string, ILocalizationModule>)SelectedItem).Key;

            LocalizationManager.SwitchLocalization(
                cultureName);
        }

        private void LocalizationManager_LocalizationChanged(object sender, LocalizationChangedEventArgs e)
        {
            using var @lock = LocalizationManager.SyncRoot.Lock();

            SelectionChanged -= Button_SelectionChanged;

            SelectedItem = new KeyValuePair<string, ILocalizationModule>(
                e.NewLocalization.CultureName, e.NewLocalization);

            SelectionChanged += Button_SelectionChanged;
        }



        private void Button_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using var @lock = LocalizationManager.SyncRoot.Lock();

            if (SelectedItem == null)
                return;

            var selectedPair =
                (KeyValuePair<string, ILocalizationModule>)SelectedItem;

            LocalizationManager.SwitchLocalization(
                selectedPair.Key);
        }
    }
}
