﻿// Copyright (c) RISStudio, 2020. All rights reserved.
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

        public DebugLog(string path)
        {
            ActivatedLog = false;
            LogFile = null;
            FileEncoding = Encoding.UTF8;

            Create(path, Encoding.UTF8);
        }
        public DebugLog(string path, Encoding encoding)
        {
            ActivatedLog = false;
            LogFile = null;
            FileEncoding = encoding;

            Create(path, encoding);
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

        public void WriteLine(string text)
        {
            WriteLine(text, LogSituation.Unknown);
        }
        public void WriteLine(string text, LogSituation situation)
        {
            try
            {
                if (ActivatedLog)
                    return;

                LogUtilities.GetTextFromSituation(situation, out string situationText, out string sortText);

                string logTime = DateTime.Now.ToLongTimeString();
                string logLine = sortText + "|" + logTime + "|" + situationText + ":  " + text;

                LogFile.WriteLine(logLine);
                LogFile.Flush();
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }

        public void WriteDividingLine()
        {
            WriteDividingLine(111, '_');
        }
        public void WriteDividingLine(ushort charsCount)
        {
            WriteDividingLine(charsCount, '_');
        }
        public void WriteDividingLine(ushort charsCount, char lineChar)
        {
            if (lineChar == '\0')
                lineChar = '_';

            string text = string.Empty.PadRight(charsCount, lineChar);
            //for (ushort i = 1; i <= charsCount; ++i)
            //{
            //    text += lineChar;
            //}

            WriteLine(text, LogSituation.LogAction);
        }

        private void Create(string path, Encoding encoding)
        {
            if (!Directory.Exists(path))
            {
                var exception = new DirectoryNotFoundException("Невозможно создать лог-файл, так как указанный каталог не существует");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            try
            {
                if (!Path.IsPathRooted(path))
                    path = Path.GetFullPath(path);

                string fileDirectory = Path.Combine(path, "Logs", "Debug");

                if (!Directory.Exists(fileDirectory))
                {
                    Directory.CreateDirectory(fileDirectory);
                }

                string day = DateTime.Now.Day.ToString().PadLeft(2, '0');
                string month = DateTime.Now.Month.ToString().PadLeft(2, '0');
                string year = DateTime.Now.Year.ToString().PadLeft(4, '0');
                string hours = DateTime.Now.Hour.ToString().PadLeft(2, '0');
                string minutes = DateTime.Now.Minute.ToString().PadLeft(2, '0');
                string seconds = DateTime.Now.Second.ToString().PadLeft(2, '0');

                FileName = day + "-" + month + "-" + year + "_" + hours + "-" + minutes + "-" + seconds;
                FileExtension = ".rdlog";
                FileFullName = FileName + FileExtension;
                FileDirectory = fileDirectory;
                FileFullPath = Path.Combine(FileDirectory, FileFullName);

                LogFile = new StreamWriter(FileFullPath, true, encoding);
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }

        public void Pause()
        {
            ActivatedLog = false;
        }

        public void Resume()
        {
            ActivatedLog = true;
        }

        public void Close()
        {
            try
            {
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
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }
    }
}
