// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Globalization;
using System.Text;

namespace RIS.Cryptography.Hash.Methods
{
    public sealed class Argon2iWNP : IHashMethod
    {
        private ushort _saltLength;
        public ushort SaltLength
        {
            get
            {
                return _saltLength;
            }
            set
            {
                if (value < 8)
                    value = 8;

                _saltLength = value;
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
                try
                {
                    _associatedData = Convert.FromBase64String(value);
                }
                catch (FormatException)
                {
                    _associatedData = Convert.FromBase64String(Convert.ToBase64String(Utils.GetBytes(value)));
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
                try
                {
                    _knownSecret = Convert.FromBase64String(value);
                }
                catch (FormatException)
                {
                    _knownSecret = Convert.FromBase64String(Convert.ToBase64String(Utils.GetBytes(value)));
                }
            }
        }
        public bool FixedHashLength { get; set; }

        public bool Initialized { get; }

        public Argon2iWNP()
        {
            SaltLength = 8;
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
            byte[] data = Utils.GetBytes(plainText);

            return GetHash(data);
        }
        public string GetHash(byte[] data)
        {
            byte[] hashSalt = HashMethodsUtils.GenerateSaltBytes(SaltLength);

            Konscious.Security.Cryptography.Argon2i argon2Service = new Konscious.Security.Cryptography.Argon2i(data)
            {
                Salt = hashSalt,
                DegreeOfParallelism = DegreeOfParallelism,
                Iterations = Iterations,
                MemorySize = MemorySize,
                AssociatedData = AssociatedDataBytes,
                KnownSecret = KnownSecretBytes
            };

            byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);

            StringBuilder hashText = new StringBuilder(hashBytes.Length * 2);
            for (int i = 0; i < hashBytes.Length; ++i)
            {
                hashText.Append(hashBytes[i].ToString("x2", CultureInfo.InvariantCulture));
            }

            string hashString = Convert.ToBase64String(hashSalt) + "=/" + Convert.ToBase64String(Utils.GetBytes(hashText.ToString()));
            hashString = Convert.ToBase64String(Utils.GetBytes(hashString));

            return hashString;
        }
        public string GetHash(string plainText, string salt)
        {
            byte[] data = Utils.GetBytes(plainText);

            return GetHash(data, salt);
        }
        public string GetHash(byte[] data, string salt)
        {
            byte[] hashSalt;
            try
            {
                hashSalt = Convert.FromBase64String(salt);
            }
            catch (FormatException)
            {
                hashSalt = Convert.FromBase64String(Convert.ToBase64String(Utils.GetBytes(salt)));
            }

            Konscious.Security.Cryptography.Argon2i argon2Service = new Konscious.Security.Cryptography.Argon2i(data)
            {
                Salt = hashSalt,
                DegreeOfParallelism = DegreeOfParallelism,
                Iterations = Iterations,
                MemorySize = MemorySize,
                AssociatedData = AssociatedDataBytes,
                KnownSecret = KnownSecretBytes
            };

            byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);

            StringBuilder hashText = new StringBuilder(hashBytes.Length * 2);
            for (int i = 0; i < hashBytes.Length; ++i)
            {
                hashText.Append(hashBytes[i].ToString("x2", CultureInfo.InvariantCulture));
            }

            string hashString = Convert.ToBase64String(hashSalt) + "=/" + Convert.ToBase64String(Utils.GetBytes(hashText.ToString()));
            hashString = Convert.ToBase64String(Utils.GetBytes(hashString));

            return hashString;
        }

        public bool VerifyHash(string plainText, string hashText)
        {
            byte[] data = Utils.GetBytes(plainText);

            return VerifyHash(data, hashText);
        }
        public bool VerifyHash(byte[] data, string hashText)
        {
            string hashTextSub = Utils.GetString(
                Convert.FromBase64String(hashText));

            string hashSalt = hashTextSub.Substring(
                0, hashTextSub.IndexOf('='));
            if (hashTextSub.Contains("===/"))
                hashSalt += "==";
            else if (hashTextSub.Contains("==/"))
                hashSalt += "=";

            var plainTextHash = GetHash(data, hashSalt);

            return Utils.SecureEquals(plainTextHash, hashText,
                false, null);
        }
    }
}
