// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Security.Cryptography;

namespace RIS.Cryptography.Cipher.Methods
{
    public sealed class RSAiCNG : ICipherMethod
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        private RSACng RSAServiceEncryptor { get; }
        private RSACng RSAServiceDecryptor { get; }

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

        public RSAiCNG()
            : this(RSAKeySize.L2048Bit)
        {

        }
        public RSAiCNG(int publicKeySize)
        {
            try
            {
                RSACng rsaService = new RSACng();
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

                RSAServiceEncryptor = new RSACng(publicKeySize);

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
        public RSAiCNG(int publicKeySize, int privateKeySize)
        {
            try
            {
                RSACng rsaService = new RSACng();
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

                RSAServiceEncryptor = new RSACng(publicKeySize);
                RSAServiceDecryptor = new RSACng(privateKeySize);
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
        public RSAiCNG(RSAKeySize publicKeySize)
        {
            try
            {
                RSAServiceEncryptor = new RSACng((int)publicKeySize);

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
        public RSAiCNG(RSAKeySize publicKeySize, RSAKeySize privateKeySize)
        {
            try
            {
                RSAServiceEncryptor = new RSACng((int)publicKeySize);
                RSAServiceDecryptor = new RSACng((int)privateKeySize);
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
        public RSAiCNG(string xmlPublicKey, int publicKeySize)
        {
            try
            {
                if (xmlPublicKey.Length != 0)
                {
                    RSACng rsaService = new RSACng();
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

                    RSAServiceEncryptor = new RSACng(publicKeySize);
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
        public RSAiCNG(string xmlPublicKey, string xmlPrivateKey, int publicKeySize, int privateKeySize)
        {
            try
            {
                if (xmlPublicKey.Length != 0 && xmlPrivateKey.Length != 0)
                {
                    RSACng rsaService = new RSACng();
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

                    RSAServiceEncryptor = new RSACng(publicKeySize);
                    RSAServiceEncryptor.FromXmlString(xmlPublicKey);
                    RSAServiceDecryptor = new RSACng(privateKeySize);
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
        public RSAiCNG(string xmlPublicKey, RSAKeySize publicKeySize)
        {
            try
            {
                if (xmlPublicKey.Length != 0)
                {
                    RSAServiceEncryptor = new RSACng((int)publicKeySize);
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
        public RSAiCNG(string xmlPublicKey, string xmlPrivateKey, RSAKeySize publicKeySize, RSAKeySize privateKeySize)
        {
            try
            {
                if (xmlPublicKey.Length != 0 && xmlPrivateKey.Length != 0)
                {
                    RSAServiceEncryptor = new RSACng((int)publicKeySize);
                    RSAServiceEncryptor.FromXmlString(xmlPublicKey);
                    RSAServiceDecryptor = new RSACng((int)privateKeySize);
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
            byte[] data = Utils.GetBytes(plainText);

            return Convert.ToBase64String(
                Encrypt(data));
        }
        public byte[] Encrypt(byte[] data)
        {
            try
            {
                byte[] encryptedData;

                if (data.Length == 0)
                    return Array.Empty<byte>();

                encryptedData = RSAServiceEncryptor.Encrypt(data, Padding);

                if (ReverseBytes)
                    Array.Reverse(encryptedData);

                return encryptedData;
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                return Array.Empty<byte>();
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
            byte[] data = Convert.FromBase64String(cipherText);

            return Utils.GetString(
                Decrypt(data));
        }
        public byte[] Decrypt(byte[] data)
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

                if (data.Length == 0)
                    return Array.Empty<byte>();

                if (ReverseBytes)
                    Array.Reverse(data);

                decryptedData = RSAServiceDecryptor.Decrypt(data, Padding);

                return decryptedData;
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }
        }

        public string GetPublicKey()
        {
            return RSAServiceEncryptor.ToXmlString(false);
        }

        public string GetPrivateKey()
        {
            return RSAServiceDecryptor.ToXmlString(true);
        }

        public bool SetPublicKey(string xmlPublicKey)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlPublicKey))
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
                if (string.IsNullOrEmpty(xmlPrivateKey))
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
