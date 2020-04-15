using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using Newtonsoft.Json.Linq;
using RIS.Configuration;

namespace RIS
{
    public static class Environment
    {
        public static event RMessageHandler ShowMessage;
        public static event RErrorHandler ShowError;

        public static string ExecAppDirectoryName { get; private set; }
        public static string ExecAppFileName { get; private set; }
        public static string ExecAppFileNameWithoutExtension { get; private set; }
        

        private static uint _originalGCLOHThresholdSize;
        private static uint _modifiedGCLOHThresholdSize;
        public static uint GCLOHThresholdSize
        {
            get
            {
                return _modifiedGCLOHThresholdSize;
            }

            private set
            {
                _originalGCLOHThresholdSize = value;

                if (value > 16)
                    _modifiedGCLOHThresholdSize = value - 16;
                else if (value > 1)
                    _modifiedGCLOHThresholdSize = value - 1;
                else
                    _modifiedGCLOHThresholdSize = 0;
            }
        }

        static Environment()
        {
            System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ExecAppDirectoryName = Path.GetDirectoryName(configuration.FilePath);
            ExecAppFileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(configuration.FilePath)) + ".exe";
            ExecAppFileNameWithoutExtension = Path.GetFileNameWithoutExtension(ExecAppFileName);

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

        public static void SetGCLOHThresholdSize(uint sizeInBytes)
        {
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
                        Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                        ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                        throw exception;
                    }
                    catch (Exception ex)
                    {
                        if (node?.Attributes?["enabled"] != null)
                            node.Attributes["enabled"].Value = _originalGCLOHThresholdSize.ToString();

                        Events.DShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                        ShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                        throw;
                    }
                }
                else
                {
                    var exception =
                        new ConfigurationErrorsException(
                            "Не удалось изменить значение параметра 'GCLOHThreshold' в AppConfig. Параметр не найден в файле конфигурации");
                    Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }
            }
            else
            {
                var exception =
                    new ConfigurationErrorsException(
                        "Не удалось изменить значение параметра 'GCLOHThreshold' в AppConfig. Файл конфигурации не загружен");
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
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
                        Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                        ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                        throw exception;
                    }
                    catch (Exception ex)
                    {
                        JValue value = new JValue(JValue.CreateString(_originalGCLOHThresholdSize.ToString()));
                        token?.Replace(value);

                        Events.DShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                        ShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                        throw;
                    }
                }
                else
                {
                    var exception =
                        new ConfigurationErrorsException(
                            "Не удалось изменить значение параметра 'System.GC.LOHThreshold' в RuntimeConfig. Параметр не найден в файле конфигурации");
                    Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }
            }
            else
            {
                var exception =
                    new ConfigurationErrorsException(
                        "Не удалось изменить значение параметра 'System.GC.LOHThreshold' в RuntimeConfig. Файл конфигурации не загружен");
                Events.DShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

#endif
        }

        public static uint GetSize<T>()
        {
            Type type = typeof(T);
            int size;

            try
            {
                if (type.IsValueType)
                {
                    if (type.IsGenericType)
                    {
                        var defaultValue = default(T);
                        size = Marshal.SizeOf(defaultValue);
                    }
                    else
                    {
                        size = Marshal.SizeOf(type);
                    }
                }
                else
                {
                    size = IntPtr.Size;
                }
            }
            catch (Exception ex)
            {
                Events.DShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }

            return (uint)size;
        }
        public static uint GetSize(Type type)
        {
            int size;

            try
            {
                if (type.IsValueType)
                {
                    if (type.IsGenericType)
                    {
                        var defaultValue = Activator.CreateInstance(type);
                        size = Marshal.SizeOf(defaultValue);
                    }
                    else
                    {
                        size = Marshal.SizeOf(type);
                    }
                }
                else
                {
                    size = IntPtr.Size;
                }
            }
            catch (Exception ex)
            {
                Events.DShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                ShowError?.Invoke(null, new RErrorEventArgs(ex.Message, ex.StackTrace));
                throw;
            }

            return (uint)size;
        }
    }
}
