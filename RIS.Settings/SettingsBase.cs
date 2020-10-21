// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Reflection;

namespace RIS.Settings
{
    public abstract class SettingsBase
    {
        private readonly IEnumerable<Setting> _settingsList;

        protected SettingsBase()
        {
            _settingsList = BuildSettingsList();
        }

        private IEnumerable<Setting> BuildSettingsList()
        {
            foreach (PropertyInfo property in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (Attribute.IsDefined(property, typeof(ExcludedSettingAttribute)))
                    continue;

                string category = null;

                if (Attribute.IsDefined(property, typeof(SettingCategoryAttribute)))
                    category = ((SettingCategoryAttribute)property.GetCustomAttribute(typeof(SettingCategoryAttribute)))?.Name;

                yield return new Setting(this, property, category);
            }
        }

        protected abstract void OnLoadSettings(IEnumerable<Setting> settings, SettingsLoadOptions options = SettingsLoadOptions.None);

        protected abstract void OnSaveSettings(IEnumerable<Setting> settings);

        public void Load(SettingsLoadOptions options = SettingsLoadOptions.None)
        {
            OnLoadSettings(_settingsList, options);
            OnSaveSettings(_settingsList);
        }

        public void Save()
        {
            OnSaveSettings(_settingsList);
        }
    }
}
