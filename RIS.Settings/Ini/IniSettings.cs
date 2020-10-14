// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;

namespace RIS.Settings.Ini
{
    public abstract class IniSettings : SettingsBase
    {
        [ExcludedSetting]
        public string SettingsFilePath { get; }
        [ExcludedSetting]
        public IniFile SettingsFile { get; }

        protected IniSettings(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                var exception =
                    new ArgumentException($"{nameof(path)} cannot be null, empty or consist only of whitespaces", nameof(path));
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            SettingsFilePath = path;
            SettingsFile = new IniFile();
        }

        protected override void OnLoadSettings(IEnumerable<Setting> settings)
        {
            SettingsFile.Load(SettingsFilePath);

            foreach (Setting setting in settings)
            {
                string sectionName = string.IsNullOrWhiteSpace(setting.CategoryName)
                    ? SettingsFile.DefaultSectionName
                    : setting.CategoryName;
                string value = SettingsFile.GetString(sectionName, setting.Name);

                if (value != null)
                    setting.SetValueFromString(value);
            }
        }

        protected override void OnSaveSettings(IEnumerable<Setting> settings)
        {
            foreach (Setting setting in settings)
            {
                string sectionName = string.IsNullOrWhiteSpace(setting.CategoryName)
                    ? SettingsFile.DefaultSectionName
                    : setting.CategoryName;
                string value = setting.GetValueToString();

                if (value != null)
                    SettingsFile.Set(sectionName, setting.Name, value);
            }

            SettingsFile.Save();
        }
    }
}
