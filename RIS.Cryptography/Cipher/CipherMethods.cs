﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace RIS.Cryptography.Cipher
{
    public static class CipherMethods
    {
        public static event EventHandler<RInformationEventArgs> Information;
        public static event EventHandler<RWarningEventArgs> Warning;
        public static event EventHandler<RErrorEventArgs> Error;

        private static ReadOnlyCollection<Type> CipherMethodsTypes { get; }
        private static ReadOnlyCollection<string> CipherMethodsNames { get; }
        private static int CipherMethodsCount { get; }

        public static Encoding TextEncoding { get; }

        static CipherMethods()
        {
            TextEncoding = Utils.SecureUTF8;

            Type cipherMethodsType = typeof(CipherMethods);
            MemberInfo[] cipherMethods =
                cipherMethodsType.FindMembers(MemberTypes.NestedType, BindingFlags.Public,
                    (info, _) =>
                        typeof(CipherMethods).GetNestedType(info.Name)?.IsClass != false &&
                        typeof(ICipherMethod).IsAssignableFrom(typeof(CipherMethods).GetNestedType(info.Name)),
                    "IsClass && IsAssignableFrom");

            CipherMethodsCount = cipherMethods.Length;

            Type[] cipherMethodsTypes = new Type[cipherMethods.Length];
            for (int i = 0; i < cipherMethodsTypes.Length; ++i)
            {
                cipherMethodsTypes[i] = cipherMethods[i].GetType();
            }

            CipherMethodsTypes = new ReadOnlyCollection<Type>(cipherMethodsTypes);

            string[] cipherMethodsNames = new string[cipherMethods.Length];
            for (int i = 0; i < cipherMethodsNames.Length; ++i)
            {
                cipherMethodsNames[i] = cipherMethods[i].Name;
            }

            CipherMethodsNames = new ReadOnlyCollection<string>(cipherMethodsNames);
        }

        public static void OnInformation(RInformationEventArgs e)
        {
            OnInformation(null, e);
        }
        public static void OnInformation(object sender, RInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        public static void OnWarning(RWarningEventArgs e)
        {
            OnWarning(null, e);
        }
        public static void OnWarning(object sender, RWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        public static void OnError(RErrorEventArgs e)
        {
            OnError(null, e);
        }
        public static void OnError(object sender, RErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }

        public static ReadOnlyCollection<Type> GetTypes()
        {
            return CipherMethodsTypes;
        }

        public static ReadOnlyCollection<string> GetNames()
        {
            return CipherMethodsNames;
        }

        public static int GetCount()
        {
            return CipherMethodsCount;
        }

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
                                    $"KeySize[{publicKeySize}] not supported for CipherMethod[{this.GetType().FullName}]");
                                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

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
                                    $"KeySize[{publicKeySize}] not supported for CipherMethod[{this.GetType().FullName}]");
                                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
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
                                    $"KeySize[{privateKeySize}] not supported for CipherMethod[{this.GetType().FullName}]");
                                if (Error == null)
                                {
                                    throw exception;
                                }
                                else
                                {
                                    Error.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

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
                                        $"KeySize[{publicKeySize}] not supported for CipherMethod[{this.GetType().FullName}]");
                                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

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
                                        $"KeySize[{publicKeySize}] not supported for CipherMethod[{this.GetType().FullName}]");
                                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
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
                                        $"KeySize[{privateKeySize}] not supported for CipherMethod[{this.GetType().FullName}]");
                                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

                    throw;
                }
            }

            public string Encrypt(string plainText)
            {
                try
                {
                    byte[] encryptedData;

                    if (plainText.Length == 0)
                        return string.Empty;

                    byte[] data = TextEncoding.GetBytes(plainText);

                    encryptedData = RSAServiceEncryptor.Encrypt(data, Padding);

                    if (ReverseBytes)
                        Array.Reverse(encryptedData);

                    string cipherText = Convert.ToBase64String(encryptedData);
                    return cipherText;
                }
                catch (ArgumentNullException ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
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
                            $"CipherMethod[{ this.GetType().FullName }] not contained PrivateKey, which is needed for decryption");
                        Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                        OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                        throw exception;
                    }

                    if (cipherText.Length == 0)
                        return string.Empty;

                    byte[] data = Convert.FromBase64String(cipherText);

                    if (ReverseBytes)
                        Array.Reverse(data);

                    decryptedData = RSAServiceDecryptor.Decrypt(data, Padding);

                    string plainText = TextEncoding.GetString(decryptedData);
                    return plainText;
                }
                catch (ArgumentNullException ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                    throw;
                }
            }
        }
        public sealed class RSAiCNG : ICipherMethod
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
                                    $"KeySize[{publicKeySize}] not supported for CipherMethod[{this.GetType().FullName}]");
                                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

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
                                    $"KeySize[{publicKeySize}] not supported for CipherMethod[{this.GetType().FullName}]");
                                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
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
                                    $"KeySize[{privateKeySize}] not supported for CipherMethod[{this.GetType().FullName}]");
                                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

                    throw;
                }
            }
            public RSAiCNG(CipherKeySizes publicKeySize)
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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

                    throw;
                }
            }
            public RSAiCNG(CipherKeySizes publicKeySize, CipherKeySizes privateKeySize)
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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

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
                                        $"KeySize[{publicKeySize}] not supported for CipherMethod[{this.GetType().FullName}]");
                                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

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
                                        $"KeySize[{publicKeySize}] not supported for CipherMethod[{this.GetType().FullName}]");
                                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
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
                                        $"KeySize[{privateKeySize}] not supported for CipherMethod[{this.GetType().FullName}]");
                                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

                    throw;
                }
            }
            public RSAiCNG(string xmlPublicKey, CipherKeySizes publicKeySize)
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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

                    throw;
                }
            }
            public RSAiCNG(string xmlPublicKey, string xmlPrivateKey, CipherKeySizes publicKeySize, CipherKeySizes privateKeySize)
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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

                    throw;
                }
            }

            public string Encrypt(string plainText)
            {
                try
                {
                    byte[] encryptedData;

                    if (plainText.Length == 0)
                        return string.Empty;

                    byte[] data = TextEncoding.GetBytes(plainText);

                    encryptedData = RSAServiceEncryptor.Encrypt(data, Padding);

                    if (ReverseBytes)
                        Array.Reverse(encryptedData);

                    string cipherText = Convert.ToBase64String(encryptedData);
                    return cipherText;
                }
                catch (ArgumentNullException ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
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
                            $"CipherMethod[{ this.GetType().FullName }] not contained PrivateKey, which is needed for decryption");
                        Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                        OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                        throw exception;
                    }

                    if (cipherText.Length == 0)
                        return string.Empty;

                    byte[] data = Convert.FromBase64String(cipherText);

                    if (ReverseBytes)
                        Array.Reverse(data);

                    decryptedData = RSAServiceDecryptor.Decrypt(data, Padding);

                    string plainText = TextEncoding.GetString(decryptedData);
                    return plainText;
                }
                catch (ArgumentNullException ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
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
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                    throw;
                }
            }
        }
        public sealed class Rijndael : ICipherMethod
        {
            public event EventHandler<RInformationEventArgs> Information;
            public event EventHandler<RWarningEventArgs> Warning;
            public event EventHandler<RErrorEventArgs> Error;

            public enum CipherKeySizes
            {
                L128Bit = 128,
                L192Bit = 192,
                L256Bit = 256
            }

            private RijndaelManaged RijndaelService { get; }

            public string Key
            {
                get
                {
                    return Convert.ToBase64String(RijndaelService.Key);
                }
            }
            public string IV
            {
                get
                {
                    return Convert.ToBase64String(RijndaelService.IV);
                }
                set
                {
                    RijndaelService.IV = Convert.FromBase64String(value);
                }
            }
            public int KeySize
            {
                get
                {
                    return RijndaelService.KeySize;
                }
            }
            public int BlockSize
            {
                get
                {
                    return RijndaelService.BlockSize;
                }
                set
                {
                    KeySizes blockSizes = RijndaelService.LegalBlockSizes[0];
                    int countSizes = ((blockSizes.MaxSize - blockSizes.MinSize) / blockSizes.SkipSize) + 1;
                    int size = blockSizes.MinSize;

                    for (int i = 0; i < countSizes; ++i)
                    {
                        if (size == value)
                        {
                            RijndaelService.BlockSize = value;
                            break;
                        }

                        if (i != countSizes - 1)
                        {
                            size += blockSizes.SkipSize;
                        }
                        else
                        {
                            if (RijndaelService.BlockSize != value)
                            {
                                var exception = new Exception(
                                    $"BlockSize[{value}] not supported for CipherMethod[{this.GetType().FullName}]");
                                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                                throw exception;
                            }
                        }
                    }
                }
            }
            public PaddingMode Padding
            {
                get
                {
                    return RijndaelService.Padding;
                }
                set
                {
                    RijndaelService.Padding = value;
                }
            }
            public CipherMode Mode
            {
                get
                {
                    return RijndaelService.Mode;
                }
                set
                {
                    RijndaelService.Mode = value;
                }
            }
            public bool GenIVAfterEncrypt { get; set; }

            public bool Initialized { get; }

            public Rijndael(int keySize)
            {
                try
                {
                    RijndaelManaged rijndaelService = new RijndaelManaged();
                    KeySizes keySizes = rijndaelService.LegalKeySizes[0];
                    int countSizes = ((keySizes.MaxSize - keySizes.MinSize) / keySizes.SkipSize) + 1;
                    int size = keySizes.MinSize;

                    for (int i = 0; i < countSizes; ++i)
                    {
                        if (size == keySize)
                        {
                            rijndaelService.KeySize = keySize;
                            break;
                        }

                        if (i != countSizes - 1)
                        {
                            size += keySizes.SkipSize;
                        }
                        else
                        {
                            if (rijndaelService.KeySize != keySize)
                            {
                                var exception = new Exception(
                                    $"KeySize[{keySize}] not supported for CipherMethod[{this.GetType().FullName}]");
                                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                                throw exception;
                            }
                        }
                    }

                    rijndaelService.Dispose();

                    RijndaelService = new RijndaelManaged
                    {
                        BlockSize = 128,
                        Padding = PaddingMode.ISO10126,
                        Mode = CipherMode.CBC,
                        KeySize = keySize
                    };

                    GenIVAfterEncrypt = true;
                    Initialized = true;
                }
                catch (Exception ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

                    throw;
                }
            }
            public Rijndael(CipherKeySizes keySize)
            {
                try
                {
                    RijndaelService = new RijndaelManaged
                    {
                        BlockSize = 128,
                        Padding = PaddingMode.ISO10126,
                        Mode = CipherMode.CBC,
                        KeySize = (int)keySize
                    };

                    GenIVAfterEncrypt = true;
                    Initialized = true;
                }
                catch (Exception ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

                    throw;
                }
            }
            public Rijndael(string key, bool keyInBase64, int keySize)
            {
                try
                {
                    if (string.IsNullOrEmpty(key))
                        return;

                    RijndaelManaged rijndaelService = new RijndaelManaged();
                    KeySizes keySizes = rijndaelService.LegalKeySizes[0];
                    int countSizes = ((keySizes.MaxSize - keySizes.MinSize) / keySizes.SkipSize) + 1;
                    int size = keySizes.MinSize;

                    for (int i = 0; i < countSizes; ++i)
                    {
                        if (size == keySize)
                        {
                            rijndaelService.KeySize = keySize;
                            break;
                        }

                        if (i != countSizes - 1)
                        {
                            size += keySizes.SkipSize;
                        }
                        else
                        {
                            if (rijndaelService.KeySize != keySize)
                            {
                                var exception = new Exception(
                                    $"KeySize[{keySize}] not supported for CipherMethod[{this.GetType().FullName}]");
                                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                                throw exception;
                            }
                        }
                    }

                    rijndaelService.Dispose();

                    RijndaelService = new RijndaelManaged
                    {
                        BlockSize = 128,
                        Padding = PaddingMode.ISO10126,
                        Mode = CipherMode.CBC,
                        KeySize = keySize
                    };

                    if (keyInBase64)
                    {
                        RijndaelService.Key = Convert.FromBase64String(key);
                    }
                    else
                    {
                        RijndaelService.Key = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(key)));
                    }

                    GenIVAfterEncrypt = true;
                    Initialized = true;
                }
                catch (Exception ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

                    throw;
                }
            }
            public Rijndael(string key, bool keyInBase64, CipherKeySizes keySize)
            {
                try
                {
                    if (string.IsNullOrEmpty(key))
                        return;

                    RijndaelService = new RijndaelManaged
                    {
                        BlockSize = 128,
                        Padding = PaddingMode.ISO10126,
                        Mode = CipherMode.CBC,
                        KeySize = (int)keySize
                    };

                    if (keyInBase64)
                    {
                        RijndaelService.Key = Convert.FromBase64String(key);
                    }
                    else
                    {
                        RijndaelService.Key = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(key)));
                    }

                    GenIVAfterEncrypt = true;
                    Initialized = true;
                }
                catch (Exception ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

                    throw;
                }
            }
            public Rijndael(string key, bool keyInBase64, int keySize, string iv, bool ivInBase64)
            {
                try
                {
                    if (string.IsNullOrEmpty(key))
                        return;

                    RijndaelManaged rijndaelService = new RijndaelManaged();
                    KeySizes keySizes = rijndaelService.LegalKeySizes[0];
                    int countSizes = ((keySizes.MaxSize - keySizes.MinSize) / keySizes.SkipSize) + 1;
                    int size = keySizes.MinSize;

                    for (int i = 0; i < countSizes; ++i)
                    {
                        if (size == keySize)
                        {
                            rijndaelService.KeySize = keySize;
                            break;
                        }

                        if (i != countSizes - 1)
                        {
                            size += keySizes.SkipSize;
                        }
                        else
                        {
                            if (rijndaelService.KeySize != keySize)
                            {
                                var exception = new Exception(
                                    $"KeySize[{keySize}] not supported for CipherMethod[{this.GetType().FullName}]");
                                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                                throw exception;
                            }
                        }
                    }

                    rijndaelService.Dispose();

                    RijndaelService = new RijndaelManaged
                    {
                        BlockSize = 128,
                        Padding = PaddingMode.ISO10126,
                        Mode = CipherMode.CBC,
                        KeySize = keySize
                    };

                    if (keyInBase64)
                    {
                        RijndaelService.Key = Convert.FromBase64String(key);
                    }
                    else
                    {
                        RijndaelService.Key = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(key)));
                    }

                    if (ivInBase64)
                    {
                        RijndaelService.IV = Convert.FromBase64String(iv);
                    }
                    else
                    {
                        RijndaelService.IV = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(iv)));
                    }

                    GenIVAfterEncrypt = true;
                    Initialized = true;
                }
                catch (Exception ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

                    throw;
                }
            }
            public Rijndael(string key, bool keyInBase64, CipherKeySizes keySize, string iv, bool ivInBase64)
            {
                try
                {
                    if (string.IsNullOrEmpty(key))
                        return;

                    RijndaelService = new RijndaelManaged
                    {
                        BlockSize = 128,
                        Padding = PaddingMode.ISO10126,
                        Mode = CipherMode.CBC,
                        KeySize = (int)keySize
                    };

                    if (keyInBase64)
                    {
                        RijndaelService.Key = Convert.FromBase64String(key);
                    }
                    else
                    {
                        RijndaelService.Key = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(key)));
                    }

                    if (ivInBase64)
                    {
                        RijndaelService.IV = Convert.FromBase64String(iv);
                    }
                    else
                    {
                        RijndaelService.IV = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(iv)));
                    }

                    GenIVAfterEncrypt = true;
                    Initialized = true;
                }
                catch (Exception ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));

                    var exception = new Exception($"CipherMethod[{ this.GetType().FullName }] is not initialized");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));

                    throw;
                }
            }

            public string Encrypt(string plainText)
            {
                return EncryptWithWriteIV(plainText);
            }
            private string EncryptWithWriteIV(string plainText)
            {
                try
                {
                    byte[] encryptedData;
                    byte[] iv;

                    if (plainText.Length == 0)
                        return string.Empty;

                    byte[] data = TextEncoding.GetBytes(plainText);

                    iv = RijndaelService.IV;
                    ICryptoTransform transform = RijndaelService.CreateEncryptor(RijndaelService.Key, iv);

                    encryptedData = transform.TransformFinalBlock(data, 0, data.Length);

                    if (GenIVAfterEncrypt)
                    {
                        RijndaelService.GenerateIV();
                    }

                    string cipherTextWithIV = Convert.ToBase64String(iv) + Convert.ToBase64String(encryptedData);
                    return cipherTextWithIV;
                }
                catch (ArgumentNullException ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                    throw;
                }
            }

            public string Decrypt(string cipherText)
            {
                return DecryptWithReadIV(cipherText);
            }
            private string DecryptWithReadIV(string cipherTextWithIV)
            {
                try
                {
                    byte[] decryptedData;

                    if (cipherTextWithIV.Length == 0)
                        return string.Empty;

                    int ivLength = Convert.ToBase64String(RijndaelService.IV).Length;
                    byte[] iv = Convert.FromBase64String(cipherTextWithIV.Substring(0, ivLength));
                    string cipherText = cipherTextWithIV.Substring(ivLength);
                    byte[] data = Convert.FromBase64String(cipherText);

                    ICryptoTransform transform = RijndaelService.CreateDecryptor(RijndaelService.Key, iv);

                    decryptedData = transform.TransformFinalBlock(data, 0, data.Length);

                    return TextEncoding.GetString(decryptedData);
                }
                catch (ArgumentNullException ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                    OnError(new RErrorEventArgs(ex.Message, ex.StackTrace));
                    throw;
                }
            }
        }
    }
}
