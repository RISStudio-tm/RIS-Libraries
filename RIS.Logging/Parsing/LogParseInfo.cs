// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RIS.Collections.Chunked;

namespace RIS.Logging.Parsing
{
    public sealed class LogParseInfo
    {
        public event EventHandler<RInformationEventArgs> Information;
		public event EventHandler<RWarningEventArgs> Warning;
		public event EventHandler<RErrorEventArgs> Error;

        private StreamReader LogFile { get; set; }

        private string[] SituationsOriginalNames { get; set; }
        private string[] SituationsNames { get; set; }
        private LogSituation[] Situations { get; set; }
        private long[] SituationsMeetsCounts { get; set; }
        private ChunkedArrayD<long>[] SituationsMeetsLines { get; set; }
        private long LinesCount { get; set; }

        public string FileDirectory { get; private set; }
        public string FileFullPath { get; private set; }
        public string FileName { get; private set; }
        public string FileExtension { get; private set; }
        public string FileFullName { get; private set; }
        public Encoding FileEncoding { get; private set; }

        public LogParseInfo(string path)
            : this(path, Encoding.UTF8)
        {

        }
        public LogParseInfo(string path, Encoding encoding)
        {
            LogFile = null;
            FileEncoding = encoding;

            Task parse = Task.Factory.StartNew(() =>
            {
                OpenFile(path, encoding);
                ParseFile();
                CloseFile();
            });

            parse.Wait();
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

        private void OpenFile(string path, Encoding encoding)
        {
            if (!File.Exists(path))
            {
                var exception = new DirectoryNotFoundException("Невозможно открыть лог-файл, так как указанный каталог не существует");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (Path.GetExtension(path) != ".rlog" && Path.GetExtension(path) != ".rdlog")
            {
                var exception = new Exception("Невозможно открыть лог-файл, так как расширение файла имеет неподдерживаемый формат");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            try
            {
                if (!Path.IsPathRooted(path))
                    path = Path.GetFullPath(path);

                FileName = Path.GetFileNameWithoutExtension(path);
                FileExtension = Path.GetExtension(path);
                FileFullName = FileName + FileExtension;
                FileDirectory = Path.GetDirectoryName(path);
                FileFullPath = path;

                LogFile = new StreamReader(FileFullPath, encoding);
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }

        private void CloseFile()
        {
            LogFile.Close();

            LogFile = null;
        }

        private void ParseFile()
        {
            SituationsOriginalNames = Enum.GetNames(typeof(LogSituation));
            SituationsNames = new string[SituationsOriginalNames.Length];
            Situations = new LogSituation[SituationsOriginalNames.Length];
            SituationsMeetsCounts = new long[SituationsOriginalNames.Length];
            SituationsMeetsLines = new ChunkedArrayD<long>[SituationsOriginalNames.Length];

            for (int i = 0; i < Situations.Length; ++i)
            {
                Situations[i] = (LogSituation) Enum.Parse(typeof(LogSituation), SituationsOriginalNames[i], true);
                SituationsNames[i] = LogUtilities.GetTextFromSituation(Situations[i]).SituationText;

                SituationsMeetsLines[i] = new ChunkedArrayD<long>();
            }

            while (!LogFile.EndOfStream)
            {
                string logLine;

                try
                {
                    logLine = LogFile.ReadLine();
                }
                catch (IOException ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                    continue;
                }

                if (logLine == null)
                    continue;

                try
                {
                    ++LinesCount;
                    string sortText = logLine.Substring(0, logLine.IndexOf('|'));
                    LogSituation situation = LogUtilities.GetSituationFromSortText(sortText);
                    ++SituationsMeetsCounts[(int) situation - 1];
                    SituationsMeetsLines[(int) situation - 1].Add(LinesCount);
                }
                catch (Exception ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                    throw;
                }
            }
        }

        public long GetLinesCount()
        {
            return LinesCount;
        }

        public string[] GetSituationsOriginalNames()
        {
            return SituationsOriginalNames;
        }

        public string[] GetSituationsNames()
        {
            return SituationsNames;
        }

        public LogSituation[] GetSituations()
        {
            return Situations;
        }

        public long GetSituationMeetsCount(LogSituation situation)
        {
            return SituationsMeetsCounts[(int) situation - 1];
        }

        public List<long> GetSituationMeetsLinesList(LogSituation situation)
        {
            ChunkedArrayD<long> linesArray = SituationsMeetsLines[(int) situation - 1];
            int linesCount = ((ICollection) linesArray).Count;
            List<long> linesList = new List<long>(linesCount);

            linesList.AddRange(new long[linesCount]);
            linesArray.CopyTo(linesList, false);

            return linesList;
        }
        public ChunkedArrayD<long> GetSituationMeetsLinesArray(LogSituation situation)
        {
            return SituationsMeetsLines[(int)situation - 1];
        }
    }
}
