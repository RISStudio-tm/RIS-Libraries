// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using RIS.Extensions;

namespace RIS.Settings
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
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
        [DefaultSettingValue("0.0.0")]
        public string AppVersion { get; private set; }

        protected SettingsBase()
        {
            _settingsList = BuildSettingsList();

            SyncRoot = new object();
        }

        private IEnumerable<Setting> BuildSettingsList()
        {
            var settings = new List<Setting>(10);

            string category = null;

            foreach (var property in GetType().GetProperties(BindingFlags.Instance
                                                             | BindingFlags.Public
                                                             | BindingFlags.GetProperty
                                                             | BindingFlags.SetProperty))
            {
                var declaringProperty = property
                    .GetDeclaring();

                if (Attribute.IsDefined(property, typeof(SettingCategoryAttribute)))
                    category = ((SettingCategoryAttribute)property.GetCustomAttribute(typeof(SettingCategoryAttribute)))?.Name;
                else if (Attribute.IsDefined(declaringProperty, typeof(SettingCategoryAttribute)))
                    category = ((SettingCategoryAttribute)declaringProperty.GetCustomAttribute(typeof(SettingCategoryAttribute)))?.Name;

                if (Attribute.IsDefined(property, typeof(ExcludedSettingAttribute)))
                    continue;
                else if (Attribute.IsDefined(declaringProperty, typeof(ExcludedSettingAttribute)))
                    continue;

                var setting = new Setting(this,
                    declaringProperty, category);

                if (Attribute.IsDefined(property, typeof(DefaultSettingValueAttribute)))
                {
                    setting.DefaultValue = ((DefaultSettingValueAttribute)property.GetCustomAttribute(typeof(DefaultSettingValueAttribute)))?.DefaultValue;
                }
                else if (Attribute.IsDefined(declaringProperty, typeof(DefaultSettingValueAttribute)))
                {
                    setting.DefaultValue = ((DefaultSettingValueAttribute)declaringProperty.GetCustomAttribute(typeof(DefaultSettingValueAttribute)))?.DefaultValue;
                }
                else if (setting.Type.IsEnum)
                {
                    setting.DefaultValue = setting.Type
                        .GetEnumDefaultValue();
                }
                else
                {
                    setting.DefaultValue = setting.Type.IsValueType
                        ? Activator.CreateInstance(setting.Type)
                        : null;
                }

                if (setting.Type.IsPrimitive
                    || setting.Type.IsArray
                    || setting.Type.IsEnum
                    || setting.Type == typeof(decimal)
                    || setting.Type == typeof(DateTime))
                {
                    setting.SetValue(
                        setting.DefaultValue);
                }
                else
                {
                    setting.SetValueFromString(
                        setting.DefaultValue?.ToString());
                }

                settings.Add(setting);
            }

            return settings;
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
                    var appFilePath = Environment.ExecAppAssemblyFilePath;

                    if (string.IsNullOrEmpty(appFilePath) || appFilePath == "Unknown")
                        appFilePath = Environment.ExecAppFilePath;
                    if (string.IsNullOrEmpty(appFilePath) || appFilePath == "Unknown" )
                        appFilePath = Environment.ExecProcessFilePath;

                    if (File.Exists(appFilePath))
                    {
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
                    else
                    {
                        var exception =
                            new FileNotFoundException($"File[Path={appFilePath}] not found", appFilePath);
                        Events.OnError(this,
                            new RErrorEventArgs(exception, exception.Message));
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
