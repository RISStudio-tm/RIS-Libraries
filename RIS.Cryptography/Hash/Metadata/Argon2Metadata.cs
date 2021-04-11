// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Text.RegularExpressions;
using RIS.Text.Encoding.Base;

namespace RIS.Cryptography.Hash.Metadata
{
    public class Argon2Metadata
    {
        public static readonly Regex HashInfoRegex = new Regex(@"^\$(?<type>argon2[a-z]{0,2}?)\$v=(?<version>\d+?)\$m=(?<memory_size>\d+?),t=(?<iterations>\d+?),p=(?<degree_of_parallelism>\d+?)\$(?<salt>[A-Za-z0-9/+]+)\$(?<hash>[A-Za-z0-9/+]+)$", RegexOptions.Multiline, TimeSpan.FromSeconds(5));

        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        public Argon2Type Type { get; private set; }
        public int Version { get; private set; }
        private ushort _saltLength;
        public ushort SaltLength
        {
            get
            {
                return _saltLength;
            }
            private set
            {
                if (value < 8)
                    value = 8;

                _saltLength = value;
            }
        }
        private byte[] _salt;
        public byte[] SaltBytes
        {
            get
            {
                return _salt;
            }
            private set
            {
                _salt = value;
            }
        }
        public string Salt
        {
            get
            {
                return Convert.ToBase64String(_salt);
            }
            private set
            {
                if (Base64.IsBase64(value))
                {
                    try
                    {
                        _salt =
                            Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _salt =
                            Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(value)));
                    }
                }
                else
                {
                    _salt =
                        Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(value)));
                }
            }
        }
        private byte[] _hash;
        public byte[] HashBytes
        {
            get
            {
                return _hash;
            }
            private set
            {
                _hash = value;
            }
        }
        public string Hash
        {
            get
            {
                return Convert.ToBase64String(_hash);
            }
            private set
            {
                if (Base64.IsBase64(value))
                {
                    try
                    {
                        _hash =
                            Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _hash =
                            Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(value)));
                    }
                }
                else
                {
                    _hash =
                        Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(value)));
                }
            }
        }
        private int _degreeOfParallelism;
        public int DegreeOfParallelism
        {
            get
            {
                return _degreeOfParallelism;
            }
            private set
            {
                if (value < 1)
                    value = 1;

                _degreeOfParallelism = value;
            }
        }
        private int _iterations;
        public int Iterations
        {
            get
            {
                return _iterations;
            }
            private set
            {
                if (value < 1)
                    value = 1;

                _iterations = value;
            }
        }
        private int _memorySize;
        public int MemorySize
        {
            get
            {
                return _memorySize;
            }
            private set
            {
                if (value < 8)
                    value = 8;

                _memorySize = value;
            }
        }

        public Argon2Metadata(string hashText)
        {
            Match hashInfo = HashInfoRegex.Match(hashText);

            if (!hashInfo.Success)
            {
                var exception = new FormatException($"Invalid hash format in metadata[{ GetType().FullName }]");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            Enum.TryParse(hashInfo.Groups["type"].Value, true, out Argon2Type type);
            Type = type;
            Version = Convert.ToInt32(hashInfo.Groups["version"].Value);
            MemorySize = Convert.ToInt32(hashInfo.Groups["memory_size"].Value);
            Iterations = Convert.ToInt32(hashInfo.Groups["iterations"].Value);
            DegreeOfParallelism = Convert.ToInt32(hashInfo.Groups["degree_of_parallelism"].Value);
            Salt = Base64.RestorePadding(hashInfo.Groups["salt"].Value);
            Hash = Base64.RestorePadding(hashInfo.Groups["hash"].Value);
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
            return $"${Enum.GetName(typeof(Argon2Type), Type)?.ToLower()}$v={Version}$m={MemorySize},t={Iterations},p={DegreeOfParallelism}${Base64.RemovePadding(Salt)}${Base64.RemovePadding(Hash)}";
        }
    }
}
