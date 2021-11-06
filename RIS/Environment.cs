// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
#if NETFRAMEWORK
using System.Xml;
#elif NETCOREAPP
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif
using RIS.Configuration;

namespace RIS
{
    public static class Environment
    {
        public static event EventHandler<RInformationEventArgs> Information;
        public static event EventHandler<RWarningEventArgs> Warning;
        public static event EventHandler<RErrorEventArgs> Error;

        public static string ExecAppDirectoryName { get; }
        public static string ExecAppFilePath { get; }
        public static string ExecAppFileName { get; }
        public static string ExecAppFileNameWithoutExtension { get; }
        public static string ExecAppAssemblyFilePath { get; }
        public static string ExecAppAssemblyFileName { get; }
        public static string ExecAppAssemblyFileNameWithoutExtension { get; }
        public static Process Process { get; }
        public static string ExecProcessDirectoryName { get; }
        public static string ExecProcessFilePath { get; }
        public static string ExecProcessFileName { get; }
        public static string ExecProcessFileNameWithoutExtension { get; }
        public static string ExecProcessAssemblyFilePath { get; }
        public static string ExecProcessAssemblyFileName { get; }
        public static string ExecProcessAssemblyFileNameWithoutExtension { get; }
        public static bool IsStandalone { get; }
        public static bool IsSingleFile { get; }
#if NETCOREAPP
        public static string RuntimeName { get; }
        public static string RuntimeVersion { get; }
        public static string RuntimeIdentifier { get; }
#endif
        public static int PlatformWordSize { get; }
        public static int PlatformWordSizeBits { get; }

        private static uint _originalGCLOHThresholdSize;
        private static uint _modifiedGCLOHThresholdSize;
        public static uint OriginalGCLOHThresholdSize
        {
            get
            {
                return _originalGCLOHThresholdSize;
            }
        }
        public static uint GCLOHThresholdSize
        {
            get
            {
                return _modifiedGCLOHThresholdSize;
            }
            private set
            {
                if (value < 85000)
                    value = 85000;

                _originalGCLOHThresholdSize = value;

                if (value % 4 != 0)
                    value -= value % 4;

                _modifiedGCLOHThresholdSize = value - 512;
            }
        }

        static Environment()
        {
            Assembly execAppAssembly = Assembly.GetEntryAssembly();

            ExecAppDirectoryName = ValidateDirectoryPath(
                Path.GetDirectoryName(execAppAssembly?.Location));
            ExecAppFilePath = ValidateFilePath(
                Path.ChangeExtension(execAppAssembly?.Location, "exe"));
            ExecAppFileName = ValidateFileName(
                Path.ChangeExtension(Path.GetFileName(ExecAppFilePath), "exe"));
            ExecAppFileNameWithoutExtension = ValidateFileName(
                Path.GetFileNameWithoutExtension(ExecAppFileName));
            ExecAppAssemblyFilePath = ValidateFilePath(
                Path.ChangeExtension(execAppAssembly?.Location, "dll"));
            ExecAppAssemblyFileName = ValidateFileName(
                Path.ChangeExtension(Path.GetFileName(ExecAppAssemblyFilePath), "dll"));
            ExecAppAssemblyFileNameWithoutExtension = ValidateFileName(
                Path.GetFileNameWithoutExtension(ExecAppAssemblyFileName));

            Process = Process.GetCurrentProcess();

            ExecProcessDirectoryName = ValidateDirectoryPath(
                Path.GetDirectoryName(Process.MainModule?.FileName));
            ExecProcessFilePath = ValidateFilePath(
                Path.ChangeExtension(Process.MainModule?.FileName, "exe"));
            ExecProcessFileName = ValidateFileName(
                Path.ChangeExtension(Path.GetFileName(ExecProcessFilePath), "exe"));
            ExecProcessFileNameWithoutExtension = ValidateFileName(
                Path.GetFileNameWithoutExtension(ExecProcessFileName));
            ExecProcessAssemblyFilePath = ValidateFilePath(
                Path.ChangeExtension(Process.MainModule?.FileName, "dll"));
            ExecProcessAssemblyFileName = ValidateFileName(
                Path.ChangeExtension(Path.GetFileName(ExecProcessAssemblyFilePath), "dll"));
            ExecProcessAssemblyFileNameWithoutExtension = ValidateFileName(
                Path.GetFileNameWithoutExtension(ExecProcessAssemblyFileName));

#if NETFRAMEWORK

            IsStandalone = false;

#elif NETCOREAPP

            if (File.Exists(Path.Combine(ExecAppDirectoryName, "hostfxr.dll"))
                && File.Exists(Path.Combine(ExecAppDirectoryName, "hostpolicy.dll")))
            {
                IsStandalone = true;
            }

#endif

#if NETFRAMEWORK

            IsSingleFile = false;

#elif NETCOREAPP

            if (!File.Exists(ExecProcessAssemblyFilePath))
                IsSingleFile = true;

#endif

#if NETCOREAPP

            (RuntimeName, RuntimeVersion, RuntimeIdentifier) = GetRuntimeInfo();

#endif

            PlatformWordSize = IntPtr.Size;
            PlatformWordSizeBits = PlatformWordSize * 8;

            GCLOHThresholdSize = 85000;

#if NETFRAMEWORK

            if (AppConfig.ConfigIsLoaded)
            {
                if (AppConfig.Elements.ContainsKey("GCLOHThreshold"))
                {
                    XmlNode node = AppConfig.GetXmlElement(AppConfig.Elements["GCLOHThreshold"]);

                    if (node?.Attributes?["enabled"] != null)
                        GCLOHThresholdSize = Convert.ToUInt32(node.Attributes["enabled"].Value);
                }
            }

#elif NETCOREAPP

            if (RuntimeConfig.ConfigIsLoaded)
            {
                if (RuntimeConfig.Elements.ContainsKey("System.GC.LOHThreshold"))
                {
                    JToken token = RuntimeConfig.GetJsonElement(RuntimeConfig.Elements["System.GC.LOHThreshold"]);

                    if (token?.HasValues == false)
                    {
                        JTokenReader reader = new JTokenReader(token);

                        decimal? value = reader.ReadAsDecimal();

                        reader.Close();

                        if (value.HasValue)
                            GCLOHThresholdSize = Convert.ToUInt32(value.Value);
                    }
                }
            }

#endif

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

        private static string ValidateFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return "Unknown";

            return path;
        }

