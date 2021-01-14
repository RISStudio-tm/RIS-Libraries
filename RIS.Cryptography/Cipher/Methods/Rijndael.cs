// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Security.Cryptography;

namespace RIS.Cryptography.Cipher.Methods
{
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
                                $"BlockSize[{value}] not supported for CipherMethod[{GetType().FullName}]");
                            Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                            OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
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
                                $"KeySize[{keySize}] not supported for CipherMethod[{GetType().FullName}]");
                            Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                            OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
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
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));

                var exception = new Exception($"CipherMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));

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
                Events.OnError(this, new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));

                var exception = new Exception($"CipherMethod[{ GetType().FullName }] is not initialized");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));

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
                                $"KeySize[{keySize}] not supported for CipherMethod[{GetType().FullName}]");
                            Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                            OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
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
                    RijndaelService.Key = Convert.FromBase64String(Convert.ToBase64String(Utils.SecureUTF8.GetBytes(key)));
                }

                GenIVAfterEncrypt = true;
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
                    RijndaelService.Key = Convert.FromBase64String(Convert.ToBase64String(Utils.SecureUTF8.GetBytes(key)));
                }

                GenIVAfterEncrypt = true;
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
                                $"KeySize[{keySize}] not supported for CipherMethod[{GetType().FullName}]");
                            Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                            OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
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
                    RijndaelService.Key = Convert.FromBase64String(Convert.ToBase64String(Utils.SecureUTF8.GetBytes(key)));
                }

                if (ivInBase64)
                {
                    RijndaelService.IV = Convert.FromBase64String(iv);
                }
                else
                {
                    RijndaelService.IV = Convert.FromBase64String(Convert.ToBase64String(Utils.SecureUTF8.GetBytes(iv)));
                }

                GenIVAfterEncrypt = true;
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
                    RijndaelService.Key = Convert.FromBase64String(Convert.ToBase64String(Utils.SecureUTF8.GetBytes(key)));
                }

                if (ivInBase64)
                {
                    RijndaelService.IV = Convert.FromBase64String(iv);
                }
                else
                {
                    RijndaelService.IV = Convert.FromBase64String(Convert.ToBase64String(Utils.SecureUTF8.GetBytes(iv)));
                }

                GenIVAfterEncrypt = true;
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

                byte[] data = Utils.SecureUTF8.GetBytes(plainText);

                iv = RijndaelService.IV;
                ICryptoTransform transform = RijndaelService.CreateEncryptor(RijndaelService.Key, iv);

                encryptedData = transform.TransformFinalBlock(data, 0, data.Length);

                if (GenIVAfterEncrypt)
                {
                    RijndaelService.GenerateIV();
                }

                return Convert.ToBase64String(iv) + Convert.ToBase64String(encryptedData);
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
    }
}
