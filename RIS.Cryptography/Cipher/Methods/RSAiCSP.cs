// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Security.Cryptography;

namespace RIS.Cryptography.Cipher.Methods
{
    public sealed class RSAiCSP : ICipherMethod
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        public enum CipherKeySizes
        {
            L512Bit = 512,
            L1024Bit = 1024,
            L2048Bit = 2048,
            L4096Bit = 4096,
            L8192Bit = 8192,
            L16384Bit = 16384
        }

        private RSACryptoServiceProvider RSAServiceEncryptor { get; }
        private RSACryptoServiceProvider RSAServiceDecryptor { get; }

        public string PublicKey
        {
            get
            {
                return RSAServiceEncryptor.ToXmlString(false);
            }
        }
        public string PrivateKey
        {
            get
            {
                return RSAServiceDecryptor.ToXmlString(true);
            }
        }
        public int PublicKeySize
        {
            get
            {
                return RSAServiceEncryptor.KeySize;
            }
        }
        public int PrivateKeySize
        {
            get
            {
                return RSAServiceDecryptor.KeySize;
            }
        }
        public RSAEncryptionPadding Padding { get; }
        public bool ReverseBytes { get; set; }

        public bool Initialized { get; }

        public RSAiCSP(int publicKeySize)
        {
            try
            {
                RSACryptoServiceProvider rsaService = new RSACryptoServiceProvider();
                KeySizes keySizes = rsaService.LegalKeySizes[0];
                int countSizes = ((keySizes.MaxSize - keySizes.MinSize) / keySizes.SkipSize) + 1;
                int size = keySizes.MinSize;

                for (int i = 0; i < countSizes; ++i)
                {
                    if (size == publicKeySize)
                    {
                        rsaService.KeySize = publicKeySize;
                        break;
                    }

                    if (i != countSizes - 1)
                    {
                        size += keySizes.SkipSize;
                    }
                    else
                    {
                        if (rsaService.KeySize != publicKeySize)
                        {
                            var exception = new Exception(
                                $"KeySize[{publicKeySize}] not supported for CipherMethod[{GetType().FullName}]");
                            Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                            OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                            throw exception;
                        }
                    }
                }

                rsaService.Dispose();

                RSAServiceEncryptor = new RSACryptoServiceProvider(publicKeySize);

                Padding = RSAEncryptionPadding.OaepSHA384;
                ReverseBytes = false;
                Initialized = true;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));

