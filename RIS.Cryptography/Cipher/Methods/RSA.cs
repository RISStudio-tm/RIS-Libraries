// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.IO;
using System.Security.Cryptography;

namespace RIS.Cryptography.Cipher.Methods
{
    public sealed class RSA : ICipherMethod, ISignMethod
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        private System.Security.Cryptography.RSA RsaService { get; set; }

        public string PublicKey
        {
            get
            {
                return RsaService.ToXmlString(false);
            }
        }
        public string PrivateKey
        {
            get
            {
                return RsaService.ToXmlString(true);
            }
        }
        public int KeySize
        {
            get
            {
                return RsaService.KeySize;
            }
        }
        public RSAEncryptionPadding Padding { get; }
        public RSASignaturePadding SignPadding { get; }
        public HashAlgorithmName SignHashAlgorithm { get; }
        public bool ReverseBytes { get; set; }

        public bool Initialized { get; }

        public RSA()
            : this(RSAKeySize.L2048Bit)
        {

        }
        public RSA(int keySize)
        {
            try
            {
                var rsaService = System.Security.Cryptography.RSA.Create();
                var keySizes = rsaService.LegalKeySizes[0];
                var countSizes = ((keySizes.MaxSize - keySizes.MinSize) / keySizes.SkipSize) + 1;
                var size = keySizes.MinSize;

                for (var i = 0; i < countSizes; ++i)
                {
                    if (size == keySize)
                    {
                        rsaService.KeySize = keySize;
                        break;
                    }

                    if (i != countSizes - 1)
                    {
                        size += keySizes.SkipSize;
                    }
                    else
                    {
                        if (rsaService.KeySize != keySize)
                        {
                            var exception = new Exception(
                                $"KeySize[{keySize}] not supported for CipherMethod[{GetType().FullName}]");
                            Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                            OnError(new RErrorEventArgs(exception, exception.Message));
                            throw exception;
                        }
                    }
                }

                rsaService.Dispose();

                RsaService = System.Security.Cryptography.RSA.Create(keySize);

                Padding = RSAEncryptionPadding.OaepSHA1;
                SignPadding = RSASignaturePadding.Pkcs1;
                SignHashAlgorithm = HashAlgorithmName.SHA384;
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
        public RSA(RSAKeySize keySize)
            : this((int)keySize)
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
            var data = SecureUtils.GetBytes(plainText);

            return Convert.ToBase64String(
                Encrypt(data));
        }
        public byte[] Encrypt(byte[] data)
        {
            try
            {
                if (data.Length == 0)
                    return Array.Empty<byte>();

                var encryptedData = RsaService.Encrypt(data, Padding);

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
            var data = Convert.FromBase64String(cipherText);

            return SecureUtils.GetString(
                Decrypt(data));
        }
        public byte[] Decrypt(byte[] data)
        {
            try
            {
                if (data.Length == 0)
                    return Array.Empty<byte>();

                if (ReverseBytes)
                    Array.Reverse(data);

                var decryptedData = RsaService.Decrypt(data, Padding);

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

        public string GetXmlPublicKey()
        {
            return RsaService.ToXmlString(false);
        }

        public string GetXmlPrivateKey()
        {
            if (RsaService == null)
            {
                var exception = new Exception(
                    $"CipherMethod[{ GetType().FullName }] not contained PrivateKey");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return RsaService.ToXmlString(true);
        }

        public byte[] ExportPublicKey()
        {
            return RsaService.ExportRSAPublicKey();
        }

        public byte[] ExportPrivateKey()
        {
            return RsaService.ExportRSAPrivateKey();
        }

        public string SignData(string plainText)
        {
            var data = SecureUtils.GetBytes(plainText);

            return Convert.ToBase64String(
                SignData(data));
        }
        public byte[] SignData(byte[] data)
        {
            try
            {
                if (data.Length == 0)
                    return Array.Empty<byte>();

                var signedData = RsaService.SignData(
                    data, SignHashAlgorithm, SignPadding);

                if (ReverseBytes)
                    Array.Reverse(signedData);

                return signedData;
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
        public byte[] SignData(Stream data)
        {
            try
            {
                if (data.Length == 0)
                    return Array.Empty<byte>();

                var signData = RsaService.SignData(
                    data, SignHashAlgorithm, SignPadding);

                if (ReverseBytes)
                    Array.Reverse(signData);

                return signData;
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

        public string SignHash(string plainText)
        {
            var data = SecureUtils.GetBytes(plainText);

            return Convert.ToBase64String(
                SignHash(data));
        }
        public byte[] SignHash(byte[] data)
        {
            try
            {
                if (data.Length == 0)
                    return Array.Empty<byte>();

                var signData = RsaService.SignHash(
                    data, SignHashAlgorithm, SignPadding);

                if (ReverseBytes)
                    Array.Reverse(signData);

                return signData;
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

        public bool VerifyData(string dataEncoded, string signDataEncoded)
        {
            var data = Convert.FromBase64String(
                dataEncoded);
            var signData = Convert.FromBase64String(
                signDataEncoded);

            return VerifyData(
                data, signData);
        }
        public bool VerifyData(byte[] data, byte[] signData)
        {
            try
            {
                if (data.Length == 0)
                    return false;

                if (ReverseBytes)
                    Array.Reverse(signData);

                var verifySuccess = RsaService.VerifyData(
                    data, signData, SignHashAlgorithm, SignPadding);

                return verifySuccess;
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                return false;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }
        public bool VerifyData(Stream data, byte[] signData)
        {
            try
            {
                if (data.Length == 0)
                    return false;

                if (ReverseBytes)
                    Array.Reverse(signData);

                var verifySuccess = RsaService.VerifyData(
                    data, signData, SignHashAlgorithm, SignPadding);

                return verifySuccess;
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                return false;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }

        public bool VerifyHash(string dataEncoded, string signDataEncoded)
        {
            var data = Convert.FromBase64String(
                dataEncoded);
            var signData = Convert.FromBase64String(
                signDataEncoded);

            return VerifyHash(
                data, signData);
        }
        public bool VerifyHash(byte[] data, byte[] signData)
        {
            try
            {
                if (data.Length == 0)
                    return false;

                if (ReverseBytes)
                    Array.Reverse(signData);

                var verifySuccess = RsaService.VerifyHash(
                    data, signData, SignHashAlgorithm, SignPadding);

                return verifySuccess;
            }
            catch (ArgumentNullException ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                return false;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }

        public bool SetXmlPublicKey(string xmlPublicKey)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlPublicKey))
                    return false;

                RsaService.FromXmlString(xmlPublicKey);

                return true;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }

        public bool SetXmlPrivateKey(string xmlPrivateKey)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlPrivateKey))
                    return false;

                RsaService.FromXmlString(xmlPrivateKey);

                return true;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }

        public int ImportPublicKey(ReadOnlySpan<byte> publicKey)
        {
            try
            {
                if (publicKey.Length == 0)
                    return 0;

                RsaService.ImportRSAPublicKey(
                    publicKey, out var bytesRead);

                return bytesRead;
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }

        public int ImportPrivateKey(ReadOnlySpan<byte> privateKey)
        {
            try
            {
                if (privateKey.Length == 0)
                    return 0;

                RsaService.ImportRSAPrivateKey(
                    privateKey, out var bytesRead);

                return bytesRead;
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
