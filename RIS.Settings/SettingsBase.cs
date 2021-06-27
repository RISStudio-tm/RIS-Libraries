// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using RIS.Extensions;

namespace RIS.Settings
{
    public abstract class SettingsBase
    {
        public event EventHandler<EventArgs> Loading;
        public event EventHandler<EventArgs> Loaded;
        public event EventHandler<EventArgs> Saving;
        public event EventHandler<EventArgs> Saved;
        public event EventHandler<AppVersionChangedEventArgs> AppVersionChanged;

        private readonly IEnumerable<Setting> _settingsList;

        [ExcludedSetting]
        public object SyncRoot { get; }

        [SettingCategory("Version")]
        public string AppVersion { get; private set; }

        protected SettingsBase()
        {
            _settingsList = BuildSettingsList();

            SyncRoot = new object();

            AppVersion = "0.0.0";
        }

        private IEnumerable<Setting> BuildSettingsList()
        {
            string category = null;

            foreach (PropertyInfo property in GetType().GetProperties(BindingFlags.Instance
                                                                      | BindingFlags.Public
                                                                      | BindingFlags.GetProperty
                                                                      | BindingFlags.SetProperty))
            {
                if (Attribute.IsDefined(property, typeof(SettingCategoryAttribute)))
                    category = ((SettingCategoryAttribute)property.GetCustomAttribute(typeof(SettingCategoryAttribute)))?.Name;

                if (Attribute.IsDefined(property, typeof(ExcludedSettingAttribute)))
                    continue;
                
                var declaringProperty = property.GetDeclaring();

                yield return new Setting(this, declaringProperty, category);
            }
        }

        protected abstract void OnLoadSettings(IEnumerable<Setting> settings,
            SettingsLoadOptions options = SettingsLoadOptions.None);

        protected abstract void OnSaveSettings(IEnumerable<Setting> settings);

        public void Load(
            SettingsLoadOptions options = SettingsLoadOptions.None,
            bool appVersionCheck = true,
            SettingsLoadOptions appVersionCheckOptions = SettingsLoadOptions.RemoveUnused)
        {
            lock (SyncRoot)
            {
                Loading?.Invoke(this,
                    EventArgs.Empty);

                OnLoadSettings(_settingsList,
                    options);
                OnSaveSettings(_settingsList);

                if (appVersionCheck)
                {

#if NETCOREAPP

                    var appFilePath = Environment.ExecAppAssemblyFilePath;

#elif NETFRAMEWORK

                    var appFilePath = Environment.ExecAppFilePath;

#endif

                    var currentAppVersion = FileVersionInfo
                        .GetVersionInfo(appFilePath)
                        .ProductVersion;

                    if (AppVersion != currentAppVersion)
                    {
                        var oldAppVersion = AppVersion;

                        OnLoadSettings(_settingsList,
                            appVersionCheckOptions);

                        AppVersion = currentAppVersion;

                        OnSaveSettings(_settingsList);

                        AppVersionChanged?.Invoke(this,
                            new AppVersionChangedEventArgs(
                                oldAppVersion, currentAppVersion));
                    }
                }

                Loaded?.Invoke(this,
                    EventArgs.Empty);
            }
        }

        public void Save()
        {
            Task.Run(() =>
            {
                lock (SyncRoot)
                {
                    Saving?.Invoke(this,
                        EventArgs.Empty);

                    OnSaveSettings(_settingsList);

                    Saved?.Invoke(this,
                        EventArgs.Empty);
                }
            });
        }
    }
}
