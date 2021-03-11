// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace RIS.Settings
{
    public abstract class SettingsBase
    {
        private readonly IEnumerable<Setting> _settingsList;

        [ExcludedSetting]
        public object SyncRoot { get; }

        [SettingCategory("Version")]
        public string AppVersion { get; set; }

        protected SettingsBase()
        {
            _settingsList = BuildSettingsList();

            SyncRoot = new object();

            AppVersion = "0.0.0";
        }

        private IEnumerable<Setting> BuildSettingsList()
        {
            string category = null;

            foreach (PropertyInfo property in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (Attribute.IsDefined(property, typeof(SettingCategoryAttribute)))
                    category = ((SettingCategoryAttribute)property.GetCustomAttribute(typeof(SettingCategoryAttribute)))?.Name;

                if (Attribute.IsDefined(property, typeof(ExcludedSettingAttribute)))
                    continue;

                yield return new Setting(this, property, category);
            }
        }

        protected abstract void OnLoadSettings(IEnumerable<Setting> settings,
            SettingsLoadOptions options = SettingsLoadOptions.None);

        protected abstract void OnSaveSettings(IEnumerable<Setting> settings);

        public void Load(bool appVersionCheck = true,
            SettingsLoadOptions options = SettingsLoadOptions.None)
        {
            lock (SyncRoot)
            {
                OnLoadSettings(_settingsList, options);
                OnSaveSettings(_settingsList);

                if (appVersionCheck)
                {
                    string appFilePath;

#if NETCOREAPP

                    appFilePath = Environment.ExecAppAssemblyFilePath;

#elif NETFRAMEWORK

                    appFilePath = Environment.ExecAppFilePath;

#endif

                    var currentAppVersion = FileVersionInfo
                        .GetVersionInfo(appFilePath)
                        .ProductVersion;

                    if (AppVersion != currentAppVersion)
                    {
                        OnLoadSettings(_settingsList,
                            SettingsLoadOptions.RemoveUnused
                            | SettingsLoadOptions.DeduplicatePreserveValues);

                        AppVersion = currentAppVersion;

                        OnSaveSettings(_settingsList);
                    }
                }
            }
        }

        public void Save()
        {
            Task.Run(() =>
            {
                lock (SyncRoot)
                {
                    OnSaveSettings(_settingsList);
                }
            });
        }
    }
}