                var exception = new Exception($"CipherMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));

                throw;
            }
        }
        public RSAiCSP(int publicKeySize, int privateKeySize)
        {
            try
            {
                RSACryptoServiceProvider rsaService = new RSACryptoServiceProvider();
                KeySizes keySizes = rsaService.LegalKeySizes[0];
                int countSizes = ((keySizes.MaxSize - keySizes.MinSize) / keySizes.SkipSize) + 1;
                int size = keySizes.MinSize;

                for (int i = 0; i < countSizes; ++i)
                {
                    if (size == publicKeySize)
                    {
                        rsaService.KeySize = publicKeySize;
                        break;
                    }

                    if (i != countSizes - 1)
                    {
                        size += keySizes.SkipSize;
                    }
                    else
                    {
                        if (rsaService.KeySize != publicKeySize)
                        {
                            var exception = new Exception(
                                $"KeySize[{publicKeySize}] not supported for CipherMethod[{GetType().FullName}]");
                            Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                            OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                            throw exception;
                        }
                    }
                }

                size = keySizes.MinSize;

                for (int i = 0; i < countSizes; ++i)
                {
                    if (size == privateKeySize)
                    {
                        rsaService.KeySize = privateKeySize;
                        break;
                    }

                    if (i != countSizes - 1)
                    {
                        size += keySizes.SkipSize;
                    }
                    else
                    {
                        if (rsaService.KeySize != privateKeySize)
                        {
                            var exception = new Exception(
                                $"KeySize[{privateKeySize}] not supported for CipherMethod[{GetType().FullName}]");
                            if (Error == null)
                            {
                                throw exception;
                            }
                            else
                            {
                                Error.Invoke(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                            }
                        }
                    }
                }

                rsaService.Dispose();

                RSAServiceEncryptor = new RSACryptoServiceProvider(publicKeySize);
                RSAServiceDecryptor = new RSACryptoServiceProvider(privateKeySize);
                RSAServiceDecryptor.FromXmlString(RSAServiceEncryptor.ToXmlString(true));

                Padding = RSAEncryptionPadding.OaepSHA384;
                ReverseBytes = false;
                Initialized = true;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));

                var exception = new Exception($"CipherMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));

                throw;
            }
        }
        public RSAiCSP(CipherKeySizes publicKeySize)
        {
            try
            {
                RSAServiceEncryptor = new RSACryptoServiceProvider((int)publicKeySize);

                Padding = RSAEncryptionPadding.OaepSHA384;
                ReverseBytes = false;
                Initialized = true;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));

                var exception = new Exception($"CipherMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));

                throw;
            }
        }
        public RSAiCSP(CipherKeySizes publicKeySize, CipherKeySizes privateKeySize)
        {
            try
            {
                RSAServiceEncryptor = new RSACryptoServiceProvider((int)publicKeySize);
                RSAServiceDecryptor = new RSACryptoServiceProvider((int)privateKeySize);
                RSAServiceDecryptor.FromXmlString(RSAServiceEncryptor.ToXmlString(true));

                Padding = RSAEncryptionPadding.OaepSHA384;
                ReverseBytes = false;
                Initialized = true;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));

                var exception = new Exception($"CipherMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));

                throw;
            }
        }
        public RSAiCSP(string xmlPublicKey, int publicKeySize)
        {
            try
            {
                if (xmlPublicKey.Length != 0)
                {
                    RSACryptoServiceProvider rsaService = new RSACryptoServiceProvider();
                    KeySizes keySizes = rsaService.LegalKeySizes[0];
                    int countSizes = ((keySizes.MaxSize - keySizes.MinSize) / keySizes.SkipSize) + 1;
                    int size = keySizes.MinSize;

                    for (int i = 0; i < countSizes; ++i)
                    {
                        if (size == publicKeySize)
                        {
                            rsaService.KeySize = publicKeySize;
                            break;
                        }

                        if (i != countSizes - 1)
                        {
                            size += keySizes.SkipSize;
                        }
                        else
                        {
                            if (rsaService.KeySize != publicKeySize)
                            {
                                var exception = new Exception(
                                    $"KeySize[{publicKeySize}] not supported for CipherMethod[{GetType().FullName}]");
                                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                                throw exception;
                            }
                        }
                    }

                    rsaService.Dispose();

                    RSAServiceEncryptor = new RSACryptoServiceProvider(publicKeySize);
                    RSAServiceEncryptor.FromXmlString(xmlPublicKey);

                    Padding = RSAEncryptionPadding.OaepSHA384;
                    ReverseBytes = false;
                    Initialized = true;
                }
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));

                var exception = new Exception($"CipherMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));

                throw;
            }
        }
        public RSAiCSP(string xmlPublicKey, string xmlPrivateKey, int publicKeySize, int privateKeySize)
        {
            try
            {
                if (xmlPublicKey.Length != 0 && xmlPrivateKey.Length != 0)
                {
                    RSACryptoServiceProvider rsaService = new RSACryptoServiceProvider();
                    KeySizes keySizes = rsaService.LegalKeySizes[0];
                    int countSizes = ((keySizes.MaxSize - keySizes.MinSize) / keySizes.SkipSize) + 1;
                    int size = keySizes.MinSize;

                    for (int i = 0; i < countSizes; ++i)
                    {
                        if (size == publicKeySize)
                        {
                            rsaService.KeySize = publicKeySize;
                            break;
                        }

                        if (i != countSizes - 1)
                        {
                            size += keySizes.SkipSize;
                        }
                        else
                        {
                            if (rsaService.KeySize != publicKeySize)
                            {
                                var exception = new Exception(
                                    $"KeySize[{publicKeySize}] not supported for CipherMethod[{GetType().FullName}]");
                                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                                throw exception;
                            }
                        }
                    }

                    size = keySizes.MinSize;

                    for (int i = 0; i < countSizes; ++i)
                    {
                        if (size == privateKeySize)
                        {
                            rsaService.KeySize = privateKeySize;
                            break;
                        }

                        if (i != countSizes - 1)
                        {
                            size += keySizes.SkipSize;
                        }
                        else
                        {
                            if (rsaService.KeySize != privateKeySize)
                            {
                                var exception = new Exception(
                                    $"KeySize[{privateKeySize}] not supported for CipherMethod[{GetType().FullName}]");
                                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                                throw exception;
                            }
                        }
                    }

                    rsaService.Dispose();

                    RSAServiceEncryptor = new RSACryptoServiceProvider(publicKeySize);
                    RSAServiceEncryptor.FromXmlString(xmlPublicKey);
                    RSAServiceDecryptor = new RSACryptoServiceProvider(privateKeySize);
                    RSAServiceDecryptor.FromXmlString(xmlPrivateKey);

                    Padding = RSAEncryptionPadding.OaepSHA384;
                    ReverseBytes = false;
                    Initialized = true;
                }
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));

                var exception = new Exception($"CipherMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));

                throw;
            }
        }
        public RSAiCSP(string xmlPublicKey, CipherKeySizes publicKeySize)
        {
            try
            {
                if (xmlPublicKey.Length != 0)
                {
                    RSAServiceEncryptor = new RSACryptoServiceProvider((int)publicKeySize);
                    RSAServiceEncryptor.FromXmlString(xmlPublicKey);

                    Padding = RSAEncryptionPadding.OaepSHA384;
                    ReverseBytes = false;
                    Initialized = true;
                }
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));

                var exception = new Exception($"CipherMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));

                throw;
            }
        }
        public RSAiCSP(string xmlPublicKey, string xmlPrivateKey, CipherKeySizes publicKeySize, CipherKeySizes privateKeySize)
        {
            try
            {
                if (xmlPublicKey.Length != 0 && xmlPrivateKey.Length != 0)
                {
                    RSAServiceEncryptor = new RSACryptoServiceProvider((int)publicKeySize);
                    RSAServiceEncryptor.FromXmlString(xmlPublicKey);
                    RSAServiceDecryptor = new RSACryptoServiceProvider((int)privateKeySize);
                    RSAServiceDecryptor.FromXmlString(xmlPrivateKey);

                    Padding = RSAEncryptionPadding.OaepSHA384;
                    ReverseBytes = false;
                    Initialized = true;
                }
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));

                var exception = new Exception($"CipherMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));

                throw;
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

        public string Encrypt(string plainText)
        {
            try
            {
                byte[] encryptedData;

                if (plainText.Length == 0)
                    return string.Empty;

                byte[] data = Utils.SecureUTF8.GetBytes(plainText);

                encryptedData = RSAServiceEncryptor.Encrypt(data, Padding);

                if (ReverseBytes)
                    Array.Reverse(encryptedData);

                return Convert.ToBase64String(encryptedData);
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                return string.Empty;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                byte[] decryptedData;

                if (RSAServiceDecryptor == null)
                {
                    var exception = new Exception(
                        $"CipherMethod[{ GetType().FullName }] not contained PrivateKey, which is needed for decryption");
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                    throw exception;
                }

                if (cipherText.Length == 0)
                    return string.Empty;

                byte[] data = Convert.FromBase64String(cipherText);

                if (ReverseBytes)
                    Array.Reverse(data);

                decryptedData = RSAServiceDecryptor.Decrypt(data, Padding);

                return Utils.SecureUTF8.GetString(decryptedData);
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                return string.Empty;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
        }

        public bool SetPublicKey(string xmlPublicKey)
        {
            try
            {
                if (xmlPublicKey.Length == 0)
                    return false;

                RSAServiceEncryptor.FromXmlString(xmlPublicKey);
                return true;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
        }

        public bool SetPrivateKey(string xmlPrivateKey)
        {
            try
            {
                if (xmlPrivateKey.Length == 0)
                    return false;

                RSAServiceDecryptor.FromXmlString(xmlPrivateKey);
                return true;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
        }
    }
}
