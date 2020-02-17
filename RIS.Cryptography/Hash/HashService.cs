using System;

namespace RIS.Cryptography.Hash
{
    public class HashService
    {
        public event RMessageHandler ShowMessage;
        public event RErrorHandler ShowError;

        public IHashMethod HashMethod { get; private set; }

        public HashService(IHashMethod hashMethod)
        {
            if (!SetHashMethod(hashMethod))
            {
                var exception = new Exception("SetHashMethod return false. HashService is not initialized");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
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
