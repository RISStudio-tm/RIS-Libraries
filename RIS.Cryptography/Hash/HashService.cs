// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Cryptography.Hash
{
    public class HashService
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        public IHashMethod HashMethod { get; private set; }

        public HashService(IHashMethod hashMethod)
        {
            if (!SetHashMethod(hashMethod))
            {
                var exception = new Exception("SetHashMethod return false. HashService is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
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

        public bool SetHashMethod(IHashMethod hashMethod)
        {
            if (!hashMethod.Initialized)
                return false;

            HashMethod = hashMethod;
            return true;
        }

        public string GetHash(string plainText)
        {
            return HashMethod.GetHash(plainText);
        }
        public string GetHash(string plainText, ushort saltLength, out string hashSalt)
        {
            hashSalt = HashMethods.GenSalt(saltLength);
            return HashMethod.GetHash(hashSalt + plainText + hashSalt);
        }

        public bool VerifyHash(string plainText, string hashText)
        {
            return HashMethod.VerifyHash(plainText, hashText);
        }
        public bool VerifyHash(string plainText, string hashText, string hashSalt)
        {
            return HashMethod.VerifyHash(hashSalt + plainText + hashSalt, hashText);
        }
    }
}
