using System;
using System.Globalization;
using System.Reflection;
#if NETCOREAPP
using System.Runtime.Intrinsics.X86;
#endif
using System.Security.Cryptography;
using System.Text;
using RIS.Cryptography.Hash;
using RIS.Text.Encoding.Base;

namespace RIS.Cryptography.Hash
{
    public static class HashMethods
    {
        public static event EventHandler<RMessageEventArgs> ShowMessage;
        public static event EventHandler<RErrorEventArgs> ShowError;

        private static string[] HashMethodsNames { get; }
        private static int HashMethodsCount { get; }
        private static RNGCryptoServiceProvider RNGProvider { get; }

        public static Encoding TextEncoding { get; }

        static HashMethods()
        {
            TextEncoding = Utils.SecureUTF8;
            RNGProvider = new RNGCryptoServiceProvider();
            
            Type hashMethodsType = typeof(HashMethods);
            MemberInfo[] hashMethods =
                hashMethodsType.FindMembers(MemberTypes.NestedType, BindingFlags.Public,
                    (info, criteria) =>
                        typeof(HashMethods).GetNestedType(info.Name).IsClass &&
                        typeof(IHashMethod).IsAssignableFrom(typeof(HashMethods).GetNestedType(info.Name)),
                    "IsClass && IsAssignableFrom");

            HashMethodsCount = hashMethods.Length;

            HashMethodsNames = new string[hashMethods.Length];
            for (int i = 0; i < HashMethodsNames.Length; ++i)
            {
                HashMethodsNames[i] = hashMethods[i].Name;
            }
        }

        public static string[] GetNamesHashMethods()
        {
            return HashMethodsNames;
        }

        public static int GetCountHashMethods()
        {
            return HashMethodsCount;
        }

