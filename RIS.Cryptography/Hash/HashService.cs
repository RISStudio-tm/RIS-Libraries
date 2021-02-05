// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.IO;
using RIS.Extensions;

namespace RIS.Cryptography.Hash
{
    public class HashService
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        public IHashMethod HashMethod { get; private set; }

        public HashService(IHashMethod hashMethod)
        {
            if (!SetHashMethod(hashMethod))
            {
                var exception = new Exception("SetHashMethod return false. HashService is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }
        }

        public void OnInformation(RInformationEventArgs e)
        {
            OnInformation(this, e);
        }
        public void OnInformation(object sender, RInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        public void OnWarning(RWarningEventArgs e)
        {
            OnWarning(this, e);
        }
        public void OnWarning(object sender, RWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        public void OnError(RErrorEventArgs e)
        {
            OnError(this, e);
        }
        public void OnError(object sender, RErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }

        public bool SetHashMethod(IHashMethod hashMethod)
        {
            if (!hashMethod.Initialized)
                return false;

            HashMethod = hashMethod;
            return true;
        }

        public string GetHash(string plainText)
        {
            return HashMethod.GetHash(plainText);
        }
        public string GetHash(string plainText, ushort saltLength, out string hashSalt)
        {
            hashSalt = HashMethodsUtils.GenerateSalt(saltLength);

            return HashMethod.GetHash(hashSalt + plainText + hashSalt);
        }

        public string GetFileHash(string filePath, bool includeFileName = true)
        {
            return HashMethod.GetHash(GetFileHashInternal(filePath, includeFileName));
        }
        private string GetFileHashInternal(string filePath, bool includeFileName = true)
        {
            filePath = !Path.IsPathRooted(filePath)
                ? Path.GetFullPath(filePath)
                : filePath;

            if (!File.Exists(filePath))
            {
                var exception = new FileNotFoundException($"File at path '{filePath}' not found");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            FileStream file;

            try
            {
                file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }

            string result = HashMethod.GetHash("0");
            byte[] buffer = new byte[4096];
            int countReadBytes;

            do
            {
                countReadBytes = file.Read(buffer, 0, 4096);

                if (countReadBytes > 0)
                {
                    result = HashMethod.GetHash(
                        HashMethod.GetHash(buffer) +
                        result);
                }
            }
            while (countReadBytes > 0);

            if (includeFileName)
            {
                result = HashMethod.GetHash(
                    HashMethod.GetHash(Path.GetFileName(file.Name)) +
                    result);
            }

            file.Close();

            return result;
        }

        public string GetDirectoryHash(string directoryPath,
            bool includeDirectoryName = true, string[] blacklistPaths = null)
        {
            return HashMethod.GetHash(GetDirectoryHashInternal(directoryPath, includeDirectoryName, blacklistPaths));
        }
        private string GetDirectoryHashInternal(string directoryPath,
            bool includeDirectoryName = true, string[] blacklistPaths = null)
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
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            if (directoryPath.EndsWith(":", StringComparison.Ordinal))
                includeDirectoryName = false;

            int directorySeparatorIndex = directoryPath
                .LastIndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            string directoryName = directoryPath.Substring(directorySeparatorIndex + 1);
            string result = HashMethod.GetHash("0");

            foreach (string file in DirectoryExtensions.EnumerateAllFiles(directoryPath, blacklistPaths))
            {
                string fileDirectoryPath = Path.GetDirectoryName(file);

                if (!string.IsNullOrEmpty(fileDirectoryPath))
                {
                    int fileDirectorySeparatorIndex = fileDirectoryPath
                        .LastIndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
                    string fileDirectoryName = fileDirectoryPath.Substring(fileDirectorySeparatorIndex + 1);

                    if (fileDirectoryName != directoryName)
                    {
                        result = HashMethod.GetHash(
                            HashMethod.GetHash(fileDirectoryName) +
                            result);
                    }
                }

                result = HashMethod.GetHash(
                    GetFileHash(file) +
                    result);
            }

            if (includeDirectoryName)
            {
                result = HashMethod.GetHash(
                    HashMethod.GetHash(directoryName) +
                    result);
            }

            return result;
        }

        public bool VerifyHash(string plainText, string hashText)
        {
            return HashMethod.VerifyHash(plainText, hashText);
        }
        public bool VerifyHash(string plainText, string hashText, string hashSalt)
        {
            return HashMethod.VerifyHash(hashSalt + plainText + hashSalt, hashText);
        }

        public bool VerifyFileHash(string filePath, string hashText,
            bool includeFileName = true)
        {
            return HashMethod.VerifyHash(GetFileHashInternal(filePath, includeFileName), hashText);
        }

        public bool VerifyDirectoryHash(string directoryPath, string hashText,
            bool includeDirectoryName = true, string[] blacklistPaths = null)
        {
            return HashMethod.VerifyHash(GetDirectoryHashInternal(directoryPath, includeDirectoryName, blacklistPaths), hashText);
        }
    }
}
