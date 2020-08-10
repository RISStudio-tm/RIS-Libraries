using System;
using System.Text.RegularExpressions;
using RIS.Text.Encoding.Base;

namespace RIS.Cryptography.Hash
{
    public class BCryptMetadata
    {
        public static Regex HashInfoRegex { get; } = new Regex(@"^\$(?<version>2[a-z]{1}?)\$(?<work_factor>\d\d?)\$(?<hash>[A-Za-z0-9\./]{53})$", RegexOptions.Singleline);

        public event EventHandler<RInformationEventArgs> Information;
		public event EventHandler<RWarningEventArgs> Warning;
		public event EventHandler<RErrorEventArgs> Error;

        public string Version { get; private set; }
        private int _workFactor;
        public int WorkFactor
        {
            get
            {
                return _workFactor;
            }
            private set
            {
                if (value < 4)
                    value = 4;
                else if (value > 31)
                    value = 31;

                _workFactor = value;
            }
        }
        public string Hash { get; private set; }

        public BCryptMetadata(string hashText)
        {
            global::BCrypt.Net.HashInformation hashInfo = null;

            try
            {
                hashInfo = global::BCrypt.Net.BCrypt.InterrogateHash(hashText);
            }
            catch (global::BCrypt.Net.HashInformationException)
            {
                var exception = new FormatException($"Invalid hash format in metadata[{ this.GetType().FullName }]");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            Version = hashInfo.Version;
            WorkFactor = Convert.ToInt32(hashInfo.WorkFactor);
            Hash = hashInfo.RawHash;
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

        public override string ToString()
        {
            return $"${Version}${WorkFactor}${Hash}";
        }
    }

    public class Argon2Metadata
    {
        public static Regex HashInfoRegex { get; } = new Regex(@"^\$(?<type>argon2[a-z]{0,2}?)\$v=(?<version>\d+?)\$m=(?<memory_size>\d+?),t=(?<iterations>\d+?),p=(?<degree_of_parallelism>\d+?)\$(?<salt>[A-Za-z0-9/+]+)\$(?<hash>[A-Za-z0-9/+]+)$", RegexOptions.Singleline);

        public event EventHandler<RInformationEventArgs> Information;
		public event EventHandler<RWarningEventArgs> Warning;
		public event EventHandler<RErrorEventArgs> Error;

        public Argon2Type Type { get; private set; }
        public int Version { get; private set; }
        private ushort _saltLength;
        public ushort SaltLength
        {
            get
            {
                return _saltLength;
            }
            private set
            {
                if (value < 8)
                    value = 8;

                _saltLength = value;
            }
        }
        private byte[] _salt;
        public byte[] SaltBytes
        {
            get
            {
                return _salt;
            }
            private set
            {
                _salt = value;
            }
        }
        public string Salt
        {
            get
            {
                return Convert.ToBase64String(_salt);
            }
            private set
            {
                try
                {
                    _salt = Convert.FromBase64String(value);
                }
                catch (FormatException)
                {
                    _salt = Convert.FromBase64String(Convert.ToBase64String(HashMethods.TextEncoding.GetBytes(value)));
                }
            }
        }
        private byte[] _hash;
        public byte[] HashBytes
        {
            get
            {
                return _hash;
            }
            private set
            {
                _hash = value;
            }
        }
        public string Hash
        {
            get
            {
                return Convert.ToBase64String(_hash);
            }
            private set
            {
                try
                {
                    _hash = Convert.FromBase64String(value);
                }
                catch (FormatException)
                {
                    _hash = Convert.FromBase64String(Convert.ToBase64String(HashMethods.TextEncoding.GetBytes(value)));
                }
            }
        }
        private int _degreeOfParallelism;
        public int DegreeOfParallelism
        {
            get
            {
                return _degreeOfParallelism;
            }
            private set
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
            private set
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
            private set
            {
                if (value < 8)
                    value = 8;

                _memorySize = value;
            }
        }

        public Argon2Metadata(string hashText)
        {
            Match hashInfo = HashInfoRegex.Match(hashText);

            if (!hashInfo.Success)
            {
                var exception = new FormatException($"Invalid hash format in metadata[{ this.GetType().FullName }]");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            Enum.TryParse(hashInfo.Groups["type"].Value, true, out Argon2Type type);
            Type = type;
            Version = Convert.ToInt32(hashInfo.Groups["version"].Value);
            MemorySize = Convert.ToInt32(hashInfo.Groups["memory_size"].Value);
            Iterations = Convert.ToInt32(hashInfo.Groups["iterations"].Value);
            DegreeOfParallelism = Convert.ToInt32(hashInfo.Groups["degree_of_parallelism"].Value);
            Salt = Base64.RestorePadding(hashInfo.Groups["salt"].Value);
            Hash = Base64.RestorePadding(hashInfo.Groups["hash"].Value);
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

        public override string ToString()
        {
            return $"${Enum.GetName(typeof(Argon2Type), Type)?.ToLower()}$v={Version}$m={MemorySize},t={Iterations},p={DegreeOfParallelism}${Base64.RemovePadding(Salt)}${Base64.RemovePadding(Hash)}";
        }
    }
}
