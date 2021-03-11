// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RIS.Extensions
{
    public static class DirectoryExtensions
    {
        public static List<string> GetAllFiles(string directoryPath,
            string[] blacklistPaths = null, int nestingLevel = -1)
        {
            return EnumerateAllFiles(directoryPath, blacklistPaths, nestingLevel).ToList();
        }

        public static IEnumerable<string> EnumerateAllFiles(string directoryPath,
            string[] blacklistPaths = null, int nestingLevel = -1)
        {
            directoryPath = directoryPath
                .TrimEnd(Path.DirectorySeparatorChar)
                .TrimEnd(Path.AltDirectorySeparatorChar);

            directoryPath = !Path.IsPathRooted(directoryPath)
                ? Path.GetFullPath(directoryPath)
                : directoryPath;

            if (!Directory.Exists(directoryPath))
            {
                var exception = new FileNotFoundException($"Directory at path '{directoryPath}' not found");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (blacklistPaths == null)
                blacklistPaths = Array.Empty<string>();

            for (int i = 0; i < blacklistPaths.Length; ++i)
            {
                ref string blacklistPath = ref blacklistPaths[i];

                if (string.IsNullOrWhiteSpace(blacklistPath))
                    continue;

                blacklistPath = Path.Combine(directoryPath, blacklistPath);
            }

            return EnumerateAllFilesInternal(directoryPath, blacklistPaths, nestingLevel);
        }
        private static IEnumerable<string> EnumerateAllFilesInternal(string directoryPath,
            string[] blacklistPaths, int nestingLevel = -1)
        {
            if (!Directory.Exists(directoryPath))
                return Array.Empty<string>();

            foreach (var blacklistPath in blacklistPaths)
            {
                if (directoryPath == blacklistPath)
                    return Array.Empty<string>();
            }

            List<string> list = new List<string>(50);

            if (nestingLevel == -1 || nestingLevel != 0)
            {
                foreach (var directory in Directory.EnumerateDirectories(directoryPath))
                {
                    list.AddRange(EnumerateAllFilesInternal(directory, blacklistPaths, nestingLevel - 1));
                }
            }

            foreach (var file in Directory.EnumerateFiles(directoryPath))
            {
                bool isBlacklistPath = false;

                foreach (var blacklistPath in blacklistPaths)
                {
                    if (file != blacklistPath)
                        continue;

                    isBlacklistPath = true;
                    break;
                }

                if (isBlacklistPath)
                    continue;

                list.Add(file);
            }

            return list;
        }
    }
}
