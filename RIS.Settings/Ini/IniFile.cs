// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace RIS.Settings.Ini
{
    public sealed class IniFile
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

        private readonly Dictionary<string, IniSection> _sections;
        private readonly IniBoolOptions _boolOptions;
        private readonly StringComparer _comparer;

        public string Directory { get; private set; }
        public string FullPath { get; private set; }
        public string Name { get; private set; }
        public string Extension { get; private set; }
        public string FullName { get; private set; }
        public Encoding Encoding { get; private set; }
        public string DefaultSectionName { get; }
        public char CommentCharacter { get; }

        public IniFile(string defaultSectionName = "General", char commentCharacter = ';',
            StringComparer comparer = null, IniBoolOptions boolOptions = null)
        {
            _comparer = comparer ?? StringComparer.OrdinalIgnoreCase;
            _boolOptions = boolOptions ?? new IniBoolOptions(true, _comparer);
            _sections = new Dictionary<string, IniSection>(_comparer);

            DefaultSectionName = string.IsNullOrWhiteSpace(defaultSectionName) ? "General" : defaultSectionName;
            CommentCharacter = commentCharacter == '\0' ? ';' : commentCharacter;
        }
        public IniFile(string path,
            string defaultSectionName = "General", char commentCharacter = ';',
            StringComparer comparer = null, IniBoolOptions boolOptions = null)
            : this(defaultSectionName, commentCharacter, comparer, boolOptions)
        {
            Load(path, DefaultEncoding);
        }
        public IniFile(string path, Encoding encoding,
            string defaultSectionName = "General", char commentCharacter = ';',
            StringComparer comparer = null, IniBoolOptions boolOptions = null)
            : this(defaultSectionName, commentCharacter, comparer, boolOptions)
        {
            Load(path, encoding);
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

        public void Create(string path)
        {
            Create(path, DefaultEncoding);
        }
        public void Create(string path, Encoding encoding)
        {
            if (path == null)
            {
                var exception = new ArgumentNullException(nameof(path), $"{nameof(path)} cannot be null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(path, true, encoding))
                {
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                Events.OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                OnError(this, new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }
        }

        public void Load(string path)
        {
            Load(path, DefaultEncoding);
        }
        public void Load(string path, Encoding encoding)
        {
            if (path == null)
            {
                var exception = new ArgumentNullException(nameof(path), $"{nameof(path)} cannot be null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            Create(path, encoding);

            if (!Path.IsPathRooted(path))
                path = Path.GetFullPath(path);

            Name = Path.GetFileNameWithoutExtension(path);
            Extension = Path.GetExtension(path);
            FullName = Path.GetFileName(path);
            Directory = Path.GetDirectoryName(path);
            FullPath = path;
            Encoding = encoding;

            using (StreamReader reader = new StreamReader(path, encoding))
            {
                Load(reader);
            }
        }
        public void Load(string path, bool detectEncodingFromByteOrderMarks)
        {
            Load(path, DefaultEncoding, detectEncodingFromByteOrderMarks);
        }
        public void Load(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
        {
            if (path == null)
            {
                var exception = new ArgumentNullException(nameof(path), $"{nameof(path)} cannot be null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            Create(path, encoding);

            if (!Path.IsPathRooted(path))
                path = Path.GetFullPath(path);

            Name = Path.GetFileNameWithoutExtension(path);
            Extension = Path.GetExtension(path);
            FullName = Path.GetFileName(path);
            Directory = Path.GetDirectoryName(path);
            FullPath = path;
            Encoding = encoding;

            using (StreamReader reader = new StreamReader(path, encoding, detectEncodingFromByteOrderMarks))
            {
                Load(reader);
            }
        }
        private void Load(StreamReader reader)
        {
            if (reader == null)
            {
                var exception = new ArgumentNullException(nameof(reader), $"{nameof(reader)} cannot be null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            Clear();

            string line;
            IniSection section = null;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.TrimStart();

                if (line.Length == 0)
                    continue;

                if (line[0] == CommentCharacter)
                    continue;

                switch (line[0])
                {
                    case '[':
                    {
                        int endBracePosition = line.IndexOf(']', 1);

                        if (endBracePosition == -1)
                            endBracePosition = line.Length;

                        string sectionName = line.Substring(1, endBracePosition - 1).Trim();

                        if (sectionName.Length > 0 && !_sections.TryGetValue(sectionName, out section))
                        {
                            section = new IniSection(sectionName, _comparer);
                            _sections.Add(section.Name, section);
                        }

                        break;
                    }
                    default:
                    {
                        string settingName;
                        string settingValue;
                        int equalSignPosition = line.IndexOf('=');

                        if (equalSignPosition == -1)
                        {
                            settingName = line.Trim();
                            settingValue = string.Empty;
                        }
                        else
                        {
                            settingName = line.Substring(0, equalSignPosition).Trim();
                            settingValue = line.Substring(equalSignPosition + 1);
                        }

                        if (settingName.Length == 0)
                            break;

                        if (section == null)
                        {
                            section = new IniSection(DefaultSectionName, _comparer);
                            _sections.Add(section.Name, section);
                        }

                        if (section.Settings.TryGetValue(settingName, out IniSetting setting))
                        {
                            setting.Value = settingValue;
                        }
                        else
                        {
                            setting = new IniSetting(settingName, settingValue);
                            section.Settings.Add(settingName, setting);
                        }

                        break;
                    }
                }
            }
        }

        public void Save()
        {
            using (StreamWriter writer = new StreamWriter(FullPath, false))
            {
                Save(writer);
            }
        }
        private void Save(StreamWriter writer)
        {
            if (writer == null)
            {
                var exception = new ArgumentNullException(nameof(writer), $"{nameof(writer)} cannot be null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            bool isFirstLine = true;

            foreach (IniSection section in _sections.Values)
            {
                if (section.Settings.Count == 0)
                    continue;

                if (isFirstLine)
                    isFirstLine = false;
                else
                    writer.WriteLine();

                writer.WriteLine($"[{section.Name}]");

                foreach (IniSetting setting in section.Settings.Values)
                {
                    writer.WriteLine(setting.ToString());
                }
            }
        }

        public IEnumerable<string> GetSections()
        {
            return _sections.Keys;
        }

        public IEnumerable<IniSetting> GetSectionSettings(string sectionName)
        {
            if (sectionName == null)
            {
                var exception = new ArgumentNullException(nameof(sectionName), $"{nameof(sectionName)} cannot be null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return _sections.TryGetValue(sectionName, out IniSection section)
                ? section.Settings.Values
                : Enumerable.Empty<IniSetting>();
        }

        public IniSection GetSection(string sectionName)
        {
            if (sectionName == null)
            {
                var exception = new ArgumentNullException(nameof(sectionName), $"{nameof(sectionName)} cannot be null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (!_sections.TryGetValue(sectionName, out IniSection section))
            {
                var exception = new ArgumentNullException(nameof(sectionName), $"Section with name [{nameof(sectionName)}] not found");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return section;
        }

        public void RemoveSection(string sectionName)
        {
            if (sectionName == null)
            {
                var exception = new ArgumentNullException(nameof(sectionName), $"{nameof(sectionName)} cannot be null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (!_sections.TryGetValue(sectionName, out IniSection _))
                return;

            _sections.Remove(sectionName);
        }

        public bool GetBoolean(string sectionName, string settingName, bool defaultValue = false)
        {
            return _boolOptions.TryParse(GetString(sectionName, settingName), out bool value)
                ? value
                : defaultValue;
        }
        public sbyte GetSbyte(string sectionName, string settingName, sbyte defaultValue = 0)
        {
            return sbyte.TryParse(GetString(sectionName, settingName), out sbyte value)
                ? value
                : defaultValue;
        }
        public byte GetByte(string sectionName, string settingName, byte defaultValue = 0)
        {
            return byte.TryParse(GetString(sectionName, settingName), out byte value)
                ? value
                : defaultValue;
        }
        public short GetShort(string sectionName, string settingName, short defaultValue = 0)
        {
            return short.TryParse(GetString(sectionName, settingName), out short value)
                ? value
                : defaultValue;
        }
        public ushort GetUShort(string sectionName, string settingName, ushort defaultValue = 0)
        {
            return ushort.TryParse(GetString(sectionName, settingName), out ushort value)
                ? value
                : defaultValue;
        }
        public int GetInt(string sectionName, string settingName, int defaultValue = 0)
        {
            return int.TryParse(GetString(sectionName, settingName), out int value)
                ? value
                : defaultValue;
        }
        public uint GetUInt(string sectionName, string settingName, uint defaultValue = 0U)
        {
            return uint.TryParse(GetString(sectionName, settingName), out uint value)
                ? value
                : defaultValue;
        }
        public long GetLong(string sectionName, string settingName, long defaultValue = 0L)
        {
            return long.TryParse(GetString(sectionName, settingName), out long value)
                ? value
                : defaultValue;
        }
        public ulong GetULong(string sectionName, string settingName, ulong defaultValue = 0UL)
        {
            return ulong.TryParse(GetString(sectionName, settingName), out ulong value)
                ? value
                : defaultValue;
        }
        public float GetFloat(string sectionName, string settingName, float defaultValue = 0.0F)
        {
            return float.TryParse(GetString(sectionName, settingName), out float value)
                ? value
                : defaultValue;
        }
        public double GetDouble(string sectionName, string settingName, double defaultValue = 0.0)
        {
            return double.TryParse(GetString(sectionName, settingName), out double value)
                ? value
                : defaultValue;
        }
        public decimal GetDecimal(string sectionName, string settingName, decimal defaultValue = decimal.Zero)
        {
            return decimal.TryParse(GetString(sectionName, settingName), out decimal value)
                ? value
                : defaultValue;
        }
        public char GetChar(string sectionName, string settingName, char defaultValue = '\0')
        {
            return char.TryParse(GetString(sectionName, settingName), out char value)
                ? value
                : defaultValue;
        }
        public string GetString(string sectionName, string settingName, string defaultValue = null)
        {
            if (sectionName == null)
            {
                var exception = new ArgumentNullException(nameof(sectionName), $"{nameof(sectionName)} cannot be null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (settingName == null)
            {
                var exception = new ArgumentNullException(nameof(settingName), $"{nameof(settingName)} cannot be null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (_sections.TryGetValue(sectionName, out IniSection section)
                && section.Settings.TryGetValue(settingName, out IniSetting setting))
            {
                return setting.Value;
            }

            return defaultValue;
        }

        public void Set(string sectionName, string settingName, bool value)
        {
            Set(sectionName, settingName, _boolOptions.ToString(value));
        }
        public void Set(string sectionName, string settingName, sbyte value)
        {
            Set(sectionName, settingName, value.ToString());
        }
        public void Set(string sectionName, string settingName, byte value)
        {
            Set(sectionName, settingName, value.ToString());
        }
        public void Set(string sectionName, string settingName, short value)
        {
            Set(sectionName, settingName, value.ToString());
        }
        public void Set(string sectionName, string settingName, ushort value)
        {
            Set(sectionName, settingName, value.ToString());
        }
        public void Set(string sectionName, string settingName, int value)
        {
            Set(sectionName, settingName, value.ToString());
        }
        public void Set(string sectionName, string settingName, uint value)
        {
            Set(sectionName, settingName, value.ToString());
        }
        public void Set(string sectionName, string settingName, long value)
        {
            Set(sectionName, settingName, value.ToString());
        }
        public void Set(string sectionName, string settingName, ulong value)
        {
            Set(sectionName, settingName, value.ToString());
        }
        public void Set(string sectionName, string settingName, float value)
        {
            Set(sectionName, settingName, value.ToString(CultureInfo.InvariantCulture));
        }
        public void Set(string sectionName, string settingName, double value)
        {
            Set(sectionName, settingName, value.ToString(CultureInfo.InvariantCulture));
        }
        public void Set(string sectionName, string settingName, decimal value)
        {
            Set(sectionName, settingName, value.ToString(CultureInfo.InvariantCulture));
        }
        public void Set(string sectionName, string settingName, char value)
        {
            Set(sectionName, settingName, value.ToString());
        }
        public void Set(string sectionName, string settingName, string value)
        {
            if (sectionName == null)
            {
                var exception = new ArgumentNullException(nameof(sectionName), $"{nameof(sectionName)} cannot be null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (settingName == null)
            {
                var exception = new ArgumentNullException(nameof(settingName), $"{nameof(settingName)} cannot be null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (!_sections.TryGetValue(sectionName, out IniSection section))
            {
                section = new IniSection(sectionName, _comparer);
                _sections.Add(section.Name, section);
            }

            if (!section.Settings.TryGetValue(settingName, out IniSetting setting))
            {
                setting = new IniSetting(settingName);
                section.Settings.Add(setting.Name, setting);
            }

            setting.Value = value ?? string.Empty;
        }

        public void Remove(string sectionName, string settingName)
        {
            if (sectionName == null)
            {
                var exception = new ArgumentNullException(nameof(sectionName), $"{nameof(sectionName)} cannot be null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (settingName == null)
            {
                var exception = new ArgumentNullException(nameof(settingName), $"{nameof(settingName)} cannot be null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (!_sections.TryGetValue(sectionName, out IniSection section)
                || !section.Settings.TryGetValue(settingName, out IniSetting _))
            {
                return;
            }

            section.Settings.Remove(settingName);
        }

        public void Clear()
        {
            _sections.Clear();
        }
    }
}
