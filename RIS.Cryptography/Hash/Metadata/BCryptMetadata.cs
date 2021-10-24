// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Text.RegularExpressions;

namespace RIS.Cryptography.Hash.Metadata
{
    public class BCryptMetadata
    {
        public static readonly Regex HashInfoRegex = new Regex(@"^\$(?<version>2[a-z]{1}?)\$(?<work_factor>\d\d?)\$(?<hash>[A-Za-z0-9\./]{53})$", RegexOptions.Multiline, TimeSpan.FromSeconds(5));

        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        public string Version { get; private set; }
        private int _workFactor;
        public int WorkFactor
        {
            get
            {
                return _workFactor;
            }
            private set
            {
                if (value < 4)
                    value = 4;
                else if (value > 31)
                    value = 31;

                _workFactor = value;
            }
        }
        public string Hash { get; private set; }

        public BCryptMetadata(string hashText)
        {
            BCrypt.Net.HashInformation hashInfo;

            try
            {
                hashInfo = BCrypt.Net.BCrypt.InterrogateHash(hashText);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                var exception = new FormatException($"Invalid hash format in metadata[{GetType().FullName}]");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            Version = hashInfo.Version;
            WorkFactor = Convert.ToInt32(hashInfo.WorkFactor);
            Hash = hashInfo.RawHash;
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

        public override string ToString()
        {
            return $"${Version}${WorkFactor}${Hash}";
        }
    }
}
