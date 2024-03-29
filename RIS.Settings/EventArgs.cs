﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Settings
{
    public class AppVersionChangedEventArgs : EventArgs
    {
        public string OldAppVersion { get; }
        public string NewAppVersion { get; }

        public AppVersionChangedEventArgs(
            string oldAppVersion, string newAppVersion)
        {
            OldAppVersion = oldAppVersion;
            NewAppVersion = newAppVersion;
        }
    }
}
