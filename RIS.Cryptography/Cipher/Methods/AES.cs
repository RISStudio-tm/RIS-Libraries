// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using RIS.Text.Encoding.Base;

namespace RIS.Cryptography.Cipher.Methods
{
    public sealed class AES : ICipherMethod
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        private Aes AesService { get; }

        public string Key
        {
            get
            {
                return Convert.ToBase64String(AesService.Key);
            }
            set
            {
                if (Base64.IsBase64(value))
                {
                    try
                    {
                        AesService.Key = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        AesService.Key = SecureUtils.GetBytes(value);
                    }
                }
                else
                {
                    AesService.Key = SecureUtils.GetBytes(value);
                }
            }
        }
        public byte[] KeyBytes
        {
            get
            {
                return AesService.Key;
            }
            set
            {
                AesService.Key = value;
            }
        }
        public string IV
        {
            get
            {
                return Convert.ToBase64String(AesService.IV);
            }
            set
            {
                if (Base64.IsBase64(value))
                {
                    try
                    {
                        AesService.IV = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        AesService.IV = SecureUtils.GetBytes(value);
                    }
                }
                else
                {
                    AesService.IV = SecureUtils.GetBytes(value);
                }
            }
        }
        public byte[] IVBytes
        {
            get
            {
                return AesService.IV;
            }
            set
            {
                AesService.IV = value;
            }
        }
        public RijndaelKeySize KeySize
        {
            get
            {
                return (RijndaelKeySize)AesService.KeySize;
            }
            set
            {
                AesService.KeySize = (int)value;
            }
        }
        public RijndaelBlockSize BlockSize
        {
            get
            {
                return (RijndaelBlockSize)AesService.BlockSize;
            }
            set
            {
                AesService.BlockSize = (int)value;
            }
        }
        public PaddingMode Padding
        {
            get
            {
                return AesService.Padding;
            }
            set
            {
                AesService.Padding = value;
            }
        }
        public CipherMode Mode
        {
            get
            {
                return AesService.Mode;
            }
            set
            {
                AesService.Mode = value;
            }
        }
        public bool GenIVAfterEncrypt { get; set; }

        public bool Initialized { get; }

        public AES()
            : this(RijndaelKeySize.L256Bit)
        {

        }
        public AES(RijndaelKeySize keySize)
        {
            try
            {
                AesService = Aes.Create();

                AesService.BlockSize = (int)RijndaelBlockSize.L256Bit;
                AesService.Padding = PaddingMode.ISO10126;
                AesService.Mode = CipherMode.CBC;
                AesService.KeySize = (int)keySize;

                AesService.GenerateKey();
                AesService.GenerateIV();

                GenIVAfterEncrypt = true;

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
        public AES(string key, RijndaelKeySize keySize)
            : this(keySize)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return;

                Key = key;
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
        public AES(byte[] key, RijndaelKeySize keySize)
            : this(keySize)
        {
            try
            {
                if (key == null || key.Length == 0)
                    return;

                KeyBytes = key;
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
                EncryptWithWriteIV(data));
        }
        public byte[] Encrypt(byte[] data)
        {
            return EncryptWithWriteIV(data);
        }
        private byte[] EncryptWithWriteIV(byte[] data)
        {
            try
            {
                if (data.Length == 0)
                    return Array.Empty<byte>();

                var iv = AesService.IV;

                if (GenIVAfterEncrypt)
                    AesService.GenerateIV();

                var transform = AesService
                    .CreateEncryptor(AesService.Key, iv);
                var encryptedData = transform
                    .TransformFinalBlock(data, 0, data.Length);

                return iv
                    .Concat(encryptedData)
                    .ToArray();
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
                DecryptWithReadIV(data));
        }
        public byte[] Decrypt(byte[] data)
        {
            return DecryptWithReadIV(data);
        }
        private byte[] DecryptWithReadIV(IReadOnlyCollection<byte> dataWithIV)
        {
            try
            {
                if (dataWithIV.Count == 0)
                    return Array.Empty<byte>();

                var ivLength = AesService.IV.Length;
                var iv = dataWithIV
                    .Take(ivLength)
                    .ToArray();
                var data = dataWithIV
                    .Skip(ivLength)
                    .ToArray();
                var transform = AesService
                    .CreateDecryptor(AesService.Key, iv);
                var decryptedData = transform
                    .TransformFinalBlock(data, 0, data.Length);

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
    }
}
