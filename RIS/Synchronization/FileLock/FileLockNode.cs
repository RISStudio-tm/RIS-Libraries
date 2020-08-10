// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RIS.Synchronization
{
    internal sealed class FileLockNode
    {
        public static event EventHandler<RInformationEventArgs> Information;
		public static event EventHandler<RWarningEventArgs> Warning;
		public static event EventHandler<RErrorEventArgs> Error;

        private readonly string _path;

        public FileLockNode(string path)
        {
            _path = path;
        }

        internal async Task<bool> TrySetReleaseDate(DateTime date)
        {
            try
            {
                using (var fs = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    using (var sr = new StreamWriter(fs, Encoding.UTF8))
                    {
                        await sr.WriteAsync(date.ToUniversalTime().Ticks.ToString()).ConfigureAwait(false);
                    }
                }

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        internal async Task<DateTime> GetReleaseDate(string path = "")
        {
            try
            {
                using (var fs = new FileStream(string.IsNullOrWhiteSpace(path) ? _path : path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        string text = await sr.ReadToEndAsync().ConfigureAwait(false);
                        long ticks = long.Parse(text);

                        return new DateTime(ticks, DateTimeKind.Utc);
                    }
                }
            }
            catch(Exception)
            {
                return DateTime.MaxValue;
            }
        }
    }
}
