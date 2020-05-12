using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RIS.Collections;
using RIS.Collections.ChunkedCollections;

namespace RIS.Logging.Parsing
{
    public sealed class LogParseInfo
    {
        public event EventHandler<RMessageEventArgs> ShowMessage;
        public event EventHandler<RErrorEventArgs> ShowError;

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
        {
            LogFile = null;
            FileEncoding = Encoding.UTF8;

            Task parse = Task.Factory.StartNew(() =>
            {
                OpenFile(path, Encoding.UTF8);
                ParseFile();
                CloseFile();
            });

            parse.Wait();
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

        private void OpenFile(string path, Encoding encoding)
        {
            if (!File.Exists(path))
            {
                var exception = new DirectoryNotFoundException("Невозможно открыть лог-файл, так как указанный каталог не существует");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
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
                Events.DShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
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
                LogUtilities.GetTextFromSituation(Situations[i], out string situationText);
                SituationsNames[i] = situationText;

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
                    Events.DShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    ShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
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
                catch (IOException ex)
                {
                    Events.DShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    ShowError?.Invoke(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
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
            linesArray.CopyTo(linesList);

            return linesList;
        }
        public ChunkedArrayD<long> GetSituationMeetsLinesArray(LogSituation situation)
        {
            return SituationsMeetsLines[(int)situation - 1];
        }
    }
}