        private static string ValidateDirectoryPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return "Unknown";

            return path;
        }

        private static string ValidateFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)
                || Path.GetFileNameWithoutExtension(name) == "Unknown")
            {
                return "Unknown";
            }

            return name;
        }

#if NETCOREAPP

        private static (string RuntimeName, string RuntimeVersion, string RuntimeIdentifier) GetRuntimeInfo()
        {
            string path = Path.Combine(ExecAppDirectoryName,
                $"{ExecAppAssemblyFileNameWithoutExtension}.deps.json");

            if (!File.Exists(path))
                return ("unknown", "unknown", "unknown");

            JObject config;

            using (StreamReader reader = File.OpenText(path))
            {
                config = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
            }

            foreach (JToken token in config.Root.Children())
            {
                string tokenJsonPath = $"${token.Path}";
                string tokenName;

                if (tokenJsonPath[tokenJsonPath.Length - 1] == ']')
                {
                    tokenName = tokenJsonPath.Substring(tokenJsonPath.LastIndexOf('[') + 2,
                        tokenJsonPath.Length - tokenJsonPath.LastIndexOf('[') - 4);
                }
                else
                {
                    tokenName = tokenJsonPath.Substring(tokenJsonPath.LastIndexOfAny(new[] { '.', '$' }) + 1);
                }

                if (tokenName != "runtimeTarget")
                    continue;

                JToken runtimeFullName = token.First?.Value<string>("name");

                if (runtimeFullName == null)
                    return ("unknown", "unknown", "unknown");

                string[] runtimeFullNameComponents = runtimeFullName.Value<string>()?.Split('/');
                string[] runtimeNameComponents = runtimeFullNameComponents?[0].Split(',');

                string runtimeName = runtimeNameComponents?[0];
                string runtimeVersion = runtimeNameComponents?[1].Substring(8);

                if (runtimeFullNameComponents?.Length < 2)
                    return (runtimeName, runtimeVersion, "any");

                string runtimeIdentifier = runtimeFullNameComponents?[1];

                return (runtimeName, runtimeVersion, runtimeIdentifier);
            }

            return ("unknown", "unknown", "unknown");
        }

