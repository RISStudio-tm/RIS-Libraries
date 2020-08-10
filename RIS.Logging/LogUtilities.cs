// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Globalization;
using System.IO;

namespace RIS.Logging
{
    public static class LogUtilities
    {
        public static event EventHandler<RInformationEventArgs> Information;
		public static event EventHandler<RWarningEventArgs> Warning;
		public static event EventHandler<RErrorEventArgs> Error;

        public static void OnInformation(RInformationEventArgs e)
        {
            OnInformation(null, e);
        }
        public static void OnInformation(object sender, RInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        public static void OnWarning(RWarningEventArgs e)
        {
            OnWarning(null, e);
        }
        public static void OnWarning(object sender, RWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        public static void OnError(RErrorEventArgs e)
        {
            OnError(null, e);
        }
        public static void OnError(object sender, RErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }

        public static void GetTextFromSituation(LogSituation situation, out string situationText)
        {
            switch (situation)
            {
                case LogSituation.Information:
                    situationText = "Information";
                    break;
                case LogSituation.Warning:
                    situationText = "Warning";
                    break;
                case LogSituation.Error:
                    situationText = "Error";
                    break;
                case LogSituation.CriticalError:
                    situationText = "Critical Error";
                    break;
                case LogSituation.Unknown:
                    situationText = "Unknown";
                    break;
                case LogSituation.ApplicationAction:
                    situationText = "Application Action";
                    break;
                case LogSituation.UserAction:
                    situationText = "User Action";
                    break;
                case LogSituation.LogAction:
                    situationText = "Log Action";
                    break;
                default:
                    situationText = "Unknown";
                    break;
            }
        }
        public static void GetTextFromSituation(LogSituation situation, out string situationText, out string sortText)
        {
            switch (situation)
            {
                case LogSituation.Information:
                    situationText = "Information";
                    sortText = "INF";
                    break;
                case LogSituation.Warning:
                    situationText = "Warning";
                    sortText = "WRN";
                    break;
                case LogSituation.Error:
                    situationText = "Error";
                    sortText = "ERR";
                    break;
                case LogSituation.CriticalError:
                    situationText = "Critical Error";
                    sortText = "CER";
                    break;
                case LogSituation.Unknown:
                    situationText = "Unknown";
                    sortText = "UNW";
                    break;
                case LogSituation.ApplicationAction:
                    situationText = "Application Action";
                    sortText = "APA";
                    break;
                case LogSituation.UserAction:
                    situationText = "User Action";
                    sortText = "USA";
                    break;
                case LogSituation.LogAction:
                    situationText = "Log Action";
                    sortText = "LGA";
                    break;
                default:
                    situationText = "Unknown";
                    sortText = "UNW";
                    break;
            }
        }

        public static LogSituation GetSituationFromText(string situationText)
        {
            switch (situationText)
            {
                case "Information":
                    return LogSituation.Information;
                case "Warning":
                    return LogSituation.Warning;
                case "Error":
                    return LogSituation.Error;
                case "Critical Error":
                    return LogSituation.CriticalError;
                case "Unknown":
                    return LogSituation.Unknown;
                case "Application Action":
                    return LogSituation.ApplicationAction;
                case "User Action":
                    return LogSituation.UserAction;
                case "Log Action":
                    return LogSituation.LogAction;
                default:
                    return LogSituation.Unknown;
            }
        }
        
        public static LogSituation GetSituationFromSortText(string sortText)
        {
            switch (sortText)
            {
                case "INF":
                    return LogSituation.Information;
                case "WRN":
                    return LogSituation.Warning;
                case "ERR":
                    return LogSituation.Error;
                case "CER":
                    return LogSituation.CriticalError;
                case "UNW":
                    return LogSituation.Unknown;
                case "APA":
                    return LogSituation.ApplicationAction;
                case "USA":
                    return LogSituation.UserAction;
                case "LGA":
                    return LogSituation.LogAction;
                default:
                    return LogSituation.Unknown;
            }
        }

        public static int DeleteLogs(string logsPath, ushort daysToRemove)
        {
            if (!Directory.Exists(logsPath))
            {
                var exception = new DirectoryNotFoundException("Невозможно начать удаление лог-файлов, так как указанный каталог не существует");
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            try
            {
                int countDeletedFiles = 0;

                if (!Path.IsPathRooted(logsPath))
                    logsPath = Path.GetFullPath(logsPath);

                string filesDirectory = Path.Combine(logsPath, "Logs");

                if (!Directory.Exists(filesDirectory))
                {
                    Directory.CreateDirectory(filesDirectory);
                }

                string[] logFilesList = Directory.GetFiles(filesDirectory, "*.rlog");

                DateTime dateNow = DateTime.UtcNow;

                for (int i = 0; i < logFilesList.Length; ++i)
                {
                    DateTime dateLogFile = DateTime.ParseExact(
                        Path.GetFileNameWithoutExtension(logFilesList[i])?.Substring(0, 19),
                        "dd-MM-yyyy_HH-mm-ss", CultureInfo.CurrentCulture).ToUniversalTime();

                    if (dateLogFile.AddDays(daysToRemove) <= dateNow)
                    {
                        try
                        {
                            File.Delete(logFilesList[i]);
                            ++countDeletedFiles;
                        }
                        catch (Exception ex)
                        {
                            Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                            OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                        }
                    }
                }

                return countDeletedFiles;
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
        
        public static int DeleteDebugLogs(string logsPath, ushort daysToRemove)
        {
            if (!Directory.Exists(logsPath))
            {
                var exception = new DirectoryNotFoundException("Невозможно начать удаление лог-файлов, так как указанный каталог не существует");
                Events.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            try
            {
                int countDeletedFiles = 0;

                if (!Path.IsPathRooted(logsPath))
                    logsPath = Path.GetFullPath(logsPath);

                string filesDirectory = Path.Combine(logsPath, "Logs", "Debug");

                if (!Directory.Exists(filesDirectory))
                {
                    Directory.CreateDirectory(filesDirectory);
                }

                string[] logFilesList = Directory.GetFiles(filesDirectory, "*.rdlog");

                DateTime dateNow = DateTime.UtcNow;

                for (int i = 0; i < logFilesList.Length; ++i)
                {
                    DateTime dateLogFile = DateTime.ParseExact(
                        Path.GetFileNameWithoutExtension(logFilesList[i])?.Substring(0, 19),
                        "dd-MM-yyyy_HH-mm-ss", CultureInfo.CurrentCulture).ToUniversalTime();

                    if (dateLogFile.AddDays(daysToRemove) <= dateNow)
                    {
                        try
                        {
                            File.Delete(logFilesList[i]);
                            ++countDeletedFiles;
                        }
                        catch (Exception ex)
                        {
                            Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                            OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                        }
                    }
                }

                return countDeletedFiles;
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
    }
}
