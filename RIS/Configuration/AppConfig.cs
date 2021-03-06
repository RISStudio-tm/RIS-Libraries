﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

#if NETFRAMEWORK

using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace RIS.Configuration
{
    public static class AppConfig
    {
        public static event EventHandler<RInformationEventArgs> Information;
        public static event EventHandler<RWarningEventArgs> Warning;
        public static event EventHandler<RErrorEventArgs> Error;

        private static object ReadWriteLockObj { get; }
        private static object ConfigWatcherLockObj { get; }
        private static FileSystemWatcher ConfigWatcher { get; set; }
        public static System.Configuration.Configuration Configuration { get; private set; }
        public static XmlDocument Config { get; private set; }
        public static AppConfigElementList Elements { get; }
        public static bool ConfigurationIsLoaded { get; private set; }
        public static bool ConfigIsLoaded { get; private set; }

        static AppConfig()
        {
            ReadWriteLockObj = new object();
            ConfigWatcherLockObj = new object();

            try
            {
                Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                ConfigurationIsLoaded = true;
            }
            catch (Exception)
            {
                ConfigurationIsLoaded = false;
                return;
            }

            try
            {
                Config = new XmlDocument();
                Elements = new AppConfigElementList(false);

                ReadConfig();

                ConfigIsLoaded = true;
            }
            catch (Exception)
            {
                ConfigIsLoaded = false;
                return;
            }

            ConfigWatcher = new FileSystemWatcher(Path.GetDirectoryName(Configuration.FilePath), Path.GetFileName(Configuration.FilePath));
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
                Config.Load(Configuration.FilePath);
                ReadChildNodes(Config.DocumentElement, "/");
            }
        }

        private static void ReadChildNodes(XmlNode rootNode, string xmlPath)
        {
            string currentXmlPath = $"{xmlPath}/{rootNode.Name}";

            foreach (XmlNode node in rootNode.ChildNodes)
            {
                string nodeXmlPath = $"{currentXmlPath}/{node.Name}";

                if (nodeXmlPath == "//configuration/system.runtime.caching"
                    || nodeXmlPath == "//configuration/system.runtime.remoting"
                    || nodeXmlPath == "//configuration/system.web"
                    || nodeXmlPath == "//configuration/system.web.extensions"
                    || nodeXmlPath == "//configuration/system.identityModel"
                    || nodeXmlPath == "//configuration/location"
                    || nodeXmlPath == "//configuration/connectionStrings"
                    || nodeXmlPath == "//configuration/System.Windows.Forms.ApplicationConfigurationSection"
                    || nodeXmlPath == "//configuration/appSettings"
                    || nodeXmlPath == "//configuration/applicationSettings"
                    || nodeXmlPath == "//configuration/userSettings"
                    || nodeXmlPath == "//configuration/system.codedom"
                    || nodeXmlPath == "//configuration/system.diagnostics"
                    || nodeXmlPath == "//configuration/configSections"
                    || nodeXmlPath == "//configuration/mscorlib"
                    || nodeXmlPath == "//configuration/system.net/connectionManagement"
                    || nodeXmlPath == "//configuration/system.net/authenticationModules"
                    || nodeXmlPath == "//configuration/system.net/webRequestModules"
                    || nodeXmlPath == "//configuration/runtime/assemblyBinding")
                {
                    continue;
                }

                //if (node.HasChildNodes)
                //{
                //    ReadChildNodes(node, currentXmlPath);
                //    continue;
                //}

                Elements.Add(node.Name, new AppConfigElement(node.Name, nodeXmlPath));

                if (node.HasChildNodes)
                    ReadChildNodes(node, currentXmlPath);
            }
        }

        public static void SaveConfig()
        {
            if (!ConfigIsLoaded)
            {
                var exception = new ConfigurationErrorsException("Не удалось сохранить файл конфигурации, так как он не загружен");
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

                        Config.Save(Configuration.FilePath);

                        StartConfigWatcher();
                    }
                }
            }
            catch (Exception)
            {
                var exception = new ConfigurationErrorsException("Не удалось сохранить файл конфигурации");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
        }

        public static XmlNode GetXmlElement(AppConfigElement element)
        {
            XmlNode node;

            try
            {
                node = Config.SelectSingleNode(element.XmlPath);

                if (node == null)
                    throw new XmlException();
            }
            catch (Exception)
            {
                var exception = new XmlException("Не удалось найти указанный xml элемент");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return node;
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
