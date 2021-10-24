// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using RIS.Cryptography.Hash.Metadata;

namespace RIS.Cryptography.Hash.Methods
{
    public sealed class BCrypt : IHashMethod
    {
        private global::BCrypt.Net.HashType _hashMethod;
        public BCryptHashType HashMethod
        {
            get
            {
                Enum.TryParse(_hashMethod.ToString(), true, out BCryptHashType hashMethod);
                return hashMethod;
            }
            set
            {
                Enum.TryParse(value.ToString(), true, out global::BCrypt.Net.HashType hashMethod);
                _hashMethod = hashMethod;
            }
        }
        private global::BCrypt.Net.HashType HashMethodOriginal
        {
            get
            {
                return _hashMethod;
            }
            set
            {
                _hashMethod = value;
            }
        }
        public bool UseEnhancedAlgorithm { get; set; }
        private int _workFactor;
        public int WorkFactor
        {
            get
            {
                return _workFactor;
            }
            set
            {
                if (value < 4)
                    value = 4;
                else if (value > 31)
                    value = 31;

                _workFactor = value;
            }
        }

        public bool Initialized { get; }

        public BCrypt()
        {
            HashMethod = BCryptHashType.SHA384;
            UseEnhancedAlgorithm = true;
            WorkFactor = 14;

            Initialized = true;
        }

        public static BCryptMetadata GetMetadata(string hashText)
        {
            return new BCryptMetadata(hashText);
        }

        public string GetHash(string plainText)
        {
            return global::BCrypt.Net.BCrypt.HashPassword(
                plainText,
                global::BCrypt.Net.BCrypt.GenerateSalt(WorkFactor),
                UseEnhancedAlgorithm,
                HashMethodOriginal);
        }
        public string GetHash(byte[] data)
        {
            string plainText = SecureUtils.GetString(data);

            return GetHash(plainText);
        }

        public bool VerifyHash(string plainText, string hashText)
        {
            var plainTextHash = global::BCrypt.Net.BCrypt.HashPassword(
                plainText,
                hashText,
                UseEnhancedAlgorithm,
                HashMethodOriginal);

            return SecureUtils.SecureEqualsUnsafe(
                plainTextHash, hashText,
                false, null);
        }
        public bool VerifyHash(byte[] data, string hashText)
        {
            string plainText = SecureUtils.GetString(data);

            return VerifyHash(plainText, hashText);
        }

        public bool VerifyAndUpdateHash(string plainText, string hashText,
            out bool isUpdated, out string newHashText)
        {
            return VerifyAndUpdateHash(plainText, hashText,
                WorkFactor, out isUpdated, out newHashText);
        }
        public bool VerifyAndUpdateHash(byte[] data, string hashText,
            out bool isUpdated, out string newHashText)
        {
            string plainText = SecureUtils.GetString(data);

            return VerifyAndUpdateHash(plainText, hashText,
               out isUpdated, out newHashText);
        }
        public bool VerifyAndUpdateHash(string plainText, string hashText,
            int newWorkFactor, out bool isUpdated, out string newHashText)
        {
            bool result;
            isUpdated = false;
            newHashText = hashText;

            result = VerifyHash(plainText, hashText);

            if (!result)
                return false;

            if (newWorkFactor < 4)
                newWorkFactor = 4;
            else if (newWorkFactor > 31)
                newWorkFactor = 31;

            BCryptMetadata metadata = GetMetadata(hashText);

            isUpdated = metadata.WorkFactor != newWorkFactor;

            if (isUpdated)
            {
                newHashText = global::BCrypt.Net.BCrypt.HashPassword(
                    plainText,
                    global::BCrypt.Net.BCrypt.GenerateSalt(newWorkFactor),
                    UseEnhancedAlgorithm,
                    HashMethodOriginal);
            }
            else
            {
                newHashText = hashText;
            }

            return true;
        }
        public bool VerifyAndUpdateHash(byte[] data, string hashText,
            int newWorkFactor, out bool isUpdated, out string newHashText)
        {
            string plainText = SecureUtils.GetString(data);

            return VerifyAndUpdateHash(plainText, hashText,
                newWorkFactor, out isUpdated, out newHashText);
        }
    }
}
