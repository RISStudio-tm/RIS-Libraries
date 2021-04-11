// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Globalization;
using System.Text;
using RIS.Text.Encoding.Base;

namespace RIS.Cryptography.Hash.Methods
{
    public sealed class Argon2dRaw : IHashMethod
    {
        private byte[] _salt;
        public byte[] SaltBytes
        {
            get
            {
                return _salt;
            }
            set
            {
                if (value.Length < 8)
                    value = new byte[8];

                _salt = value;
            }
        }
        public string Salt
        {
            get
            {
                return Convert.ToBase64String(_salt);
            }
            set
            {
                if (Base64.IsBase64(value))
                {
                    try
                    {
                        var salt =
                            Convert.FromBase64String(value);

                        if (salt.Length < 8)
                        {
                            _salt =
                                Convert.FromBase64String(Convert.ToBase64String(new byte[8]));

                            return;
                        }

                        _salt = salt;
                    }
                    catch (FormatException)
                    {
                        var salt =
                            Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(value)));

                        if (salt.Length < 8)
                        {
                            _salt =
                                Convert.FromBase64String(Convert.ToBase64String(new byte[8]));

                            return;
                        }

                        _salt = salt;
                    }
                }
                else
                {
                    var salt =
                        Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(value)));

                    if (salt.Length < 8)
                    {
                        _salt =
                            Convert.FromBase64String(Convert.ToBase64String(new byte[8]));

                        return;
                    }

                    _salt = salt;
                }
            }
        }
        private ushort _hashLength;
        public ushort HashLength
        {
            get
            {
                return _hashLength;
            }
            set
            {
                if (value < 6)
                    value = 6;

                _hashLength = value;
            }
        }
        private int _degreeOfParallelism;
        public int DegreeOfParallelism
        {
            get
            {
                return _degreeOfParallelism;
            }
            set
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
            set
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
            set
            {
                if (value < 8)
                    value = 8;

                _memorySize = value;
            }
        }
        private byte[] _associatedData;
        public byte[] AssociatedDataBytes
        {
            get
            {
                return _associatedData;
            }
            set
            {
                _associatedData = value;
            }
        }
        public string AssociatedData
        {
            get
            {
                return Convert.ToBase64String(_associatedData);
            }
            set
            {
                if (Base64.IsBase64(value))
                {
                    try
                    {
                        _associatedData =
                            Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _associatedData =
                            Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(value)));
                    }
                }
                else
                {
                    _associatedData =
                        Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(value)));
                }
            }
        }
        private byte[] _knownSecret;
        public byte[] KnownSecretBytes
        {
            get
            {
                return _knownSecret;
            }
            set
            {
                _knownSecret = value;
            }
        }
        public string KnownSecret
        {
            get
            {
                return Convert.ToBase64String(_knownSecret);
            }
            set
            {
                if (Base64.IsBase64(value))
                {
                    try
                    {
                        _knownSecret =
                            Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _knownSecret =
                            Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(value)));
                    }
                }
                else
                {
                    _knownSecret =
                        Convert.FromBase64String(Convert.ToBase64String(SecureUtils.GetBytes(value)));
                }
            }
        }
        public bool FixedHashLength { get; set; }

        public bool Initialized { get; }

        public Argon2dRaw()
        {
            SaltBytes = new byte[8];
            HashLength = 6;
            DegreeOfParallelism = 2 * 2;
            Iterations = 4;
            MemorySize = (1 * 1024) * 128;
            AssociatedDataBytes = Array.Empty<byte>();
            KnownSecretBytes = Array.Empty<byte>();

            FixedHashLength = true;

            Initialized = true;
        }

        public string GetHash(string plainText)
        {
            byte[] data = SecureUtils.GetBytes(plainText);

            return GetHash(data);
        }
        public string GetHash(byte[] data)
        {
            Konscious.Security.Cryptography.Argon2d argon2Service = new Konscious.Security.Cryptography.Argon2d(data)
            {
                Salt = SaltBytes,
                DegreeOfParallelism = DegreeOfParallelism,
                Iterations = Iterations,
                MemorySize = MemorySize,
                AssociatedData = AssociatedDataBytes,
                KnownSecret = KnownSecretBytes
            };

            byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : SaltBytes.Length);

            StringBuilder hashText = new StringBuilder(hashBytes.Length * 2);
            for (int i = 0; i < hashBytes.Length; ++i)
            {
                hashText.Append(hashBytes[i].ToString("x2", CultureInfo.InvariantCulture));
            }

            return hashText.ToString();
        }

        public bool VerifyHash(string plainText, string hashText)
        {
            byte[] data = SecureUtils.GetBytes(plainText);

            return VerifyHash(data, hashText);
        }
        public bool VerifyHash(byte[] data, string hashText)
        {
            var plainTextHash = GetHash(data);

            return SecureUtils.SecureEquals(plainTextHash, hashText,
                true, null);
        }
    }
}
