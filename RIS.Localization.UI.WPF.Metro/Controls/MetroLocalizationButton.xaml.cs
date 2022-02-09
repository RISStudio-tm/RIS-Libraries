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



        public void UpdateLocalizations()
        {
            var factory = LocalizationManager.CurrentUIFactory;

            if (factory == null)
                return;

            using var @lock = factory.SyncRoot.Lock();

            GetBindingExpression(ItemsSourceProperty)?
                .UpdateTarget();

            if (factory.Localizations.Count == 0)
            {
                SelectedItem = null;

                return;
            }

            SelectionChanged -= Button_SelectionChanged;

            if (factory.CurrentLocalization != null
                && factory.Localizations.TryGetValue(factory.CurrentLocalization.CultureName, out var localizationModule))
            {
                SelectedItem = new KeyValuePair<string, ILocalizationModule>(
                    factory.CurrentLocalization.CultureName, localizationModule);
            }
            else if (factory.Localizations.TryGetValue(factory.DefaultCulture.Name, out localizationModule))
            {
                SelectedItem = new KeyValuePair<string, ILocalizationModule>(
                    factory.DefaultCulture.Name, localizationModule);
            }
            else if (factory.Localizations.TryGetValue("en-US", out localizationModule))
            {
                SelectedItem = new KeyValuePair<string, ILocalizationModule>(
                    "en-US", localizationModule);
            }
            else
            {
                SelectedItem = factory.Localizations
                    .FirstOrDefault();
            }

            SelectionChanged += Button_SelectionChanged;

            if (factory.CurrentLocalization == null)
                return;

            string cultureName = null;

            if (SelectedItem != null)
                cultureName = ((KeyValuePair<string, ILocalizationModule>)SelectedItem).Key;

            factory.SwitchLocalization(
                cultureName);
        }



        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            UpdateLocalizations();
        }



        private void LocalizationManager_LocalizationsLoaded(object sender, LocalizationLoadedEventArgs e)
        {
            var factory = LocalizationManager.CurrentUIFactory;

            if (factory == null)
                return;
            if (e.Factory.AssemblyName != factory.AssemblyName)
                return;

            using var @lock = factory.SyncRoot.Lock();

            UpdateLocalizations();
        }

        private void LocalizationManager_LocalizationChanged(object sender, LocalizationChangedEventArgs e)
        {
            var factory = LocalizationManager.CurrentUIFactory;

            if (factory == null)
                return;
            if (e.Factory.AssemblyName != factory.AssemblyName)
                return;

            using var @lock = factory.SyncRoot.Lock();

            SelectionChanged -= Button_SelectionChanged;

            SelectedItem = new KeyValuePair<string, ILocalizationModule>(
                e.NewLocalization.CultureName, e.NewLocalization);

            SelectionChanged += Button_SelectionChanged;
        }



        private void Button_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var factory = LocalizationManager.CurrentUIFactory;

            if (factory == null)
                return;

            using var @lock = factory.SyncRoot.Lock();

            if (SelectedItem == null)
                return;

            var selectedPair =
                (KeyValuePair<string, ILocalizationModule>)SelectedItem;

            factory.SwitchLocalization(
                selectedPair.Key);
        }
    }
}
