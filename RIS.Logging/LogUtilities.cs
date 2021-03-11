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

        public static string ReplaceEOLChars(string text, string replaceChar)
        {
            return text
                .Replace("\u000D\u000A", replaceChar)
                .Replace("\u000A", replaceChar)
                .Replace("\u0085", replaceChar)
                .Replace("\u2028", replaceChar)
                .Replace("\u2029", replaceChar);
        }

        public static (string SituationText, string SortText) GetTextFromSituation(LogSituation situation)
        {
            switch (situation)
            {
                case LogSituation.Information:
                    return ("Information", "INF");
                case LogSituation.Warning:
                    return ("Warning", "WRN");
                case LogSituation.Error:
                    return ("Error", "ERR");
                case LogSituation.CriticalError:
                    return ("Critical Error", "CER");
                case LogSituation.ApplicationAction:
                    return ("Application Action", "APA");
                case LogSituation.UserAction:
                    return ("User Action", "USA");
                case LogSituation.LogAction:
                    return ("Log Action", "LGA");
                case LogSituation.Unknown:
                default:
                    return ("Unknown", "UNW");
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
                case "Application Action":
                    return LogSituation.ApplicationAction;
                case "User Action":
                    return LogSituation.UserAction;
                case "Log Action":
                    return LogSituation.LogAction;
                case "Unknown":
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
                case "APA":
                    return LogSituation.ApplicationAction;
                case "USA":
                    return LogSituation.UserAction;
                case "LGA":
                    return LogSituation.LogAction;
                case "UNW":
                default:
                    return LogSituation.Unknown;
            }
        }

        public static int DeleteLogs(string logsPath, ushort daysToRemove)
        {
            return DeleteLogsInternal(logsPath, daysToRemove, ".rlog");
        }
        public static int DeleteDebugLogs(string logsPath, ushort daysToRemove)
        {
            return DeleteLogsInternal(logsPath, daysToRemove, ".rdlog");
        }

        private static int DeleteLogsInternal(string logsPath, ushort daysToRemove, string fileExtension)
        {
            if (!Directory.Exists(logsPath))
            {
                var exception = new DirectoryNotFoundException("Невозможно начать удаление лог-файлов, так как указанный каталог не существует");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            try
            {
                int countDeletedFiles = 0;

                if (!Path.IsPathRooted(logsPath))
                    logsPath = Path.GetFullPath(logsPath);

                string filesDirectory = Path.Combine(logsPath, "Logs");

                if (fileExtension == ".rdlog")
                    filesDirectory = Path.Combine(filesDirectory, "Debug");

                if (!Directory.Exists(filesDirectory))
                    Directory.CreateDirectory(filesDirectory);

                string[] logFiles = Directory.GetFiles(filesDirectory, fileExtension);

                DateTime dateNow = DateTime.UtcNow;

                for (int i = 0; i < logFiles.Length; ++i)
                {
                    ref string logFile = ref logFiles[i];

                    if (logFile == null)
                        continue;

                    DateTime dateLogFile = DateTime.ParseExact(
                        Path.GetFileNameWithoutExtension(logFile)?.Substring(0, 19),
                        "dd-MM-yyyy_HH-mm-ss", CultureInfo.CurrentCulture).ToUniversalTime();

                    if (dateLogFile.AddDays(daysToRemove) <= dateNow)
                    {
                        try
                        {
                            File.Delete(logFile);
                            ++countDeletedFiles;
                        }
                        catch (Exception ex)
                        {
                            Events.OnError(new RErrorEventArgs(ex, ex.Message));
                            OnError(new RErrorEventArgs(ex, ex.Message));
                        }
                    }
                }

                return countDeletedFiles;
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }
    }
}
