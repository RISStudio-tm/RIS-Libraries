// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Linq;
using System.Security.Cryptography;

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
                                $"BlockSize[{value}] not supported for CipherMethod[{GetType().FullName}]");
                            Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                            OnError(new RErrorEventArgs(exception, exception.Message));
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

        public Rijndael()
            : this(RijndaelKeySize.L256Bit)
        {

        }
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
                                $"KeySize[{keySize}] not supported for CipherMethod[{GetType().FullName}]");
                            Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                            OnError(new RErrorEventArgs(exception, exception.Message));
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
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));

                var exception = new Exception($"CipherMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));

                throw;
            }
        }
        public Rijndael(RijndaelKeySize keySize)
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
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));

                var exception = new Exception($"CipherMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));

                throw;
            }
        }
        public Rijndael(string key, int keySize)
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
                                $"KeySize[{keySize}] not supported for CipherMethod[{GetType().FullName}]");
                            Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                            OnError(new RErrorEventArgs(exception, exception.Message));
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

                try
                {
                    RijndaelService.Key = Convert.FromBase64String(key);
                }
                catch (FormatException)
                {
                    RijndaelService.Key = Convert.FromBase64String(Convert.ToBase64String(Utils.GetBytes(key)));
                }

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

                try
                {
                    RijndaelService.Key = Convert.FromBase64String(key);
                }
                catch (FormatException)
                {
                    RijndaelService.Key = Convert.FromBase64String(Convert.ToBase64String(Utils.GetBytes(key)));
                }

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
        public Rijndael(string key, int keySize, string iv)
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
                                $"KeySize[{keySize}] not supported for CipherMethod[{GetType().FullName}]");
                            Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                            OnError(new RErrorEventArgs(exception, exception.Message));
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

                try
                {
                    RijndaelService.Key = Convert.FromBase64String(key);
                }
                catch (FormatException)
                {
                    RijndaelService.Key = Convert.FromBase64String(Convert.ToBase64String(Utils.GetBytes(key)));
                }

                try
                {
                    RijndaelService.IV = Convert.FromBase64String(iv);
                }
                catch (FormatException)
                {
                    RijndaelService.IV = Convert.FromBase64String(Convert.ToBase64String(Utils.GetBytes(iv)));
                }

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
        public Rijndael(string key, RijndaelKeySize keySize, string iv)
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

                try
                {
                    RijndaelService.Key = Convert.FromBase64String(key);
                }
                catch (FormatException)
                {
                    RijndaelService.Key = Convert.FromBase64String(Convert.ToBase64String(Utils.GetBytes(key)));
                }

                try
                {
                    RijndaelService.IV = Convert.FromBase64String(iv);
                }
                catch (FormatException)
                {
                    RijndaelService.IV = Convert.FromBase64String(Convert.ToBase64String(Utils.GetBytes(iv)));
                }

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
                byte[] encryptedData;
                byte[] iv;

                if (data.Length == 0)
                    return Array.Empty<byte>();

                iv = RijndaelService.IV;

                if (GenIVAfterEncrypt)
                    RijndaelService.GenerateIV();

                ICryptoTransform transform = RijndaelService.CreateEncryptor(RijndaelService.Key, iv);

                encryptedData = transform.TransformFinalBlock(data, 0, data.Length);

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
            byte[] data = Convert.FromBase64String(cipherText);

            return Utils.GetString(
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
                byte[] decryptedData;

                if (dataWithIV.Length == 0)
                    return Array.Empty<byte>();

                int ivLength = RijndaelService.IV.Length;
                byte[] iv = dataWithIV
                    .Take(ivLength)
                    .ToArray();
                byte[] data = dataWithIV
                    .Skip(ivLength)
                    .ToArray();

                ICryptoTransform transform = RijndaelService.CreateDecryptor(RijndaelService.Key, iv);

                decryptedData = transform.TransformFinalBlock(data, 0, data.Length);

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
