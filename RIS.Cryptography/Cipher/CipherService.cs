// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Cryptography.Cipher
{
    public class CipherService
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        public ICipherMethod CipherMethod { get; private set; }

        public CipherService(ICipherMethod cipherMethod)
        {
            if (!SetCipherMethod(cipherMethod))
            {
                var exception = new Exception("SetCipherMethod return false. CipherService is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
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

        public bool SetCipherMethod(ICipherMethod cipherMethod)
        {
            if (!cipherMethod.Initialized)
                return false;

            CipherMethod = cipherMethod;
            return true;
        }
        
        public string Encrypt(string plainText)
        {
            return CipherMethod.Encrypt(plainText);
        }
        public string Decrypt(string cipherText)
        {
            return CipherMethod.Decrypt(cipherText);
        }
    }
}
