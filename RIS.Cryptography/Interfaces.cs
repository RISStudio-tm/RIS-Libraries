using System;

namespace RIS.Cryptography
{
    public interface ICipherMethod
    {
        bool Initialized { get; }

        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }

    public interface IHashMethod
    {
        bool Initialized { get; }

        string GetHash(string plainText);
        bool VerifyHash(string plainText, string hashText);
    }
}
