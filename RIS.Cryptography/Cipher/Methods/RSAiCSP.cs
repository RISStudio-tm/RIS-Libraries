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

        private RSACryptoServiceProvider RSAServiceEncryptor { get; set; }
        private RSACryptoServiceProvider RSAServiceDecryptor { get; set; }

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
                if (RSAServiceDecryptor == null)
                {
                    var exception = new Exception(
                        $"CipherMethod[{ GetType().FullName }] not contained PrivateKey");
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }

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
                if (RSAServiceDecryptor == null)
                {
                    var exception = new Exception(
                        $"CipherMethod[{ GetType().FullName }] not contained PrivateKey");
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }

                return RSAServiceDecryptor.KeySize;
            }
        }
        public RSAEncryptionPadding Padding { get; }
        public bool ReverseBytes { get; set; }

        public bool Initialized { get; }

        public RSAiCSP()
            : this(RSAKeySize.L2048Bit, RSAKeySize.L2048Bit)
        {

        }
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
                            Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                            OnError(new RErrorEventArgs(exception, exception.Message));
                            throw exception;
                        }
                    }
                }

                rsaService.Dispose();

                RSAServiceEncryptor = new RSACryptoServiceProvider(publicKeySize);

                Padding = RSAEncryptionPadding.OaepSHA1;
                ReverseBytes = false;
                Initialized = true;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));

                var exception = new Exception($"CipherMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));

                throw;
            }
        }
        public RSAiCSP(int publicKeySize, int privateKeySize)
            : this(publicKeySize)
        {
            try
            {
                RSACryptoServiceProvider rsaService = new RSACryptoServiceProvider();
                KeySizes keySizes = rsaService.LegalKeySizes[0];
                int countSizes = ((keySizes.MaxSize - keySizes.MinSize) / keySizes.SkipSize) + 1;
                int size = keySizes.MinSize;

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
                            Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                            OnError(new RErrorEventArgs(exception, exception.Message));
                            throw exception;
                        }
                    }
                }

                rsaService.Dispose();

                RSAServiceDecryptor = new RSACryptoServiceProvider(privateKeySize);
                RSAServiceDecryptor.FromXmlString(RSAServiceEncryptor.ToXmlString(true));
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));

                var exception = new Exception($"CipherMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));

                throw;
            }
        }
        public RSAiCSP(RSAKeySize publicKeySize)
            : this((int)publicKeySize)
        {

        }
        public RSAiCSP(RSAKeySize publicKeySize, RSAKeySize privateKeySize)
            : this((int)publicKeySize, (int)privateKeySize)
        {

        }
        public RSAiCSP(string xmlPublicKey, int publicKeySize)
            : this(publicKeySize)
        {
            try
            {
                RSAServiceEncryptor.FromXmlString(xmlPublicKey);
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));

                var exception = new Exception($"CipherMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));

                throw;
            }
        }
        public RSAiCSP(string xmlPublicKey, string xmlPrivateKey, int publicKeySize, int privateKeySize)
            : this(publicKeySize, privateKeySize)
        {
            try
            {
                RSAServiceEncryptor.FromXmlString(xmlPublicKey);
                RSAServiceDecryptor.FromXmlString(xmlPrivateKey);
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));

                var exception = new Exception($"CipherMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));

                throw;
            }
        }
        public RSAiCSP(string xmlPublicKey, RSAKeySize publicKeySize)
            : this(xmlPublicKey, (int)publicKeySize)
        {

        }
        public RSAiCSP(string xmlPublicKey, string xmlPrivateKey, RSAKeySize publicKeySize, RSAKeySize privateKeySize)
            : this(xmlPublicKey, xmlPrivateKey, (int)publicKeySize, (int)privateKeySize)
        {

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
            byte[] data = SecureUtils.GetBytes(plainText);

            return Convert.ToBase64String(
                Encrypt(data));
        }
        public byte[] Encrypt(byte[] data)
        {
            try
            {
                if (data.Length == 0)
                    return Array.Empty<byte>();

                var encryptedData = RSAServiceEncryptor.Encrypt(data, Padding);

                if (ReverseBytes)
                    Array.Reverse(encryptedData);

                return encryptedData;
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }

        public string Decrypt(string cipherText)
        {
            byte[] data = Convert.FromBase64String(cipherText);

            return SecureUtils.GetString(
                Decrypt(data));
        }
        public byte[] Decrypt(byte[] data)
        {
            try
            {
                if (RSAServiceDecryptor == null)
                {
                    var exception = new Exception(
                        $"CipherMethod[{ GetType().FullName }] not contained PrivateKey");
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }

                if (data.Length == 0)
                    return Array.Empty<byte>();

                if (ReverseBytes)
                    Array.Reverse(data);

                var decryptedData = RSAServiceDecryptor.Decrypt(data, Padding);

                return decryptedData;
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }

        public string GetPublicKey()
        {
            return RSAServiceEncryptor.ToXmlString(false);
        }

        public string GetPrivateKey()
        {
            if (RSAServiceDecryptor == null)
            {
                var exception = new Exception(
                    $"CipherMethod[{ GetType().FullName }] not contained PrivateKey");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

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
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }

        public bool SetPrivateKey(string xmlPrivateKey)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlPrivateKey))
                    return false;

                if (RSAServiceDecryptor == null)
                    RSAServiceDecryptor = new RSACryptoServiceProvider(PublicKeySize);

                RSAServiceDecryptor.FromXmlString(xmlPrivateKey);

                return true;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }
    }
}