        public static string GenSalt(ushort length)
        {
            if (length < 1)
            {
                var exception = new ArgumentOutOfRangeException(nameof(length), "Salt length cannot be less than 1");
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            byte[] salt = new byte[length];
            RNGProvider.GetBytes(salt);

            return Convert.ToBase64String(salt);
        }

#if NETFRAMEWORK

        public sealed class SHA1iCNG : IHashMethod
        {
            private SHA1Cng SHAService { get; }

            public bool Initialized { get; }

            public SHA1iCNG()
            {
                SHAService = new SHA1Cng();
                SHAService.Initialize();

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);
                byte[] hashBytes = SHAService.ComputeHash(data);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                return hashText.ToString();
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                string plainTextHash = GetHash(plainText);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class SHA256iCNG : IHashMethod
        {
            private SHA256Cng SHAService { get; }

            public bool Initialized { get; }

            public SHA256iCNG()
            {
                SHAService = new SHA256Cng();
                SHAService.Initialize();

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);
                byte[] hashBytes = SHAService.ComputeHash(data);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                return hashText.ToString();
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                var plainTextHash = GetHash(plainText);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class SHA384iCNG : IHashMethod
        {
            private SHA384Cng SHAService { get; }

            public bool Initialized { get; }

            public SHA384iCNG()
            {
                SHAService = new SHA384Cng();
                SHAService.Initialize();

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);
                byte[] hashBytes = SHAService.ComputeHash(data);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                return hashText.ToString();
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                var plainTextHash = GetHash(plainText);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class SHA512iCNG : IHashMethod
        {
            private SHA512Cng SHAService { get; }

            public bool Initialized { get; }

            public SHA512iCNG()
            {
                SHAService = new SHA512Cng();
                SHAService.Initialize();

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);
                byte[] hashBytes = SHAService.ComputeHash(data);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                return hashText.ToString();
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                var plainTextHash = GetHash(plainText);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class MD5iCNG : IHashMethod
        {
            private MD5Cng MDService { get; }

            public bool Initialized { get; }

            public MD5iCNG()
            {
                MDService = new MD5Cng();
                MDService.Initialize();

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);
                byte[] hashBytes = MDService.ComputeHash(data);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                return hashText.ToString();
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                var plainTextHash = GetHash(plainText);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

#endif

        public sealed class SHA1iCSP : IHashMethod
        {
            private SHA1CryptoServiceProvider SHAService { get; }

            public bool Initialized { get; }

            public SHA1iCSP()
            {
                SHAService = new SHA1CryptoServiceProvider();
                SHAService.Initialize();

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);
                byte[] hashBytes = SHAService.ComputeHash(data);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                return hashText.ToString();
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                string plainTextHash = GetHash(plainText);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class SHA256iCSP : IHashMethod
        {
            private SHA256CryptoServiceProvider SHAService { get; }

            public bool Initialized { get; }

            public SHA256iCSP()
            {
                SHAService = new SHA256CryptoServiceProvider();
                SHAService.Initialize();

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);
                byte[] hashBytes = SHAService.ComputeHash(data);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                return hashText.ToString();
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                var plainTextHash = GetHash(plainText);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class SHA384iCSP : IHashMethod
        {
            private SHA384CryptoServiceProvider SHAService { get; }

            public bool Initialized { get; }

            public SHA384iCSP()
            {
                SHAService = new SHA384CryptoServiceProvider();
                SHAService.Initialize();

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);
                byte[] hashBytes = SHAService.ComputeHash(data);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {

                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                return hashText.ToString();
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                var plainTextHash = GetHash(plainText);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class SHA512iCSP : IHashMethod
        {
            private SHA512CryptoServiceProvider SHAService { get; }

            public bool Initialized { get; }

            public SHA512iCSP()
            {
                SHAService = new SHA512CryptoServiceProvider();
                SHAService.Initialize();

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);
                byte[] hashBytes = SHAService.ComputeHash(data);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                return hashText.ToString();
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                var plainTextHash = GetHash(plainText);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class MD5iCSP : IHashMethod
        {
            private MD5CryptoServiceProvider MDService { get; }

            public bool Initialized { get; }

            public MD5iCSP()
            {
                MDService = new MD5CryptoServiceProvider();
                MDService.Initialize();

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);
                byte[] hashBytes = MDService.ComputeHash(data);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                return hashText.ToString();
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                var plainTextHash = GetHash(plainText);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class RIPEMD160 : IHashMethod
        {
            private RIS.Cryptography.Hash.Algorithms.RIPEMD160Managed RIPEMDService { get; }
            
            public bool Initialized { get; }
            
            public RIPEMD160()
            {
                RIPEMDService = new RIS.Cryptography.Hash.Algorithms.RIPEMD160Managed();
                RIPEMDService.Initialize();

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);
                byte[] hashBytes = RIPEMDService.ComputeHash(data);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                return hashText.ToString();
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                var plainTextHash = GetHash(plainText);
                
                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class CRC32 : IHashMethod
        {
            private Algorithms.CRC32 CRCService { get; }

            public bool Initialized { get; }

            public CRC32()
            {
                CRCService = new Algorithms.CRC32();
                CRCService.Initialize();

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);
                uint hashValue = BitConverter.ToUInt32(CRCService.ComputeHash(data), 0);

                return hashValue.ToString("x2", CultureInfo.InvariantCulture);
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                var plainTextHash = GetHash(plainText);
                
                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class CRC32C : IHashMethod
        {
            private Algorithms.CRC32C CRCService { get; }

            public bool Initialized { get; }

            public CRC32C()
            {

#if NETCOREAPP
                if (!Sse42.X64.IsSupported && !Sse42.IsSupported)
                {
                    CRCService = new Algorithms.CRC32C();
                    CRCService.Initialize();
                }

#elif NETFRAMEWORK

                CRCService = new Algorithms.CRC32C();
                CRCService.Initialize();

#endif

                Initialized = true;
            }
            
            public string GetHash(string plainText)
            {
                
#if NETCOREAPP

                byte[] data = TextEncoding.GetBytes(plainText);
                ulong hashValue = 0xFFFFFFFF;

                if (Sse42.X64.IsSupported)
                {
                    //hashValue ^= 0xFFFFFFFF;

                    Span<byte> dataSpan = new Span<byte>(data);
                    int remainingCount = dataSpan.Length % 8;

                    for (int i = 0; i < dataSpan.Length - remainingCount; i += 8)
                        hashValue = Sse42.X64.Crc32(hashValue, BitConverter.ToUInt64(dataSpan.Slice(i, 8)));

                    if (remainingCount % 2 == 0)
                        for (int i = 0; i < remainingCount; i += 2)
                            hashValue = Sse42.Crc32((uint)hashValue,
                                BitConverter.ToUInt16(dataSpan.Slice(dataSpan.Length - remainingCount + i, 2)));
                    else
                        for (int i = 0; i < remainingCount; ++i)
                            hashValue = Sse42.Crc32((uint)hashValue,
                                dataSpan.Slice(dataSpan.Length - remainingCount + i, 1)[0]);

                    //if (remainingCount == 1)
                    //{
                    //    hashValue = Sse42.Crc32((uint)hashValue, dataSpan.Slice(dataSpan.Length - 1, 1)[0]);
                    //}
                    //else if (remainingCount == 2)
                    //{
                    //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt16(dataSpan.Slice(dataSpan.Length - 2, 2)));
                    //}
                    //else if (remainingCount == 3)
                    //{
                    //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt16(dataSpan.Slice(dataSpan.Length - 3, 2)));
                    //    hashValue = Sse42.Crc32((uint)hashValue, dataSpan.Slice(dataSpan.Length - 1, 1)[0]);
                    //}
                    //else if (remainingCount == 4)
                    //{
                    //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt32(dataSpan.Slice(dataSpan.Length - 4, 4)));
                    //}
                    //else if (remainingCount == 5)
                    //{
                    //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt32(dataSpan.Slice(dataSpan.Length - 5, 4)));
                    //    hashValue = Sse42.Crc32((uint)hashValue, dataSpan.Slice(dataSpan.Length - 1, 1)[0]);
                    //}
                    //else if (remainingCount == 6)
                    //{
                    //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt32(dataSpan.Slice(dataSpan.Length - 6, 4)));
                    //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt16(dataSpan.Slice(dataSpan.Length - 2, 2)));
                    //}
                    //else if (remainingCount == 7)
                    //{
                    //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt32(dataSpan.Slice(data.Length - 7, 4)));
                    //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt16(dataSpan.Slice(data.Length - 3, 2)));
                    //    hashValue = Sse42.Crc32((uint)hashValue, dataSpan.Slice(dataSpan.Length - 1, 1)[0]);
                    //}

                    hashValue ^= 0xFFFFFFFF;
                }
                else if (Sse42.IsSupported)
                {
                    //hashValue ^= 0xFFFFFFFF;

                    Span<byte> dataSpan = new Span<byte>(data);
                    int remainingCount = dataSpan.Length % 4;

                    for (int i = 0; i < dataSpan.Length - (data.Length % 4); i += 4)
                        hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt32(dataSpan.Slice(i, 4)));

                    if (remainingCount % 2 == 0)
                        for (int i = 0; i < remainingCount; i += 2)
                            hashValue = Sse42.Crc32((uint)hashValue,
                                BitConverter.ToUInt16(dataSpan.Slice(dataSpan.Length - remainingCount + i, 2)));
                    else
                        for (int i = 0; i < remainingCount; ++i)
                            hashValue = Sse42.Crc32((uint)hashValue,
                                dataSpan.Slice(dataSpan.Length - remainingCount + i, 1)[0]);

                    //if (remainingCount == 1)
                    //{
                    //    hashValue = Sse42.Crc32((uint)hashValue, dataSpan.Slice(dataSpan.Length - 1, 1)[0]);
                    //}
                    //else if (remainingCount == 2)
                    //{
                    //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt16(dataSpan.Slice(dataSpan.Length - 2, 2)));
                    //}
                    //else if (remainingCount == 3)
                    //{
                    //    hashValue = Sse42.Crc32((uint)hashValue, BitConverter.ToUInt16(dataSpan.Slice(dataSpan.Length - 3, 2)));
                    //    hashValue = Sse42.Crc32((uint)hashValue, dataSpan.Slice(dataSpan.Length - 1, 1)[0]);
                    //}

                    hashValue ^= 0xFFFFFFFF;
                }
                else
                {
                    hashValue = BitConverter.ToUInt32(CRCService.ComputeHash(data), 0);
                }

                return hashValue.ToString("x2", CultureInfo.InvariantCulture);

#elif NETFRAMEWORK

                byte[] data = TextEncoding.GetBytes(plainText);
                uint hashValue = BitConverter.ToUInt32(CRCService.ComputeHash(data), 0);
                
                return hashValue.ToString("x2", CultureInfo.InvariantCulture);

#endif

            }
            public bool VerifyHash(string plainText, string hashText)
            {
                var plainTextHash = GetHash(plainText);
                
                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class CRC32Q : IHashMethod
        {
            private Algorithms.CRC32Q CRCService { get; }

            public bool Initialized { get; }

            public CRC32Q()
            {
                CRCService = new Algorithms.CRC32Q();
                CRCService.Initialize();

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);
                uint hashValue = BitConverter.ToUInt32(CRCService.ComputeHash(data), 0);

                return hashValue.ToString("x2", CultureInfo.InvariantCulture);
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                var plainTextHash = GetHash(plainText);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class CRC32D : IHashMethod
        {
            private Algorithms.CRC32D CRCService { get; }

            public bool Initialized { get; }

            public CRC32D()
            {
                CRCService = new Algorithms.CRC32D();
                CRCService.Initialize();

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);
                uint hashValue = BitConverter.ToUInt32(CRCService.ComputeHash(data), 0);

                return hashValue.ToString("x2", CultureInfo.InvariantCulture);
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                var plainTextHash = GetHash(plainText);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class BCrypt : IHashMethod
        {
            private global::BCrypt.Net.HashType _hashMethod;
            public BCryptHashType HashMethod
            {
                get
                {
                    Enum.TryParse(_hashMethod.ToString(), true, out BCryptHashType hashMethod);
                    return hashMethod;
                }
                set
                {
                    Enum.TryParse(value.ToString(), true, out global::BCrypt.Net.HashType hashMethod);
                    _hashMethod = hashMethod;
                }
            }
            private global::BCrypt.Net.HashType HashMethodOriginal
            {
                get
                {
                    return _hashMethod;
                }
                set
                {
                    _hashMethod = value;
                }
            }
            public bool UseEnhancedAlgorithm { get; set; }
            private int _workFactor;
            public int WorkFactor
            {
                get
                {
                    return _workFactor;
                }
                set
                {
                    if (value < 4)
                        value = 4;
                    else if (value > 31)
                        value = 31;

                    _workFactor = value;
                }
            }

            public bool Initialized { get; }

            public BCrypt()
            {
                HashMethod = BCryptHashType.SHA512;
                UseEnhancedAlgorithm = true;
                WorkFactor = 14;

                Initialized = true;
            }

            public static BCryptMetadata GetMetadata(string hashText)
            {
                return new BCryptMetadata(hashText);
            }

            public string GetHash(string plainText)
            {
                string hashText;

                if (UseEnhancedAlgorithm)
                    hashText = global::BCrypt.Net.BCrypt.EnhancedHashPassword(plainText, HashMethodOriginal, WorkFactor);
                else
                    hashText = global::BCrypt.Net.BCrypt.HashPassword(plainText, global::BCrypt.Net.BCrypt.GenerateSalt(WorkFactor), false, HashMethodOriginal);

                return hashText;
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                if (UseEnhancedAlgorithm)
                    return global::BCrypt.Net.BCrypt.EnhancedVerify(plainText, hashText, HashMethodOriginal);
                else
                    return global::BCrypt.Net.BCrypt.Verify(plainText, hashText, false, HashMethodOriginal);
            }

            public bool VerifyAndUpdateHash(string plainText, string hashText, out bool isUpdated, out string newHashText)
            {
                return VerifyAndUpdateHash(plainText, hashText, WorkFactor, out isUpdated, out newHashText);
            }
            public bool VerifyAndUpdateHash(string plainText, string hashText, int newWorkFactor, out bool isUpdated, out string newHashText)
            {
                bool result;
                isUpdated = false;
                newHashText = hashText;

                if (UseEnhancedAlgorithm)
                    result = global::BCrypt.Net.BCrypt.EnhancedVerify(plainText, hashText, HashMethodOriginal);
                else
                    result = global::BCrypt.Net.BCrypt.Verify(plainText, hashText, false, HashMethodOriginal);

                if (!result)
                    return false;

                if (newWorkFactor < 4)
                    newWorkFactor = 4;
                else if (newWorkFactor > 31)
                    newWorkFactor = 31;

                BCryptMetadata metadata = GetMetadata(hashText);

                isUpdated = metadata.WorkFactor != newWorkFactor;

                if (isUpdated)
                    if (UseEnhancedAlgorithm)
                        newHashText = global::BCrypt.Net.BCrypt.EnhancedHashPassword(
                            plainText,
                            HashMethodOriginal,
                            newWorkFactor);
                    else
                        newHashText = global::BCrypt.Net.BCrypt.HashPassword(
                            plainText,
                            global::BCrypt.Net.BCrypt.GenerateSalt(newWorkFactor),
                            false,
                            HashMethodOriginal);
                else
                    newHashText = hashText;

                return true;
            }
        }

        public sealed class Argon2iRaw : IHashMethod
        {
            private byte[] _salt;
            public byte[] SaltBytes
            {
                get
                {
                    return _salt;
                }
                set
                {
                    if (value.Length < 8)
                        value = new byte[8];

                    _salt = value;
                }
            }
            public string Salt
            {
                get
                {
                    return Convert.ToBase64String(_salt);
                }
                set
                {
                    try
                    {
                        if (Convert.FromBase64String(value).Length < 8)
                            value = Convert.ToBase64String(new byte[8]);

                        _salt = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        if (Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value))).Length < 8)
                            value = Convert.ToBase64String(new byte[8]);

                        _salt = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            private ushort _hashLength;
            public ushort HashLength
            {
                get
                {
                    return _hashLength;
                }
                set
                {
                    if (value < 6)
                        value = 6;

                    _hashLength = value;
                }
            }
            private int _degreeOfParallelism;
            public int DegreeOfParallelism
            {
                get
                {
                    return _degreeOfParallelism;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _degreeOfParallelism = value;
                }
            }
            private int _iterations;
            public int Iterations
            {
                get
                {
                    return _iterations;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _iterations = value;
                }
            }
            private int _memorySize;
            public int MemorySize
            {
                get
                {
                    return _memorySize;
                }
                set
                {
                    if (value < 8)
                        value = 8;

                    _memorySize = value;
                }
            }
            private byte[] _associatedData;
            public byte[] AssociatedDataBytes
            {
                get
                {
                    return _associatedData;
                }
                set
                {
                    _associatedData = value;
                }
            }
            public string AssociatedData
            {
                get
                {
                    return Convert.ToBase64String(_associatedData);
                }
                set
                {
                    try
                    {
                        _associatedData = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _associatedData = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            private byte[] _knownSecret;
            public byte[] KnownSecretBytes
            {
                get
                {
                    return _knownSecret;
                }
                set
                {
                    _knownSecret = value;
                }
            }
            public string KnownSecret
            {
                get
                {
                    return Convert.ToBase64String(_knownSecret);
                }
                set
                {
                    try
                    {
                        _knownSecret = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _knownSecret = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            public bool FixedHashLength { get; set; }

            public bool Initialized { get; }

            public Argon2iRaw()
            {
                SaltBytes = new byte[8];
                HashLength = 6;
                DegreeOfParallelism = 2 * 2;
                Iterations = 4;
                MemorySize = (1 * 1024) * 128;
                AssociatedDataBytes = Array.Empty<byte>();
                KnownSecretBytes = Array.Empty<byte>();

                FixedHashLength = true;

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);

                Konscious.Security.Cryptography.Argon2i argon2Service = new Konscious.Security.Cryptography.Argon2i(data)
                {
                    Salt = SaltBytes,
                    DegreeOfParallelism = DegreeOfParallelism,
                    Iterations = Iterations,
                    MemorySize = MemorySize,
                    AssociatedData = AssociatedDataBytes,
                    KnownSecret = KnownSecretBytes
                };

                byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : SaltBytes.Length);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                return hashText.ToString();
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                var plainTextHash = GetHash(plainText);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class Argon2dRaw : IHashMethod
        {
            private byte[] _salt;
            public byte[] SaltBytes
            {
                get
                {
                    return _salt;
                }
                set
                {
                    if (value.Length < 8)
                        value = new byte[8];

                    _salt = value;
                }
            }
            public string Salt
            {
                get
                {
                    return Convert.ToBase64String(_salt);
                }
                set
                {
                    try
                    {
                        if (Convert.FromBase64String(value).Length < 8)
                            value = Convert.ToBase64String(new byte[8]);

                        _salt = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        if (Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value))).Length < 8)
                            value = Convert.ToBase64String(new byte[8]);

                        _salt = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            private ushort _hashLength;
            public ushort HashLength
            {
                get
                {
                    return _hashLength;
                }
                set
                {
                    if (value < 6)
                        value = 6;

                    _hashLength = value;
                }
            }
            private int _degreeOfParallelism;
            public int DegreeOfParallelism
            {
                get
                {
                    return _degreeOfParallelism;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _degreeOfParallelism = value;
                }
            }
            private int _iterations;
            public int Iterations
            {
                get
                {
                    return _iterations;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _iterations = value;
                }
            }
            private int _memorySize;
            public int MemorySize
            {
                get
                {
                    return _memorySize;
                }
                set
                {
                    if (value < 8)
                        value = 8;

                    _memorySize = value;
                }
            }
            private byte[] _associatedData;
            public byte[] AssociatedDataBytes
            {
                get
                {
                    return _associatedData;
                }
                set
                {
                    _associatedData = value;
                }
            }
            public string AssociatedData
            {
                get
                {
                    return Convert.ToBase64String(_associatedData);
                }
                set
                {
                    try
                    {
                        _associatedData = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _associatedData = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            private byte[] _knownSecret;
            public byte[] KnownSecretBytes
            {
                get
                {
                    return _knownSecret;
                }
                set
                {
                    _knownSecret = value;
                }
            }
            public string KnownSecret
            {
                get
                {
                    return Convert.ToBase64String(_knownSecret);
                }
                set
                {
                    try
                    {
                        _knownSecret = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _knownSecret = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            public bool FixedHashLength { get; set; }

            public bool Initialized { get; }

            public Argon2dRaw()
            {
                SaltBytes = new byte[8];
                HashLength = 6;
                DegreeOfParallelism = 2 * 2;
                Iterations = 4;
                MemorySize = (1 * 1024) * 128;
                AssociatedDataBytes = Array.Empty<byte>();
                KnownSecretBytes = Array.Empty<byte>();

                FixedHashLength = true;

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);

                Konscious.Security.Cryptography.Argon2d argon2Service = new Konscious.Security.Cryptography.Argon2d(data)
                {
                    Salt = SaltBytes,
                    DegreeOfParallelism = DegreeOfParallelism,
                    Iterations = Iterations,
                    MemorySize = MemorySize,
                    AssociatedData = AssociatedDataBytes,
                    KnownSecret = KnownSecretBytes
                };

                byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : SaltBytes.Length);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                return hashText.ToString();
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                var plainTextHash = GetHash(plainText);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class Argon2idRaw : IHashMethod
        {
            private byte[] _salt;
            public byte[] SaltBytes
            {
                get
                {
                    return _salt;
                }
                set
                {
                    if (value.Length < 8)
                        value = new byte[8];

                    _salt = value;
                }
            }
            public string Salt
            {
                get
                {
                    return Convert.ToBase64String(_salt);
                }
                set
                {
                    try
                    {
                        if (Convert.FromBase64String(value).Length < 8)
                            value = Convert.ToBase64String(new byte[8]);

                        _salt = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        if (Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value))).Length < 8)
                            value = Convert.ToBase64String(new byte[8]);

                        _salt = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            private ushort _hashLength;
            public ushort HashLength
            {
                get
                {
                    return _hashLength;
                }
                set
                {
                    if (value < 6)
                        value = 6;

                    _hashLength = value;
                }
            }
            private int _degreeOfParallelism;
            public int DegreeOfParallelism
            {
                get
                {
                    return _degreeOfParallelism;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _degreeOfParallelism = value;
                }
            }
            private int _iterations;
            public int Iterations
            {
                get
                {
                    return _iterations;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _iterations = value;
                }
            }
            private int _memorySize;
            public int MemorySize
            {
                get
                {
                    return _memorySize;
                }
                set
                {
                    if (value < 8)
                        value = 8;

                    _memorySize = value;
                }
            }
            private byte[] _associatedData;
            public byte[] AssociatedDataBytes
            {
                get
                {
                    return _associatedData;
                }
                set
                {
                    _associatedData = value;
                }
            }
            public string AssociatedData
            {
                get
                {
                    return Convert.ToBase64String(_associatedData);
                }
                set
                {
                    try
                    {
                        _associatedData = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _associatedData = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            private byte[] _knownSecret;
            public byte[] KnownSecretBytes
            {
                get
                {
                    return _knownSecret;
                }
                set
                {
                    _knownSecret = value;
                }
            }
            public string KnownSecret
            {
                get
                {
                    return Convert.ToBase64String(_knownSecret);
                }
                set
                {
                    try
                    {
                        _knownSecret = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _knownSecret = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            public bool FixedHashLength { get; set; }

            public bool Initialized { get; }

            public Argon2idRaw()
            {
                SaltBytes = new byte[8];
                HashLength = 6;
                DegreeOfParallelism = 2 * 2;
                Iterations = 4;
                MemorySize = (1 * 1024) * 128;
                AssociatedDataBytes = Array.Empty<byte>();
                KnownSecretBytes = Array.Empty<byte>();

                FixedHashLength = true;

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);

                Konscious.Security.Cryptography.Argon2id argon2Service = new Konscious.Security.Cryptography.Argon2id(data)
                {
                    Salt = SaltBytes,
                    DegreeOfParallelism = DegreeOfParallelism,
                    Iterations = Iterations,
                    MemorySize = MemorySize,
                    AssociatedData = AssociatedDataBytes,
                    KnownSecret = KnownSecretBytes
                };

                byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : SaltBytes.Length);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                return hashText.ToString();
            }
            public bool VerifyHash(string plainText, string hashText)
            {
                var plainTextHash = GetHash(plainText);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class Argon2iWNP : IHashMethod
        {
            private ushort _saltLength;
            public ushort SaltLength
            {
                get
                {
                    return _saltLength;
                }
                set
                {
                    if (value < 8)
                        value = 8;

                    _saltLength = value;
                }
            }
            private ushort _hashLength;
            public ushort HashLength
            {
                get
                {
                    return _hashLength;
                }
                set
                {
                    if (value < 6)
                        value = 6;

                    _hashLength = value;
                }
            }
            private int _degreeOfParallelism;
            public int DegreeOfParallelism
            {
                get
                {
                    return _degreeOfParallelism;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _degreeOfParallelism = value;
                }
            }
            private int _iterations;
            public int Iterations
            {
                get
                {
                    return _iterations;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _iterations = value;
                }
            }
            private int _memorySize;
            public int MemorySize
            {
                get
                {
                    return _memorySize;
                }
                set
                {
                    if (value < 8)
                        value = 8;

                    _memorySize = value;
                }
            }
            private byte[] _associatedData;
            public byte[] AssociatedDataBytes
            {
                get
                {
                    return _associatedData;
                }
                set
                {
                    _associatedData = value;
                }
            }
            public string AssociatedData
            {
                get
                {
                    return Convert.ToBase64String(_associatedData);
                }
                set
                {
                    try
                    {
                        _associatedData = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _associatedData = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            private byte[] _knownSecret;
            public byte[] KnownSecretBytes
            {
                get
                {
                    return _knownSecret;
                }
                set
                {
                    _knownSecret = value;
                }
            }
            public string KnownSecret
            {
                get
                {
                    return Convert.ToBase64String(_knownSecret);
                }
                set
                {
                    try
                    {
                        _knownSecret = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _knownSecret = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            public bool FixedHashLength { get; set; }

            public bool Initialized { get; }

            public Argon2iWNP()
            {
                SaltLength = 8;
                HashLength = 6;
                DegreeOfParallelism = 2 * 2;
                Iterations = 4;
                MemorySize = (1 * 1024) * 128;
                AssociatedDataBytes = Array.Empty<byte>();
                KnownSecretBytes = Array.Empty<byte>();

                FixedHashLength = true;

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);

                byte[] hashSalt = Convert.FromBase64String(GenSalt(SaltLength));

                Konscious.Security.Cryptography.Argon2i argon2Service = new Konscious.Security.Cryptography.Argon2i(data)
                {
                    Salt = hashSalt,
                    DegreeOfParallelism = DegreeOfParallelism,
                    Iterations = Iterations,
                    MemorySize = MemorySize,
                    AssociatedData = AssociatedDataBytes,
                    KnownSecret = KnownSecretBytes
                };

                byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                string hashString = Convert.ToBase64String(hashSalt) + "=/" + Convert.ToBase64String(TextEncoding.GetBytes(hashText.ToString()));
                hashString = Convert.ToBase64String(TextEncoding.GetBytes(hashString));

                return hashString;
            }
            public string GetHash(string plainText, string salt)
            {
                byte[] data = TextEncoding.GetBytes(plainText);

                byte[] hashSalt;
                try
                {
                    hashSalt = Convert.FromBase64String(salt);
                }
                catch (FormatException)
                {
                    hashSalt = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(salt)));
                }

                Konscious.Security.Cryptography.Argon2i argon2Service = new Konscious.Security.Cryptography.Argon2i(data)
                {
                    Salt = hashSalt,
                    DegreeOfParallelism = DegreeOfParallelism,
                    Iterations = Iterations,
                    MemorySize = MemorySize,
                    AssociatedData = AssociatedDataBytes,
                    KnownSecret = KnownSecretBytes
                };

                byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                string hashString = Convert.ToBase64String(hashSalt) + "=/" + Convert.ToBase64String(TextEncoding.GetBytes(hashText.ToString()));
                hashString = Convert.ToBase64String(TextEncoding.GetBytes(hashString));

                return hashString;
            }

            public bool VerifyHash(string plainText, string hashText)
            {
                string hashTextSub = TextEncoding.GetString(Convert.FromBase64String(hashText));

                string hashSalt = hashTextSub.Substring(0, hashTextSub.IndexOf('='));
                if (hashTextSub.Contains("===/"))
                    hashSalt += "==";
                else if (hashTextSub.Contains("==/"))
                    hashSalt += "=";

                var plainTextHash = GetHash(plainText, hashSalt);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class Argon2dWNP : IHashMethod
        {
            private ushort _saltLength;
            public ushort SaltLength
            {
                get
                {
                    return _saltLength;
                }
                set
                {
                    if (value < 8)
                        value = 8;

                    _saltLength = value;
                }
            }
            private ushort _hashLength;
            public ushort HashLength
            {
                get
                {
                    return _hashLength;
                }
                set
                {
                    if (value < 6)
                        value = 6;

                    _hashLength = value;
                }
            }
            private int _degreeOfParallelism;
            public int DegreeOfParallelism
            {
                get
                {
                    return _degreeOfParallelism;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _degreeOfParallelism = value;
                }
            }
            private int _iterations;
            public int Iterations
            {
                get
                {
                    return _iterations;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _iterations = value;
                }
            }
            private int _memorySize;
            public int MemorySize
            {
                get
                {
                    return _memorySize;
                }
                set
                {
                    if (value < 8)
                        value = 8;

                    _memorySize = value;
                }
            }
            private byte[] _associatedData;
            public byte[] AssociatedDataBytes
            {
                get
                {
                    return _associatedData;
                }
                set
                {
                    _associatedData = value;
                }
            }
            public string AssociatedData
            {
                get
                {
                    return Convert.ToBase64String(_associatedData);
                }
                set
                {
                    try
                    {
                        _associatedData = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _associatedData = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            private byte[] _knownSecret;
            public byte[] KnownSecretBytes
            {
                get
                {
                    return _knownSecret;
                }
                set
                {
                    _knownSecret = value;
                }
            }
            public string KnownSecret
            {
                get
                {
                    return Convert.ToBase64String(_knownSecret);
                }
                set
                {
                    try
                    {
                        _knownSecret = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _knownSecret = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            public bool FixedHashLength { get; set; }

            public bool Initialized { get; }

            public Argon2dWNP()
            {
                SaltLength = 8;
                HashLength = 6;
                DegreeOfParallelism = 2 * 2;
                Iterations = 4;
                MemorySize = (1 * 1024) * 128;
                AssociatedDataBytes = Array.Empty<byte>();
                KnownSecretBytes = Array.Empty<byte>();

                FixedHashLength = true;

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);

                byte[] hashSalt = Convert.FromBase64String(GenSalt(SaltLength));

                Konscious.Security.Cryptography.Argon2d argon2Service = new Konscious.Security.Cryptography.Argon2d(data)
                {
                    Salt = hashSalt,
                    DegreeOfParallelism = DegreeOfParallelism,
                    Iterations = Iterations,
                    MemorySize = MemorySize,
                    AssociatedData = AssociatedDataBytes,
                    KnownSecret = KnownSecretBytes
                };

                byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                string hashString = Convert.ToBase64String(hashSalt) + "=/" + Convert.ToBase64String(TextEncoding.GetBytes(hashText.ToString()));
                hashString = Convert.ToBase64String(TextEncoding.GetBytes(hashString));

                return hashString;
            }
            public string GetHash(string plainText, string salt)
            {
                byte[] data = TextEncoding.GetBytes(plainText);

                byte[] hashSalt;
                try
                {
                    hashSalt = Convert.FromBase64String(salt);
                }
                catch (FormatException)
                {
                    hashSalt = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(salt)));
                }

                Konscious.Security.Cryptography.Argon2d argon2Service = new Konscious.Security.Cryptography.Argon2d(data)
                {
                    Salt = hashSalt,
                    DegreeOfParallelism = DegreeOfParallelism,
                    Iterations = Iterations,
                    MemorySize = MemorySize,
                    AssociatedData = AssociatedDataBytes,
                    KnownSecret = KnownSecretBytes
                };

                byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                string hashString = Convert.ToBase64String(hashSalt) + "=/" + Convert.ToBase64String(TextEncoding.GetBytes(hashText.ToString()));
                hashString = Convert.ToBase64String(TextEncoding.GetBytes(hashString));

                return hashString;
            }

            public bool VerifyHash(string plainText, string hashText)
            {
                string hashTextSub = TextEncoding.GetString(Convert.FromBase64String(hashText));

                string hashSalt = hashTextSub.Substring(0, hashTextSub.IndexOf('='));
                if (hashTextSub.Contains("===/"))
                    hashSalt += "==";
                else if (hashTextSub.Contains("==/"))
                    hashSalt += "=";

                var plainTextHash = GetHash(plainText, hashSalt);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class Argon2idWNP : IHashMethod
        {
            private ushort _saltLength;
            public ushort SaltLength
            {
                get
                {
                    return _saltLength;
                }
                set
                {
                    if (value < 8)
                        value = 8;

                    _saltLength = value;
                }
            }
            private ushort _hashLength;
            public ushort HashLength
            {
                get
                {
                    return _hashLength;
                }
                set
                {
                    if (value < 6)
                        value = 6;

                    _hashLength = value;
                }
            }
            private int _degreeOfParallelism;
            public int DegreeOfParallelism
            {
                get
                {
                    return _degreeOfParallelism;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _degreeOfParallelism = value;
                }
            }
            private int _iterations;
            public int Iterations
            {
                get
                {
                    return _iterations;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _iterations = value;
                }
            }
            private int _memorySize;
            public int MemorySize
            {
                get
                {
                    return _memorySize;
                }
                set
                {
                    if (value < 8)
                        value = 8;

                    _memorySize = value;
                }
            }
            private byte[] _associatedData;
            public byte[] AssociatedDataBytes
            {
                get
                {
                    return _associatedData;
                }
                set
                {
                    _associatedData = value;
                }
            }
            public string AssociatedData
            {
                get
                {
                    return Convert.ToBase64String(_associatedData);
                }
                set
                {
                    try
                    {
                        _associatedData = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _associatedData = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            private byte[] _knownSecret;
            public byte[] KnownSecretBytes
            {
                get
                {
                    return _knownSecret;
                }
                set
                {
                    _knownSecret = value;
                }
            }
            public string KnownSecret
            {
                get
                {
                    return Convert.ToBase64String(_knownSecret);
                }
                set
                {
                    try
                    {
                        _knownSecret = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _knownSecret = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            public bool FixedHashLength { get; set; }

            public bool Initialized { get; }

            public Argon2idWNP()
            {
                SaltLength = 8;
                HashLength = 6;
                DegreeOfParallelism = 2 * 2;
                Iterations = 4;
                MemorySize = (1 * 1024) * 128;
                AssociatedDataBytes = Array.Empty<byte>();
                KnownSecretBytes = Array.Empty<byte>();

                FixedHashLength = true;

                Initialized = true;
            }

            public string GetHash(string plainText)
            {
                byte[] data = TextEncoding.GetBytes(plainText);

                byte[] hashSalt = Convert.FromBase64String(GenSalt(SaltLength));

                Konscious.Security.Cryptography.Argon2id argon2Service = new Konscious.Security.Cryptography.Argon2id(data)
                {
                    Salt = hashSalt,
                    DegreeOfParallelism = DegreeOfParallelism,
                    Iterations = Iterations,
                    MemorySize = MemorySize,
                    AssociatedData = AssociatedDataBytes,
                    KnownSecret = KnownSecretBytes
                };

                byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                string hashString = Convert.ToBase64String(hashSalt) + "=/" + Convert.ToBase64String(TextEncoding.GetBytes(hashText.ToString()));
                hashString = Convert.ToBase64String(TextEncoding.GetBytes(hashString));

                return hashString;
            }
            public string GetHash(string plainText, string salt)
            {
                byte[] data = TextEncoding.GetBytes(plainText);

                byte[] hashSalt;
                try
                {
                    hashSalt = Convert.FromBase64String(salt);
                }
                catch (FormatException)
                {
                    hashSalt = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(salt)));
                }

                Konscious.Security.Cryptography.Argon2id argon2Service = new Konscious.Security.Cryptography.Argon2id(data)
                {
                    Salt = hashSalt,
                    DegreeOfParallelism = DegreeOfParallelism,
                    Iterations = Iterations,
                    MemorySize = MemorySize,
                    AssociatedData = AssociatedDataBytes,
                    KnownSecret = KnownSecretBytes
                };

                byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                string hashString = Convert.ToBase64String(hashSalt) + "=/" + Convert.ToBase64String(TextEncoding.GetBytes(hashText.ToString()));
                hashString = Convert.ToBase64String(TextEncoding.GetBytes(hashString));

                return hashString;
            }

            public bool VerifyHash(string plainText, string hashText)
            {
                string hashTextSub = TextEncoding.GetString(Convert.FromBase64String(hashText));

                string hashSalt = hashTextSub.Substring(0, hashTextSub.IndexOf('='));
                if (hashTextSub.Contains("===/"))
                    hashSalt += "==";
                else if (hashTextSub.Contains("==/"))
                    hashSalt += "=";

                var plainTextHash = GetHash(plainText, hashSalt);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }
        }

        public sealed class Argon2iWP : IHashMethod
        {
            private ushort _saltLength;
            public ushort SaltLength
            {
                get
                {
                    return _saltLength;
                }
                set
                {
                    if (value < 8)
                        value = 8;

                    _saltLength = value;
                }
            }
            private ushort _hashLength;
            public ushort HashLength
            {
                get
                {
                    return _hashLength;
                }
                set
                {
                    if (value < 6)
                        value = 6;

                    _hashLength = value;
                }
            }
            private int _degreeOfParallelism;
            public int DegreeOfParallelism
            {
                get
                {
                    return _degreeOfParallelism;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _degreeOfParallelism = value;
                }
            }
            private int _iterations;
            public int Iterations
            {
                get
                {
                    return _iterations;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _iterations = value;
                }
            }
            private int _memorySize;
            public int MemorySize
            {
                get
                {
                    return _memorySize;
                }
                set
                {
                    if (value < 8)
                        value = 8;

                    _memorySize = value;
                }
            }
            private byte[] _associatedData;
            public byte[] AssociatedDataBytes
            {
                get
                {
                    return _associatedData;
                }
                set
                {
                    _associatedData = value;
                }
            }
            public string AssociatedData
            {
                get
                {
                    return Convert.ToBase64String(_associatedData);
                }
                set
                {
                    try
                    {
                        _associatedData = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _associatedData = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            private byte[] _knownSecret;
            public byte[] KnownSecretBytes
            {
                get
                {
                    return _knownSecret;
                }
                set
                {
                    _knownSecret = value;
                }
            }
            public string KnownSecret
            {
                get
                {
                    return Convert.ToBase64String(_knownSecret);
                }
                set
                {
                    try
                    {
                        _knownSecret = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _knownSecret = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            public bool FixedHashLength { get; set; }

            public bool Initialized { get; }

            public Argon2iWP()
            {
                SaltLength = 8;
                HashLength = 6;
                DegreeOfParallelism = 2 * 2;
                Iterations = 4;
                MemorySize = (1 * 1024) * 128;
                AssociatedDataBytes = Array.Empty<byte>();
                KnownSecretBytes = Array.Empty<byte>();

                FixedHashLength = true;

                Initialized = true;
            }

            public static Argon2Metadata GetMetadata(string hashText)
            {
                return new Argon2Metadata(hashText);
            }

            public string GetHash(string plainText)
            {
                return GetHash(plainText, MemorySize, Iterations,
                    DegreeOfParallelism, AssociatedData, KnownSecret);
            }
            public string GetHash(string plainText, int memorySize, int iterations,
                int degreeOfParallelism)
            {
                return GetHash(plainText, memorySize, iterations,
                    degreeOfParallelism, AssociatedData, KnownSecret);
            }
            public string GetHash(string plainText, int memorySize, int iterations,
                int degreeOfParallelism, byte[] associatedData)
            {
                return GetHash(plainText, memorySize, iterations,
                    degreeOfParallelism, Convert.ToBase64String(associatedData), KnownSecret);
            }
            public string GetHash(string plainText, int memorySize, int iterations,
                int degreeOfParallelism, string associatedData)
            {
                return GetHash(plainText, memorySize, iterations,
                    degreeOfParallelism, associatedData, KnownSecret);
            }
            public string GetHash(string plainText, int memorySize, int iterations,
                int degreeOfParallelism, byte[] associatedData, byte[] knownSecret)
            {
                return GetHash(plainText, memorySize, iterations,
                    degreeOfParallelism, Convert.ToBase64String(associatedData), Convert.ToBase64String(knownSecret));
            }
            public string GetHash(string plainText, int memorySize, int iterations,
                int degreeOfParallelism, string associatedData, string knownSecret)
            {
                byte[] data = TextEncoding.GetBytes(plainText);

                byte[] hashSalt = Convert.FromBase64String(GenSalt(SaltLength));

                if (memorySize < 8)
                    memorySize = 8;

                if (iterations < 1)
                    iterations = 1;

                if (degreeOfParallelism < 1)
                    degreeOfParallelism = 1;

                byte[] associatedDataBytes;
                try
                {
                    associatedDataBytes = Convert.FromBase64String(associatedData);
                }
                catch (FormatException)
                {
                    associatedDataBytes = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(associatedData)));
                }

                byte[] knownSecretBytes;
                try
                {
                    knownSecretBytes = Convert.FromBase64String(knownSecret);
                }
                catch (FormatException)
                {
                    knownSecretBytes = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(knownSecret)));
                }

                Konscious.Security.Cryptography.Argon2i argon2Service = new Konscious.Security.Cryptography.Argon2i(data)
                {
                    Salt = hashSalt,
                    DegreeOfParallelism = degreeOfParallelism,
                    Iterations = iterations,
                    MemorySize = memorySize,
                    AssociatedData = associatedDataBytes,
                    KnownSecret = knownSecretBytes
                };

                byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                string hashString = $"${Enum.GetName(typeof(Argon2Type), Argon2Type.Argon2i)?.ToLower()}$v=19$m={memorySize},t={iterations},p={degreeOfParallelism}${Base64.RemovePadding(Convert.ToBase64String(hashSalt))}${Base64.RemovePadding(Convert.ToBase64String(TextEncoding.GetBytes(hashText.ToString())))}";

                return hashString;
            }
            public string GetHash(string plainText, string salt)
            {
                return GetHash(plainText, salt, MemorySize, Iterations,
                    DegreeOfParallelism, AssociatedData, KnownSecret);
            }
            public string GetHash(string plainText, string salt, int memorySize, int iterations,
                int degreeOfParallelism)
            {
                return GetHash(plainText, salt, memorySize, iterations,
                    degreeOfParallelism, AssociatedData, KnownSecret);
            }
            public string GetHash(string plainText, string salt, int memorySize, int iterations,
                int degreeOfParallelism, byte[] associatedData)
            {
                return GetHash(plainText, salt, memorySize, iterations,
                    degreeOfParallelism, Convert.ToBase64String(associatedData), KnownSecret);
            }
            public string GetHash(string plainText, string salt, int memorySize, int iterations,
                int degreeOfParallelism, string associatedData)
            {
                return GetHash(plainText, salt, memorySize, iterations,
                    degreeOfParallelism, associatedData, KnownSecret);
            }
            public string GetHash(string plainText, string salt, int memorySize, int iterations,
                int degreeOfParallelism, byte[] associatedData, byte[] knownSecret)
            {
                return GetHash(plainText, salt, memorySize, iterations,
                    degreeOfParallelism, Convert.ToBase64String(associatedData), Convert.ToBase64String(knownSecret));
            }
            public string GetHash(string plainText, string salt, int memorySize, int iterations,
                int degreeOfParallelism, string associatedData, string knownSecret)
            {
                byte[] data = TextEncoding.GetBytes(plainText);

                byte[] hashSalt;
                try
                {
                    hashSalt = Convert.FromBase64String(salt);
                }
                catch (FormatException)
                {
                    hashSalt = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(salt)));
                }

                if (memorySize < 8)
                    memorySize = 8;

                if (iterations < 1)
                    iterations = 1;

                if (degreeOfParallelism < 1)
                    degreeOfParallelism = 1;

                byte[] associatedDataBytes;
                try
                {
                    associatedDataBytes = Convert.FromBase64String(associatedData);
                }
                catch (FormatException)
                {
                    associatedDataBytes = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(associatedData)));
                }

                byte[] knownSecretBytes;
                try
                {
                    knownSecretBytes = Convert.FromBase64String(knownSecret);
                }
                catch (FormatException)
                {
                    knownSecretBytes = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(knownSecret)));
                }

                Konscious.Security.Cryptography.Argon2i argon2Service = new Konscious.Security.Cryptography.Argon2i(data)
                {
                    Salt = hashSalt,
                    DegreeOfParallelism = degreeOfParallelism,
                    Iterations = iterations,
                    MemorySize = memorySize,
                    AssociatedData = associatedDataBytes,
                    KnownSecret = knownSecretBytes
                };

                byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                string hashString = $"${Enum.GetName(typeof(Argon2Type), Argon2Type.Argon2i)?.ToLower()}$v=19$m={memorySize},t={iterations},p={degreeOfParallelism}${Base64.RemovePadding(Convert.ToBase64String(hashSalt))}${Base64.RemovePadding(Convert.ToBase64String(TextEncoding.GetBytes(hashText.ToString())))}";

                return hashString;
            }

            public bool VerifyHash(string plainText, string hashText)
            {
                return VerifyHash(plainText, hashText, AssociatedData,
                    KnownSecret);
            }
            public bool VerifyHash(string plainText, string hashText, byte[] associatedData)
            {
                return VerifyHash(plainText, hashText, Convert.ToBase64String(associatedData),
                    KnownSecret);
            }
            public bool VerifyHash(string plainText, string hashText, string associatedData)
            {
                return VerifyHash(plainText, hashText, associatedData,
                    KnownSecret);
            }
            public bool VerifyHash(string plainText, string hashText, byte[] associatedData, byte[] knownSecret)
            {
                return VerifyHash(plainText, hashText, Convert.ToBase64String(associatedData),
                    Convert.ToBase64String(knownSecret));
            }
            public bool VerifyHash(string plainText, string hashText, string associatedData, string knownSecret)
            {
                Argon2Metadata metadata = GetMetadata(hashText);

                var plainTextHash = GetHash(plainText, metadata.Salt, metadata.MemorySize, metadata.Iterations,
                    metadata.DegreeOfParallelism, associatedData, knownSecret);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }

            public bool VerifyAndUpdateHash(string plainText, string hashText, out bool isUpdated, out string newHashText)
            {
                return VerifyAndUpdateHash(plainText, hashText, MemorySize, Iterations,
                    DegreeOfParallelism, AssociatedData, KnownSecret, out isUpdated, out newHashText);
            }
            public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
                int newDegreeOfParallelism, out bool isUpdated, out string newHashText)
            {
                return VerifyAndUpdateHash(plainText, hashText, newMemorySize, newIterations,
                    newDegreeOfParallelism, AssociatedData, KnownSecret, out isUpdated, out newHashText);
            }
            public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
                int newDegreeOfParallelism, byte[] associatedData, out bool isUpdated, out string newHashText)
            {
                return VerifyAndUpdateHash(plainText, hashText, newMemorySize, newIterations,
                    newDegreeOfParallelism, Convert.ToBase64String(associatedData), KnownSecret,
                    out isUpdated, out newHashText);
            }
            public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
                int newDegreeOfParallelism, string associatedData, out bool isUpdated, out string newHashText)
            {
                return VerifyAndUpdateHash(plainText, hashText, newMemorySize, newIterations,
                    newDegreeOfParallelism, associatedData, KnownSecret, out isUpdated, out newHashText);
            }
            public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
                int newDegreeOfParallelism, byte[] associatedData, byte[] knownSecret, out bool isUpdated, out string newHashText)
            {
                return VerifyAndUpdateHash(plainText, hashText, newMemorySize, newIterations,
                    newDegreeOfParallelism, Convert.ToBase64String(associatedData), Convert.ToBase64String(knownSecret),
                    out isUpdated, out newHashText);
            }
            public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
                int newDegreeOfParallelism, string associatedData, string knownSecret, out bool isUpdated, out string newHashText)
            {
                bool result;
                isUpdated = false;
                newHashText = hashText;

                result = VerifyHash(plainText, hashText, associatedData, knownSecret);

                if (!result)
                    return false;

                Argon2Metadata metadata = GetMetadata(hashText);

                isUpdated = metadata.MemorySize != newMemorySize || metadata.Iterations != newIterations || metadata.DegreeOfParallelism != newDegreeOfParallelism;

                if (isUpdated)
                    newHashText = GetHash(plainText, newMemorySize, newIterations, newDegreeOfParallelism, associatedData, knownSecret);
                else
                    newHashText = hashText;

                return true;
            }
        }

        public sealed class Argon2dWP : IHashMethod
        {
            private ushort _saltLength;
            public ushort SaltLength
            {
                get
                {
                    return _saltLength;
                }
                set
                {
                    if (value < 8)
                        value = 8;

                    _saltLength = value;
                }
            }
            private ushort _hashLength;
            public ushort HashLength
            {
                get
                {
                    return _hashLength;
                }
                set
                {
                    if (value < 6)
                        value = 6;

                    _hashLength = value;
                }
            }
            private int _degreeOfParallelism;
            public int DegreeOfParallelism
            {
                get
                {
                    return _degreeOfParallelism;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _degreeOfParallelism = value;
                }
            }
            private int _iterations;
            public int Iterations
            {
                get
                {
                    return _iterations;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _iterations = value;
                }
            }
            private int _memorySize;
            public int MemorySize
            {
                get
                {
                    return _memorySize;
                }
                set
                {
                    if (value < 8)
                        value = 8;

                    _memorySize = value;
                }
            }
            private byte[] _associatedData;
            public byte[] AssociatedDataBytes
            {
                get
                {
                    return _associatedData;
                }
                set
                {
                    _associatedData = value;
                }
            }
            public string AssociatedData
            {
                get
                {
                    return Convert.ToBase64String(_associatedData);
                }
                set
                {
                    try
                    {
                        _associatedData = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _associatedData = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            private byte[] _knownSecret;
            public byte[] KnownSecretBytes
            {
                get
                {
                    return _knownSecret;
                }
                set
                {
                    _knownSecret = value;
                }
            }
            public string KnownSecret
            {
                get
                {
                    return Convert.ToBase64String(_knownSecret);
                }
                set
                {
                    try
                    {
                        _knownSecret = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _knownSecret = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            public bool FixedHashLength { get; set; }

            public bool Initialized { get; }

            public Argon2dWP()
            {
                SaltLength = 8;
                HashLength = 6;
                DegreeOfParallelism = 2 * 2;
                Iterations = 4;
                MemorySize = (1 * 1024) * 128;
                AssociatedDataBytes = Array.Empty<byte>();
                KnownSecretBytes = Array.Empty<byte>();

                FixedHashLength = true;

                Initialized = true;
            }

            public static Argon2Metadata GetMetadata(string hashText)
            {
                return new Argon2Metadata(hashText);
            }

            public string GetHash(string plainText)
            {
                return GetHash(plainText, MemorySize, Iterations,
                    DegreeOfParallelism, AssociatedData, KnownSecret);
            }
            public string GetHash(string plainText, int memorySize, int iterations,
                int degreeOfParallelism)
            {
                return GetHash(plainText, memorySize, iterations,
                    degreeOfParallelism, AssociatedData, KnownSecret);
            }
            public string GetHash(string plainText, int memorySize, int iterations,
                int degreeOfParallelism, byte[] associatedData)
            {
                return GetHash(plainText, memorySize, iterations,
                    degreeOfParallelism, Convert.ToBase64String(associatedData), KnownSecret);
            }
            public string GetHash(string plainText, int memorySize, int iterations,
                int degreeOfParallelism, string associatedData)
            {
                return GetHash(plainText, memorySize, iterations,
                    degreeOfParallelism, associatedData, KnownSecret);
            }
            public string GetHash(string plainText, int memorySize, int iterations,
                int degreeOfParallelism, byte[] associatedData, byte[] knownSecret)
            {
                return GetHash(plainText, memorySize, iterations,
                    degreeOfParallelism, Convert.ToBase64String(associatedData), Convert.ToBase64String(knownSecret));
            }
            public string GetHash(string plainText, int memorySize, int iterations,
                int degreeOfParallelism, string associatedData, string knownSecret)
            {
                byte[] data = TextEncoding.GetBytes(plainText);

                byte[] hashSalt = Convert.FromBase64String(GenSalt(SaltLength));

                if (memorySize < 8)
                    memorySize = 8;

                if (iterations < 1)
                    iterations = 1;

                if (degreeOfParallelism < 1)
                    degreeOfParallelism = 1;

                byte[] associatedDataBytes;
                try
                {
                    associatedDataBytes = Convert.FromBase64String(associatedData);
                }
                catch (FormatException)
                {
                    associatedDataBytes = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(associatedData)));
                }

                byte[] knownSecretBytes;
                try
                {
                    knownSecretBytes = Convert.FromBase64String(knownSecret);
                }
                catch (FormatException)
                {
                    knownSecretBytes = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(knownSecret)));
                }

                Konscious.Security.Cryptography.Argon2d argon2Service = new Konscious.Security.Cryptography.Argon2d(data)
                {
                    Salt = hashSalt,
                    DegreeOfParallelism = degreeOfParallelism,
                    Iterations = iterations,
                    MemorySize = memorySize,
                    AssociatedData = associatedDataBytes,
                    KnownSecret = knownSecretBytes
                };

                byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                string hashString = $"${Enum.GetName(typeof(Argon2Type), Argon2Type.Argon2d)?.ToLower()}$v=19$m={memorySize},t={iterations},p={degreeOfParallelism}${Base64.RemovePadding(Convert.ToBase64String(hashSalt))}${Base64.RemovePadding(Convert.ToBase64String(TextEncoding.GetBytes(hashText.ToString())))}";

                return hashString;
            }
            public string GetHash(string plainText, string salt)
            {
                return GetHash(plainText, salt, MemorySize, Iterations,
                    DegreeOfParallelism, AssociatedData, KnownSecret);
            }
            public string GetHash(string plainText, string salt, int memorySize, int iterations,
                int degreeOfParallelism)
            {
                return GetHash(plainText, salt, memorySize, iterations,
                    degreeOfParallelism, AssociatedData, KnownSecret);
            }
            public string GetHash(string plainText, string salt, int memorySize, int iterations,
                int degreeOfParallelism, byte[] associatedData)
            {
                return GetHash(plainText, salt, memorySize, iterations,
                    degreeOfParallelism, Convert.ToBase64String(associatedData), KnownSecret);
            }
            public string GetHash(string plainText, string salt, int memorySize, int iterations,
                int degreeOfParallelism, string associatedData)
            {
                return GetHash(plainText, salt, memorySize, iterations,
                    degreeOfParallelism, associatedData, KnownSecret);
            }
            public string GetHash(string plainText, string salt, int memorySize, int iterations,
                int degreeOfParallelism, byte[] associatedData, byte[] knownSecret)
            {
                return GetHash(plainText, salt, memorySize, iterations,
                    degreeOfParallelism, Convert.ToBase64String(associatedData), Convert.ToBase64String(knownSecret));
            }
            public string GetHash(string plainText, string salt, int memorySize, int iterations,
                int degreeOfParallelism, string associatedData, string knownSecret)
            {
                byte[] data = TextEncoding.GetBytes(plainText);

                byte[] hashSalt;
                try
                {
                    hashSalt = Convert.FromBase64String(salt);
                }
                catch (FormatException)
                {
                    hashSalt = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(salt)));
                }

                if (memorySize < 8)
                    memorySize = 8;

                if (iterations < 1)
                    iterations = 1;

                if (degreeOfParallelism < 1)
                    degreeOfParallelism = 1;

                byte[] associatedDataBytes;
                try
                {
                    associatedDataBytes = Convert.FromBase64String(associatedData);
                }
                catch (FormatException)
                {
                    associatedDataBytes = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(associatedData)));
                }

                byte[] knownSecretBytes;
                try
                {
                    knownSecretBytes = Convert.FromBase64String(knownSecret);
                }
                catch (FormatException)
                {
                    knownSecretBytes = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(knownSecret)));
                }

                Konscious.Security.Cryptography.Argon2d argon2Service = new Konscious.Security.Cryptography.Argon2d(data)
                {
                    Salt = hashSalt,
                    DegreeOfParallelism = degreeOfParallelism,
                    Iterations = iterations,
                    MemorySize = memorySize,
                    AssociatedData = associatedDataBytes,
                    KnownSecret = knownSecretBytes
                };

                byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                string hashString = $"${Enum.GetName(typeof(Argon2Type), Argon2Type.Argon2d)?.ToLower()}$v=19$m={memorySize},t={iterations},p={degreeOfParallelism}${Base64.RemovePadding(Convert.ToBase64String(hashSalt))}${Base64.RemovePadding(Convert.ToBase64String(TextEncoding.GetBytes(hashText.ToString())))}";

                return hashString;
            }

            public bool VerifyHash(string plainText, string hashText)
            {
                return VerifyHash(plainText, hashText, AssociatedData,
                    KnownSecret);
            }
            public bool VerifyHash(string plainText, string hashText, byte[] associatedData)
            {
                return VerifyHash(plainText, hashText, Convert.ToBase64String(associatedData),
                    KnownSecret);
            }
            public bool VerifyHash(string plainText, string hashText, string associatedData)
            {
                return VerifyHash(plainText, hashText, associatedData,
                    KnownSecret);
            }
            public bool VerifyHash(string plainText, string hashText, byte[] associatedData, byte[] knownSecret)
            {
                return VerifyHash(plainText, hashText, Convert.ToBase64String(associatedData),
                    Convert.ToBase64String(knownSecret));
            }
            public bool VerifyHash(string plainText, string hashText, string associatedData, string knownSecret)
            {
                Argon2Metadata metadata = GetMetadata(hashText);

                var plainTextHash = GetHash(plainText, metadata.Salt, metadata.MemorySize, metadata.Iterations,
                    metadata.DegreeOfParallelism, associatedData, knownSecret);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }

            public bool VerifyAndUpdateHash(string plainText, string hashText, out bool isUpdated, out string newHashText)
            {
                return VerifyAndUpdateHash(plainText, hashText, MemorySize, Iterations,
                    DegreeOfParallelism, AssociatedData, KnownSecret, out isUpdated, out newHashText);
            }
            public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
                int newDegreeOfParallelism, out bool isUpdated, out string newHashText)
            {
                return VerifyAndUpdateHash(plainText, hashText, newMemorySize, newIterations,
                    newDegreeOfParallelism, AssociatedData, KnownSecret, out isUpdated, out newHashText);
            }
            public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
                int newDegreeOfParallelism, byte[] associatedData, out bool isUpdated, out string newHashText)
            {
                return VerifyAndUpdateHash(plainText, hashText, newMemorySize, newIterations,
                    newDegreeOfParallelism, Convert.ToBase64String(associatedData), KnownSecret,
                    out isUpdated, out newHashText);
            }
            public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
                int newDegreeOfParallelism, string associatedData, out bool isUpdated, out string newHashText)
            {
                return VerifyAndUpdateHash(plainText, hashText, newMemorySize, newIterations,
                    newDegreeOfParallelism, associatedData, KnownSecret, out isUpdated, out newHashText);
            }
            public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
                int newDegreeOfParallelism, byte[] associatedData, byte[] knownSecret, out bool isUpdated, out string newHashText)
            {
                return VerifyAndUpdateHash(plainText, hashText, newMemorySize, newIterations,
                    newDegreeOfParallelism, Convert.ToBase64String(associatedData), Convert.ToBase64String(knownSecret),
                    out isUpdated, out newHashText);
            }
            public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
                int newDegreeOfParallelism, string associatedData, string knownSecret, out bool isUpdated, out string newHashText)
            {
                bool result;
                isUpdated = false;
                newHashText = hashText;

                result = VerifyHash(plainText, hashText, associatedData, knownSecret);

                if (!result)
                    return false;

                Argon2Metadata metadata = GetMetadata(hashText);

                isUpdated = metadata.MemorySize != newMemorySize || metadata.Iterations != newIterations || metadata.DegreeOfParallelism != newDegreeOfParallelism;

                if (isUpdated)
                    newHashText = GetHash(plainText, newMemorySize, newIterations, newDegreeOfParallelism, associatedData, knownSecret);
                else
                    newHashText = hashText;

                return true;
            }
        }

        public sealed class Argon2idWP : IHashMethod
        {
            private ushort _saltLength;
            public ushort SaltLength
            {
                get
                {
                    return _saltLength;
                }
                set
                {
                    if (value < 8)
                        value = 8;

                    _saltLength = value;
                }
            }
            private ushort _hashLength;
            public ushort HashLength
            {
                get
                {
                    return _hashLength;
                }
                set
                {
                    if (value < 6)
                        value = 6;

                    _hashLength = value;
                }
            }
            private int _degreeOfParallelism;
            public int DegreeOfParallelism
            {
                get
                {
                    return _degreeOfParallelism;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _degreeOfParallelism = value;
                }
            }
            private int _iterations;
            public int Iterations
            {
                get
                {
                    return _iterations;
                }
                set
                {
                    if (value < 1)
                        value = 1;

                    _iterations = value;
                }
            }
            private int _memorySize;
            public int MemorySize
            {
                get
                {
                    return _memorySize;
                }
                set
                {
                    if (value < 8)
                        value = 8;

                    _memorySize = value;
                }
            }
            private byte[] _associatedData;
            public byte[] AssociatedDataBytes
            {
                get
                {
                    return _associatedData;
                }
                set
                {
                    _associatedData = value;
                }
            }
            public string AssociatedData
            {
                get
                {
                    return Convert.ToBase64String(_associatedData);
                }
                set
                {
                    try
                    {
                        _associatedData = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _associatedData = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            private byte[] _knownSecret;
            public byte[] KnownSecretBytes
            {
                get
                {
                    return _knownSecret;
                }
                set
                {
                    _knownSecret = value;
                }
            }
            public string KnownSecret
            {
                get
                {
                    return Convert.ToBase64String(_knownSecret);
                }
                set
                {
                    try
                    {
                        _knownSecret = Convert.FromBase64String(value);
                    }
                    catch (FormatException)
                    {
                        _knownSecret = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(value)));
                    }
                }
            }
            public bool FixedHashLength { get; set; }

            public bool Initialized { get; }

            public Argon2idWP()
            {
                SaltLength = 8;
                HashLength = 6;
                DegreeOfParallelism = 2 * 2;
                Iterations = 4;
                MemorySize = (1 * 1024) * 128;
                AssociatedDataBytes = Array.Empty<byte>();
                KnownSecretBytes = Array.Empty<byte>();

                FixedHashLength = true;

                Initialized = true;
            }

            public static Argon2Metadata GetMetadata(string hashText)
            {
                return new Argon2Metadata(hashText);
            }

            public string GetHash(string plainText)
            {
                return GetHash(plainText, MemorySize, Iterations,
                    DegreeOfParallelism, AssociatedData, KnownSecret);
            }
            public string GetHash(string plainText, int memorySize, int iterations,
                int degreeOfParallelism)
            {
                return GetHash(plainText, memorySize, iterations,
                    degreeOfParallelism, AssociatedData, KnownSecret);
            }
            public string GetHash(string plainText, int memorySize, int iterations,
                int degreeOfParallelism, byte[] associatedData)
            {
                return GetHash(plainText, memorySize, iterations,
                    degreeOfParallelism, Convert.ToBase64String(associatedData), KnownSecret);
            }
            public string GetHash(string plainText, int memorySize, int iterations,
                int degreeOfParallelism, string associatedData)
            {
                return GetHash(plainText, memorySize, iterations,
                    degreeOfParallelism, associatedData, KnownSecret);
            }
            public string GetHash(string plainText, int memorySize, int iterations,
                int degreeOfParallelism, byte[] associatedData, byte[] knownSecret)
            {
                return GetHash(plainText, memorySize, iterations,
                    degreeOfParallelism, Convert.ToBase64String(associatedData), Convert.ToBase64String(knownSecret));
            }
            public string GetHash(string plainText, int memorySize, int iterations,
                int degreeOfParallelism, string associatedData, string knownSecret)
            {
                byte[] data = TextEncoding.GetBytes(plainText);

                byte[] hashSalt = Convert.FromBase64String(GenSalt(SaltLength));

                if (memorySize < 8)
                    memorySize = 8;

                if (iterations < 1)
                    iterations = 1;

                if (degreeOfParallelism < 1)
                    degreeOfParallelism = 1;

                byte[] associatedDataBytes;
                try
                {
                    associatedDataBytes = Convert.FromBase64String(associatedData);
                }
                catch (FormatException)
                {
                    associatedDataBytes = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(associatedData)));
                }

                byte[] knownSecretBytes;
                try
                {
                    knownSecretBytes = Convert.FromBase64String(knownSecret);
                }
                catch (FormatException)
                {
                    knownSecretBytes = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(knownSecret)));
                }

                Konscious.Security.Cryptography.Argon2id argon2Service = new Konscious.Security.Cryptography.Argon2id(data)
                {
                    Salt = hashSalt,
                    DegreeOfParallelism = degreeOfParallelism,
                    Iterations = iterations,
                    MemorySize = memorySize,
                    AssociatedData = associatedDataBytes,
                    KnownSecret = knownSecretBytes
                };

                byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                string hashString = $"${Enum.GetName(typeof(Argon2Type), Argon2Type.Argon2id)?.ToLower()}$v=19$m={memorySize},t={iterations},p={degreeOfParallelism}${Base64.RemovePadding(Convert.ToBase64String(hashSalt))}${Base64.RemovePadding(Convert.ToBase64String(TextEncoding.GetBytes(hashText.ToString())))}";

                return hashString;
            }
            public string GetHash(string plainText, string salt)
            {
                return GetHash(plainText, salt, MemorySize, Iterations,
                    DegreeOfParallelism, AssociatedData, KnownSecret);
            }
            public string GetHash(string plainText, string salt, int memorySize, int iterations,
                int degreeOfParallelism)
            {
                return GetHash(plainText, salt, memorySize, iterations,
                    degreeOfParallelism, AssociatedData, KnownSecret);
            }
            public string GetHash(string plainText, string salt, int memorySize, int iterations,
                int degreeOfParallelism, byte[] associatedData)
            {
                return GetHash(plainText, salt, memorySize, iterations,
                    degreeOfParallelism, Convert.ToBase64String(associatedData), KnownSecret);
            }
            public string GetHash(string plainText, string salt, int memorySize, int iterations,
                int degreeOfParallelism, string associatedData)
            {
                return GetHash(plainText, salt, memorySize, iterations,
                    degreeOfParallelism, associatedData, KnownSecret);
            }
            public string GetHash(string plainText, string salt, int memorySize, int iterations,
                int degreeOfParallelism, byte[] associatedData, byte[] knownSecret)
            {
                return GetHash(plainText, salt, memorySize, iterations,
                    degreeOfParallelism, Convert.ToBase64String(associatedData), Convert.ToBase64String(knownSecret));
            }
            public string GetHash(string plainText, string salt, int memorySize, int iterations,
                int degreeOfParallelism, string associatedData, string knownSecret)
            {
                byte[] data = TextEncoding.GetBytes(plainText);

                byte[] hashSalt;
                try
                {
                    hashSalt = Convert.FromBase64String(salt);
                }
                catch (FormatException)
                {
                    hashSalt = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(salt)));
                }

                if (memorySize < 8)
                    memorySize = 8;

                if (iterations < 1)
                    iterations = 1;

                if (degreeOfParallelism < 1)
                    degreeOfParallelism = 1;

                byte[] associatedDataBytes;
                try
                {
                    associatedDataBytes = Convert.FromBase64String(associatedData);
                }
                catch (FormatException)
                {
                    associatedDataBytes = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(associatedData)));
                }

                byte[] knownSecretBytes;
                try
                {
                    knownSecretBytes = Convert.FromBase64String(knownSecret);
                }
                catch (FormatException)
                {
                    knownSecretBytes = Convert.FromBase64String(Convert.ToBase64String(TextEncoding.GetBytes(knownSecret)));
                }

                Konscious.Security.Cryptography.Argon2id argon2Service = new Konscious.Security.Cryptography.Argon2id(data)
                {
                    Salt = hashSalt,
                    DegreeOfParallelism = degreeOfParallelism,
                    Iterations = iterations,
                    MemorySize = memorySize,
                    AssociatedData = associatedDataBytes,
                    KnownSecret = knownSecretBytes
                };

                byte[] hashBytes = argon2Service.GetBytes(FixedHashLength ? HashLength : hashSalt.Length);

                StringBuilder hashText = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; ++i)
                {
                    hashText.Append(hashBytes[i].ToString("x2"));
                }

                string hashString = $"${Enum.GetName(typeof(Argon2Type), Argon2Type.Argon2id)?.ToLower()}$v=19$m={memorySize},t={iterations},p={degreeOfParallelism}${Base64.RemovePadding(Convert.ToBase64String(hashSalt))}${Base64.RemovePadding(Convert.ToBase64String(TextEncoding.GetBytes(hashText.ToString())))}";

                return hashString;
            }

            public bool VerifyHash(string plainText, string hashText)
            {
                return VerifyHash(plainText, hashText, AssociatedData,
                    KnownSecret);
            }
            public bool VerifyHash(string plainText, string hashText, byte[] associatedData)
            {
                return VerifyHash(plainText, hashText, Convert.ToBase64String(associatedData),
                    KnownSecret);
            }
            public bool VerifyHash(string plainText, string hashText, string associatedData)
            {
                return VerifyHash(plainText, hashText, associatedData,
                    KnownSecret);
            }
            public bool VerifyHash(string plainText, string hashText, byte[] associatedData, byte[] knownSecret)
            {
                return VerifyHash(plainText, hashText, Convert.ToBase64String(associatedData),
                    Convert.ToBase64String(knownSecret));
            }
            public bool VerifyHash(string plainText, string hashText, string associatedData, string knownSecret)
            {
                Argon2Metadata metadata = GetMetadata(hashText);

                var plainTextHash = GetHash(plainText, metadata.Salt, metadata.MemorySize, metadata.Iterations,
                    metadata.DegreeOfParallelism, associatedData, knownSecret);

                return Utils.SecureEquals(plainTextHash, hashText, true, true);
            }

            public bool VerifyAndUpdateHash(string plainText, string hashText, out bool isUpdated, out string newHashText)
            {
                return VerifyAndUpdateHash(plainText, hashText, MemorySize, Iterations,
                    DegreeOfParallelism, AssociatedData, KnownSecret, out isUpdated, out newHashText);
            }
            public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
                int newDegreeOfParallelism, out bool isUpdated, out string newHashText)
            {
                return VerifyAndUpdateHash(plainText, hashText, newMemorySize, newIterations,
                    newDegreeOfParallelism, AssociatedData, KnownSecret, out isUpdated, out newHashText);
            }
            public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
                int newDegreeOfParallelism, byte[] associatedData, out bool isUpdated, out string newHashText)
            {
                return VerifyAndUpdateHash(plainText, hashText, newMemorySize, newIterations,
                    newDegreeOfParallelism, Convert.ToBase64String(associatedData), KnownSecret,
                    out isUpdated, out newHashText);
            }
            public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
                int newDegreeOfParallelism, string associatedData, out bool isUpdated, out string newHashText)
            {
                return VerifyAndUpdateHash(plainText, hashText, newMemorySize, newIterations,
                    newDegreeOfParallelism, associatedData, KnownSecret, out isUpdated, out newHashText);
            }
            public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
                int newDegreeOfParallelism, byte[] associatedData, byte[] knownSecret, out bool isUpdated, out string newHashText)
            {
                return VerifyAndUpdateHash(plainText, hashText, newMemorySize, newIterations,
                    newDegreeOfParallelism, Convert.ToBase64String(associatedData), Convert.ToBase64String(knownSecret),
                    out isUpdated, out newHashText);
            }
            public bool VerifyAndUpdateHash(string plainText, string hashText, int newMemorySize, int newIterations,
                int newDegreeOfParallelism, string associatedData, string knownSecret, out bool isUpdated, out string newHashText)
            {
                bool result;
                isUpdated = false;
                newHashText = hashText;

                result = VerifyHash(plainText, hashText, associatedData, knownSecret);

                if (!result)
                    return false;

                Argon2Metadata metadata = GetMetadata(hashText);

                isUpdated = metadata.MemorySize != newMemorySize || metadata.Iterations != newIterations || metadata.DegreeOfParallelism != newDegreeOfParallelism;

                if (isUpdated)
                    newHashText = GetHash(plainText, newMemorySize, newIterations, newDegreeOfParallelism, associatedData, knownSecret);
                else
                    newHashText = hashText;

                return true;
            }
        }
    }
}
