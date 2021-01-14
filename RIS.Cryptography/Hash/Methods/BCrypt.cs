// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

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
            HashMethod = BCryptHashType.SHA512;
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
            string hashText;

            if (UseEnhancedAlgorithm)
                hashText = global::BCrypt.Net.BCrypt.EnhancedHashPassword(plainText, HashMethodOriginal, WorkFactor);
            else
                hashText = global::BCrypt.Net.BCrypt.HashPassword(plainText, global::BCrypt.Net.BCrypt.GenerateSalt(WorkFactor), false, HashMethodOriginal);

            return hashText;
        }
        public string GetHash(byte[] data)
        {
            string plainText = Utils.SecureUTF8.GetString(data);

            return GetHash(plainText);
        }
        public bool VerifyHash(string plainText, string hashText)
        {
            if (UseEnhancedAlgorithm)
                return global::BCrypt.Net.BCrypt.EnhancedVerify(plainText, hashText, HashMethodOriginal);
            else
                return global::BCrypt.Net.BCrypt.Verify(plainText, hashText, false, HashMethodOriginal);
        }

        public bool VerifyAndUpdateHash(string plainText, string hashText, out bool isUpdated, out string newHashText)
        {
            return VerifyAndUpdateHash(plainText, hashText, WorkFactor, out isUpdated, out newHashText);
        }
        public bool VerifyAndUpdateHash(string plainText, string hashText, int newWorkFactor, out bool isUpdated, out string newHashText)
        {
            bool result;
            isUpdated = false;
            newHashText = hashText;

            if (UseEnhancedAlgorithm)
                result = global::BCrypt.Net.BCrypt.EnhancedVerify(plainText, hashText, HashMethodOriginal);
            else
                result = global::BCrypt.Net.BCrypt.Verify(plainText, hashText, false, HashMethodOriginal);

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
                if (UseEnhancedAlgorithm)
                {
                    newHashText = global::BCrypt.Net.BCrypt.EnhancedHashPassword(
                        plainText,
                        HashMethodOriginal,
                        newWorkFactor);
                }
                else
                {
                    newHashText = global::BCrypt.Net.BCrypt.HashPassword(
                        plainText,
                        global::BCrypt.Net.BCrypt.GenerateSalt(newWorkFactor),
                        false,
                        HashMethodOriginal);
                }
            }
            else
            {
                newHashText = hashText;
            }

            return true;
        }
    }
}
