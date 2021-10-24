// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Linq;
using System.Security.Cryptography;
using RIS.Text.Encoding.Base;

namespace RIS.Cryptography.Cipher.Methods
{
    public sealed class Rijndael : ICipherMethod
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        private RijndaelManaged RijndaelService { get; }

        public string Key
        {
            get
            {
                return Convert.ToBase64String(RijndaelService.Key);
            }
            set
            {
                if (Base64.IsBase64(value))
                {
                    try
                    {
                        RijndaelService.Key = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        RijndaelService.Key = SecureUtils.GetBytes(value);
                    }
                }
                else
                {
                    RijndaelService.Key = SecureUtils.GetBytes(value);
                }
            }
        }
        public byte[] KeyBytes
        {
            get
            {
                return RijndaelService.Key;
            }
            set
            {
                RijndaelService.Key = value;
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
                if (Base64.IsBase64(value))
                {
                    try
                    {
                        RijndaelService.IV = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        RijndaelService.IV = SecureUtils.GetBytes(value);
                    }
                }
                else
                {
                    RijndaelService.IV = SecureUtils.GetBytes(value);
                }
            }
        }
        public byte[] IVBytes
        {
            get
            {
                return RijndaelService.IV;
            }
            set
            {
                RijndaelService.IV = value;
            }
        }
        public RijndaelKeySize KeySize
        {
            get
            {
                return (RijndaelKeySize)RijndaelService.KeySize;
            }
            set
            {
                RijndaelService.KeySize = (int)value;
            }
        }
        public RijndaelBlockSize BlockSize
        {
            get
            {
                return (RijndaelBlockSize)RijndaelService.BlockSize;
            }
            set
            {
                RijndaelService.BlockSize = (int)value;
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

        public Rijndael()
            : this(RijndaelKeySize.L256Bit)
        {

        }
        public Rijndael(RijndaelKeySize keySize)
        {
            try
            {
                RijndaelService = new RijndaelManaged
                {
                    BlockSize = (int)RijndaelBlockSize.L128Bit,
                    Padding = PaddingMode.ISO10126,
                    Mode = CipherMode.CBC,
                    KeySize = (int)keySize
                };

                RijndaelService.GenerateKey();
                RijndaelService.GenerateIV();

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
        public Rijndael(string key, RijndaelKeySize keySize)
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
        public Rijndael(byte[] key, RijndaelKeySize keySize)
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

                var iv = RijndaelService.IV;

                if (GenIVAfterEncrypt)
                    RijndaelService.GenerateIV();

                var transform = RijndaelService
                    .CreateEncryptor(RijndaelService.Key, iv);
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
        private byte[] DecryptWithReadIV(byte[] dataWithIV)
        {
            try
            {
                if (dataWithIV.Length == 0)
                    return Array.Empty<byte>();

                var ivLength = RijndaelService.IV.Length;
                var iv = dataWithIV
                    .Take(ivLength)
                    .ToArray();
                var data = dataWithIV
                    .Skip(ivLength)
                    .ToArray();
                var transform = RijndaelService
                    .CreateDecryptor(RijndaelService.Key, iv);
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
