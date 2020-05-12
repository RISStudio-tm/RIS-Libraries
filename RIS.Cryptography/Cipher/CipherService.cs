using System;

namespace RIS.Cryptography.Cipher
{
    public class CipherService
    {
        public event EventHandler<RMessageEventArgs> ShowMessage;
        public event EventHandler<RErrorEventArgs> ShowError;

        public ICipherMethod CipherMethod { get; private set; }

        public CipherService(ICipherMethod cipherMethod)
        {
            if (!SetCipherMethod(cipherMethod))
            {
                var exception = new Exception("SetCipherMethod return false. CipherService is not initialized");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
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
