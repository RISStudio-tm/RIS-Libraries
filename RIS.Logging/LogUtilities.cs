// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.IO;

namespace RIS.Logging
{
    public static class LogUtilities
    {
        public static string GetLogDirectoryPath(
            LogType log)
        {
            var basePath = Environment.ExecProcessDirectoryName;
            string relativePath;

            switch (log)
            {
                case LogType.Debug:
                    relativePath = @"logs\debug";
                    break;
                case LogType.Default:
                default:
                    relativePath = @"logs";
                    break;
            }

            return Path.Combine(
                basePath, relativePath);
        }
    }
}