#endif

        public static void SetGCLOHThresholdSize(uint sizeInBytes)
        {
            if (sizeInBytes < 85000)
                sizeInBytes = 85000;

#if NETFRAMEWORK

            if (AppConfig.ConfigIsLoaded)
            {
                if (AppConfig.Elements.ContainsKey("GCLOHThreshold"))
                {
                    XmlNode node = AppConfig.GetXmlElement(AppConfig.Elements["GCLOHThreshold"]);

                    if (node?.Attributes?["enabled"] != null)
                        node.Attributes["enabled"].Value = sizeInBytes.ToString();

                    try
                    {
                        AppConfig.SaveConfig();
                    }
                    catch (ConfigurationErrorsException)
                    {
                        if (node?.Attributes?["enabled"] != null)
                            node.Attributes["enabled"].Value = _originalGCLOHThresholdSize.ToString();

                        var exception =
                            new ConfigurationErrorsException(
                                "Не удалось изменить значение параметра 'GCLOHThreshold' в AppConfig. Ошибка сохранения файла конфигурации");
                        Events.OnError(new RErrorEventArgs(exception, exception.Message));
                        OnError(new RErrorEventArgs(exception, exception.Message));
                        throw exception;
                    }
                    catch (Exception ex)
                    {
                        if (node?.Attributes?["enabled"] != null)
                            node.Attributes["enabled"].Value = _originalGCLOHThresholdSize.ToString();

                        Events.OnError(new RErrorEventArgs(ex, ex.Message));
                        OnError(new RErrorEventArgs(ex, ex.Message));
                        throw;
                    }
                }
                else
                {
                    var exception =
                        new ConfigurationErrorsException(
                            "Не удалось изменить значение параметра 'GCLOHThreshold' в AppConfig. Параметр не найден в файле конфигурации");
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }
            }
            else
            {
                var exception =
                    new ConfigurationErrorsException(
                        "Не удалось изменить значение параметра 'GCLOHThreshold' в AppConfig. Файл конфигурации не загружен");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

#elif NETCOREAPP

            if (RuntimeConfig.ConfigIsLoaded)
            {
                if (RuntimeConfig.Elements.ContainsKey("System.GC.LOHThreshold"))
                {
                    JToken token = RuntimeConfig.GetJsonElement(RuntimeConfig.Elements["System.GC.LOHThreshold"]);

                    if (token?.HasValues == false)
                    {
                        JValue value = new JValue(JValue.CreateString(sizeInBytes.ToString()));
                        token.Replace(value);
                    }

                    try
                    {
                        RuntimeConfig.SaveConfig();
                    }
                    catch (ConfigurationErrorsException)
                    {
                        JValue value = new JValue(JValue.CreateString(_originalGCLOHThresholdSize.ToString()));
                        token?.Replace(value);

                        var exception =
                            new ConfigurationErrorsException(
                                "Не удалось изменить значение параметра 'System.GC.LOHThreshold' в RuntimeConfig. Ошибка сохранения файла конфигурации");
                        Events.OnError(new RErrorEventArgs(exception, exception.Message));
                        OnError(new RErrorEventArgs(exception, exception.Message));
                        throw exception;
                    }
                    catch (Exception ex)
                    {
                        JValue value = new JValue(JValue.CreateString(_originalGCLOHThresholdSize.ToString()));
                        token?.Replace(value);

                        Events.OnError(new RErrorEventArgs(ex, ex.Message));
                        OnError(new RErrorEventArgs(ex, ex.Message));
                        throw;
                    }
                }
                else
                {
                    var exception =
                        new ConfigurationErrorsException(
                            "Не удалось изменить значение параметра 'System.GC.LOHThreshold' в RuntimeConfig. Параметр не найден в файле конфигурации");
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }
            }
            else
            {
                var exception =
                    new ConfigurationErrorsException(
                        "Не удалось изменить значение параметра 'System.GC.LOHThreshold' в RuntimeConfig. Файл конфигурации не загружен");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

#endif
        }

        public static int GetSize<T>()
        {
            var type = typeof(T);

            try
            {
                return type.IsValueType
                    ? Unsafe.SizeOf<T>()
                    : IntPtr.Size;
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }

        // ReSharper disable AssignNullToNotNullAttribute
        public static int GetSize(Type type)
        {
            try
            {
                return type.IsValueType
                    ? Marshal.SizeOf(Activator.CreateInstance(
                        type, true))
                    : IntPtr.Size;
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex, ex.Message));
                OnError(new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }
        // ReSharper restore AssignNullToNotNullAttribute


        public static byte ReflectBits(byte value)
        {
            byte valueReflected = 0;

            for (int i = 0; i < 8; ++i)
            {
                if ((value & (1 << i)) != 0)
                    valueReflected |= (byte)(1 << (7 - i));
            }

            return valueReflected;
        }
        public static ushort ReflectBits(ushort value)
        {
            ushort valueReflected = 0;

            for (int i = 0; i < 16; ++i)
            {
                if ((value & (1 << i)) != 0)
                    valueReflected |= (ushort)(1 << (15 - i));
            }

            return valueReflected;
        }
        public static uint ReflectBits(uint value)
        {
            uint valueReflected = 0;

            for (int i = 0; i < 32; ++i)
            {
                if ((value & (1 << i)) != 0)
                    valueReflected |= (uint)(1 << (31 - i));
            }

            return valueReflected;
        }
        public static ulong ReflectBits(ulong value)
        {
            ulong valueReflected = 0;

            for (int i = 0; i < 64; ++i)
            {
                if ((value & ((ulong) 1 << i)) != 0)
                    valueReflected |= ((ulong) 1 << (63 - i));
            }

            return valueReflected;
        }
        public static byte[] ReflectBits(byte[] value)
        {
            for (int i = 0; i < value.Length; ++i)
            {
                ref byte valueByte = ref value[i];
                valueByte = ReflectBits(valueByte);
            }

            return value;
        }
        public static Span<byte> ReflectBits(Span<byte> value)
        {
            for (int i = 0; i < value.Length; ++i)
            {
                ref byte valueByte = ref value[i];
                valueByte = ReflectBits(valueByte);
            }

            return value;
        }

        public static TimeSpan MeasureActionTime(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();

            return stopwatch.Elapsed;
        }
        public static async Task<TimeSpan> MeasureActionTime(Func<Task> function)
        {
            var stopwatch = Stopwatch.StartNew();
            await function().ConfigureAwait(false);
            stopwatch.Stop();

            return stopwatch.Elapsed;
        }
    }
}
