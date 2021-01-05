// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RIS.Settings.Ini
{
    public abstract class IniSettings : SettingsBase
    {
        [ExcludedSetting]
        public string SettingsFilePath { get; }
        [ExcludedSetting]
        public IniFile SettingsFile { get; }

        protected IniSettings(string path,
            string defaultSectionName = "General", char commentCharacter = ';',
            StringComparer comparer = null, IniBoolOptions boolOptions = null)
            : this(path, null, defaultSectionName, commentCharacter, comparer, boolOptions)
        {

        }
        protected IniSettings(string path, Encoding encoding,
            string defaultSectionName = "General", char commentCharacter = ';',
            StringComparer comparer = null, IniBoolOptions boolOptions = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                var exception =
                    new ArgumentException($"{nameof(path)} cannot be null, empty or consist only of whitespaces", nameof(path));
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            SettingsFilePath = path;
            SettingsFile = new IniFile(path, encoding,
                defaultSectionName, commentCharacter,
                comparer, boolOptions);
        }

        protected override void OnLoadSettings(IEnumerable<Setting> settings,
            SettingsLoadOptions options = SettingsLoadOptions.None)
        {
            SettingsFile.Load(SettingsFilePath);

            Setting[] settingsArray = settings as Setting[] ?? settings.ToArray();

            foreach (Setting setting in settingsArray)
            {
                string sectionName = string.IsNullOrWhiteSpace(setting.CategoryName)
                    ? SettingsFile.DefaultSectionName
                    : setting.CategoryName;
                string value = SettingsFile.GetString(sectionName, setting.Name);

                if (value != null)
                    setting.SetValueFromString(value);
            }

            if (options.HasFlag(SettingsLoadOptions.RemoveUnused))
            {
                foreach (var sectionName in SettingsFile.GetSections())
                {
                    bool settingExist = false;

                    foreach (var iniSetting in SettingsFile.GetSectionSettings(sectionName))
                    {
                        foreach (Setting setting in settingsArray)
                        {
                            if (setting.Name != iniSetting.Name)
                                continue;

                            if (setting.CategoryName != sectionName)
                            {
                                if (options.HasFlag(SettingsLoadOptions.DeduplicatePreserveValues))
                                    setting.SetValueFromString(iniSetting.Value);

                                break;
                            }

                            settingExist = true;

                            break;
                        }

                        if (!settingExist)
                            SettingsFile.Remove(sectionName, iniSetting?.Name);
                    }
                }
            }
            else if (options.HasFlag(SettingsLoadOptions.DeduplicatePreserveValues))
            {
                foreach (var sectionName in SettingsFile.GetSections())
                {
                    IniSection section = SettingsFile.GetSection(sectionName);

                    foreach (Setting setting in settingsArray)
                    {
                        if (!section.Settings.TryGetValue(setting.Name, out IniSetting iniSetting)
                            || setting.CategoryName == sectionName)
                        {
                            continue;
                        }

                        setting.SetValueFromString(iniSetting.Value);
                        SettingsFile.Remove(sectionName, iniSetting?.Name);
                    }
                }
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
