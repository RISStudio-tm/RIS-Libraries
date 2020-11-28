// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.IO;
using System.Text;

namespace RIS.Logging
{
    public sealed class DebugLog
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        private StreamWriter LogFile { get; set; }

        public bool ActivatedLog { get; private set; }
        public string FileDirectory { get; private set; }
        public string FileFullPath { get; private set; }
        public string FileName { get; private set; }
        public string FileExtension { get; private set; }
        public string FileFullName { get; private set; }
        public Encoding FileEncoding { get; private set; }

        public DebugLog(string directoryPath)
            : this(directoryPath, Encoding.UTF8)
        {

        }
        public DebugLog(string directoryPath, Encoding encoding)
        {
            ActivatedLog = false;
            LogFile = null;
            FileEncoding = encoding;

            Create(directoryPath, encoding);
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

        private void Create(string directoryPath, Encoding encoding)
        {
            if (!Directory.Exists(directoryPath))
            {
                var exception = new DirectoryNotFoundException("Невозможно создать лог-файл, так как указанный каталог не существует");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            try
            {
                if (!Path.IsPathRooted(directoryPath))
                    directoryPath = Path.GetFullPath(directoryPath);

                string fileDirectory = Path.Combine(directoryPath, "Logs", "Debug");

                if (!Directory.Exists(fileDirectory))
                    Directory.CreateDirectory(fileDirectory);

                DateTime now = DateTime.Now;

                string day = now.Day.ToString().PadLeft(2, '0');
                string month = now.Month.ToString().PadLeft(2, '0');
                string year = now.Year.ToString().PadLeft(4, '0');
                string hours = now.Hour.ToString().PadLeft(2, '0');
                string minutes = now.Minute.ToString().PadLeft(2, '0');
                string seconds = now.Second.ToString().PadLeft(2, '0');

                FileName = day + "-" + month + "-" + year + "_" + hours + "-" + minutes + "-" + seconds;
                FileExtension = ".rdlog";
                FileFullName = FileName + FileExtension;
                FileDirectory = fileDirectory;
                FileFullPath = Path.Combine(FileDirectory, FileFullName);

                LogFile = new StreamWriter(FileFullPath, true, encoding);
                ActivatedLog = true;

                WriteLine("Start", LogSituation.LogAction);
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
        }

        public void Pause()
        {
            ActivatedLog = false;

            WriteLine("Pause", LogSituation.LogAction);
        }

        public void Resume()
        {
            ActivatedLog = true;

            WriteLine("Resume", LogSituation.LogAction);
        }

        public void Close()
        {
            try
            {
                WriteLine("Stop", LogSituation.LogAction);

                ActivatedLog = false;
                LogFile.Close();

                LogFile = null;
                FileEncoding = null;

                FileDirectory = string.Empty;
                FileFullPath = string.Empty;
                FileName = string.Empty;
                FileExtension = string.Empty;
                FileFullName = string.Empty;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
        }

        public void WriteLine(string text, LogSituation situation = LogSituation.Unknown)
        {
            try
            {
                if (!ActivatedLog)
                    return;

                (string situationText, string sortText) = LogUtilities.GetTextFromSituation(situation);

                string logTimeUtc = DateTime.UtcNow.ToLongTimeString();
                string logTime = DateTime.Now.ToLongTimeString();
                string logLine = sortText + "|" + logTimeUtc + "|" + logTime + "|" + situationText + ":  " + LogUtilities.ReplaceEOLChars(text, " > ");

                LogFile.WriteLine(logLine);
                LogFile.Flush();
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
        }

        public void WriteDividingLine(ushort charsCount = 111, char lineChar = '_')
        {
            if (lineChar == '\0')
                lineChar = '_';

            string text = string.Empty.PadRight(charsCount, lineChar);

            WriteLine(text, LogSituation.LogAction);
        }
    }
}
