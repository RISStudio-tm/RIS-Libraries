using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RIS.Configuration
{

#if NETCOREAPP

    public static class RuntimeConfig
    {
        public static event EventHandler<RMessageEventArgs> ShowMessage;
        public static event EventHandler<RErrorEventArgs> ShowError;

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

            ConfigPath = Path.Combine(Environment.ExecAppDirectoryName, $"{Environment.ExecAppFileNameWithoutExtension}.runtimeconfig.json");

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

            ConfigWatcher = new FileSystemWatcher(Path.GetDirectoryName(ConfigPath), Path.GetFileName(ConfigPath));
            ConfigWatcher.IncludeSubdirectories = false;
            ConfigWatcher.NotifyFilter = NotifyFilters.LastWrite;
            ConfigWatcher.Changed += ConfigWatcher_Changed;

            StartConfigWatcher();
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
                using (StreamReader reader = File.OpenText(ConfigPath))
                {
                    Config = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                }

                ReadChildNodes(Config.Root);
            }
        }

        private static void ReadChildNodes(JToken rootToken)
        {
            foreach (JToken token in rootToken.Children())
            {
                string tokenJsonPath = $"${token.Path}";
                string tokenName;

                if (tokenJsonPath[tokenJsonPath.Length - 1] == ']')
                    tokenName = tokenJsonPath.Substring(tokenJsonPath.LastIndexOf('[') + 2, tokenJsonPath.Length - tokenJsonPath.LastIndexOf('[') - 4);
                else
                    tokenName = tokenJsonPath.Substring(tokenJsonPath.LastIndexOfAny(new[] { '.', '$' }) + 1);

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
                var exception = new ConfigurationErrorsException("Не удалось сохранить файл конфигурации, так как он не загружен");
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            try
            {
                lock (ConfigWatcherLockObj)
                {
                    lock (ReadWriteLockObj)
                    {
                        StopConfigWatcher();

                        using (StreamWriter writer = File.CreateText(ConfigPath))
                        {
                            Config.Root.WriteTo(new JsonTextWriter(writer));
                        }

                        StartConfigWatcher();
                    }
                }
            }
            catch (Exception)
            {
                var exception = new ConfigurationErrorsException("Не удалось сохранить файл конфигурации");
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
        }
        
        public static JToken GetJsonElement(RuntimeConfigElement element)
        {
            JToken token;

            try
            {
                token = Config.SelectToken(element.JsonPath.Substring(1), false);

                if (token == null)
                    throw new JsonException();
            }
            catch (Exception)
            {
                var exception = new JsonException("Не удалось найти указанный json элемент");
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
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

#endif

}
