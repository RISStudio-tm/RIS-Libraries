// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

#if NETCOREAPP

using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RIS.Configuration
{
    public static class RuntimeConfig
    {
        public static event EventHandler<RInformationEventArgs> Information;
        public static event EventHandler<RWarningEventArgs> Warning;
        public static event EventHandler<RErrorEventArgs> Error;

        private static object ReadWriteLockObj { get; }
        private static object ConfigWatcherLockObj { get; }
        private static FileSystemWatcher ConfigWatcher { get; set; }
        public static JObject Config { get; private set; }
        public static string ConfigPath { get; }
        public static RuntimeConfigElementList Elements { get; }
        public static bool ConfigIsLoaded { get; private set; }

        static RuntimeConfig()
        {
            ReadWriteLockObj = new object();
            ConfigWatcherLockObj = new object();

            ConfigPath = Path.Combine(
                Environment.ExecAppDirectoryName,
                $"{Environment.ExecAppFileNameWithoutExtension}.runtimeconfig.json");

            if (!File.Exists(ConfigPath))
            {
                ConfigIsLoaded = false;
                return;
            }

            try
            {
                Config = new JObject();
                Elements = new RuntimeConfigElementList(false);

                ReadConfig();

                ConfigIsLoaded = true;
            }
            catch (Exception)
            {
                ConfigIsLoaded = false;
                return;
            }

            ConfigWatcher = new FileSystemWatcher(
                Path.GetDirectoryName(ConfigPath),
                Path.GetFileName(ConfigPath));
            ConfigWatcher.IncludeSubdirectories = false;
            ConfigWatcher.NotifyFilter = NotifyFilters.LastWrite;
            ConfigWatcher.Changed += ConfigWatcher_Changed;

            StartConfigWatcher();
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

        private static void StartConfigWatcher()
        {
            if (ConfigIsLoaded)
                ConfigWatcher.EnableRaisingEvents = true;
        }

        private static void StopConfigWatcher()
        {
            if (ConfigIsLoaded)
                ConfigWatcher.EnableRaisingEvents = false;
        }

        private static void ReadConfig()
        {
            lock (ReadWriteLockObj)
            {
                using (var reader = File.OpenText(ConfigPath))
                {
                    Config = (JObject)JToken.ReadFrom(
                        new JsonTextReader(reader));
                }

                ReadChildNodes(Config.Root);
            }
        }

        private static void ReadChildNodes(JToken rootToken)
        {
            foreach (var token in rootToken.Children())
            {
                var tokenJsonPath = $"${token.Path}";
                string tokenName;

                if (tokenJsonPath[^1] == ']')
                {
                    tokenName = tokenJsonPath.Substring(
                        tokenJsonPath.LastIndexOf('[') + 2,
                        tokenJsonPath.Length - tokenJsonPath.LastIndexOf('[') - 4);
                }
                else
                {
                    tokenName = tokenJsonPath.Substring(
                        tokenJsonPath.LastIndexOfAny(new[] { '.', '$' }) + 1);
                }

                if (token.Type == JTokenType.Property)
                {
                    Elements.Add(tokenName, new RuntimeConfigElement(tokenName, tokenJsonPath));

                    if (token.HasValues)
                        ReadChildNodes(token);
                }

                if (token.Type == JTokenType.Object)
                {
                    //Elements.Add(tokenName, new RuntimeConfigElement(tokenName, tokenJsonPath));

                    if (token.HasValues)
                        ReadChildNodes(token);
                }
            }
        }

        public static void SaveConfig()
        {
            if (!ConfigIsLoaded)
            {
                var exception = new Exception("Не удалось сохранить файл конфигурации, так как он не загружен");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            try
            {
                lock (ConfigWatcherLockObj)
                {
                    lock (ReadWriteLockObj)
                    {
                        StopConfigWatcher();

                        using (var writer = File.CreateText(ConfigPath))
                        {
                            writer.Write(
                                Config.ToString(Formatting.Indented));
                        }

                        StartConfigWatcher();
                    }
                }
            }
            catch (Exception)
            {
                var exception = new Exception("Не удалось сохранить файл конфигурации");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
        }

        public static JToken GetJsonElement(RuntimeConfigElement element)
        {
            JToken token;

            try
            {
                token = Config.SelectToken(
                    element.JsonPath[1..], false);

                if (token == null)
                    throw new JsonException();
            }
            catch (Exception)
            {
                var exception = new JsonException("Не удалось найти указанный json элемент");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return token;
        }

        private static void ConfigWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                lock (ConfigWatcherLockObj)
                {
                    StopConfigWatcher();

                    Task.Delay(3000).Wait();

                    Elements.Clear();
                    ReadConfig();

                    StartConfigWatcher();
                }
            });
        }
    }
}

#endif
