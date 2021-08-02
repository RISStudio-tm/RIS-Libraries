// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using RIS.Graphics.WPF.Localization;
using RIS.Graphics.WPF.Localization.Entities;

namespace RIS.Graphics.WPF.Controls
{
    public partial class MetroLocalizationButton : SplitButton
    {
        public MetroLocalizationButton()
        {
            InitializeComponent();

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

            if (e.Localizations.TryGetValue(LocalizationManager.CurrentLocalization.CultureName, out var localizationModule))
            {
                SelectedItem = new KeyValuePair<string, LocalizationXamlModule>(
                    LocalizationManager.CurrentLocalization.CultureName, localizationModule);
            }
            else if (e.Localizations.TryGetValue(LocalizationManager.DefaultCulture.Name, out localizationModule))
            {
                SelectedItem = new KeyValuePair<string, LocalizationXamlModule>(
                    LocalizationManager.DefaultCulture.Name, localizationModule);
            }
            else
            {
                SelectedItem = e.Localizations.FirstOrDefault();
            }

            SelectionChanged += Button_SelectionChanged;

            string cultureName = null;

            if (SelectedItem != null)
                cultureName = ((KeyValuePair<string, LocalizationXamlModule>)SelectedItem).Key;

            LocalizationManager.SwitchLocalization(
                cultureName);
        }

        private void LocalizationManager_LocalizationChanged(object sender, LocalizationChangedEventArgs e)
        {
            using var @lock = LocalizationManager.SyncRoot.Lock();

            SelectionChanged -= Button_SelectionChanged;

            SelectedItem = new KeyValuePair<string, LocalizationXamlModule>(
                e.NewLocalization.CultureName, e.NewLocalization);

            SelectionChanged += Button_SelectionChanged;
        }



        private void Button_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using var @lock = LocalizationManager.SyncRoot.Lock();

            if (SelectedItem == null)
                return;

            var selectedPair =
                (KeyValuePair<string, LocalizationXamlModule>)SelectedItem;

            LocalizationManager.SwitchLocalization(
                selectedPair.Key);
        }
    }
}
