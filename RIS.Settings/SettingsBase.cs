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
            foreach (PropertyInfo prop in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!Attribute.IsDefined(prop, typeof(ExcludedSettingAttribute)))
                    yield return new Setting(this, prop);
            }
        }

        protected abstract void OnLoadSettings(IEnumerable<Setting> settings);

        protected abstract void OnSaveSettings(IEnumerable<Setting> settings);

        public void Load()
        {
            OnLoadSettings(_settingsList);
            OnSaveSettings(_settingsList);
        }

        public void Save()
        {
            OnSaveSettings(_settingsList);
        }
    }
}
