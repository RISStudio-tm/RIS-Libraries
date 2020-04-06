using System;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Xml;
using RIS.Configuration;

namespace RIS
{
    public static class Environment
    {
        public static event RMessageHandler ShowMessage;
        public static event RErrorHandler ShowError;

        private static uint _GCLOHThresholdSize;
        public static uint GCLOHThresholdSize
        {
            get
            {
                return _GCLOHThresholdSize;
            }

            private set
            {
                if (value > 16)
                    _GCLOHThresholdSize = value - 16;
                else if (value > 1)
                    _GCLOHThresholdSize = value - 1;
                else
                    _GCLOHThresholdSize = 0;
            }
        }

        static Environment()
        {
            GCLOHThresholdSize = 85000;

            if (AppConfig.ConfigIsLoaded)
            {
                if (AppConfig.Elements.ContainsKey("GCLOHThreshold"))
                {
                    XmlNode node = AppConfig.GetXmlElement(AppConfig.Elements["GCLOHThreshold"]);

                    if (node?.Attributes?["enabled"] != null)
                        GCLOHThresholdSize = Convert.ToUInt32(node.Attributes["enabled"].Value);
                }
            }
        }

        public static void SetGCLOHThresholdSize(uint sizeInBytes)
        {
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
                            node.Attributes["enabled"].Value = GCLOHThresholdSize.ToString();

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
                            node.Attributes["enabled"].Value = GCLOHThresholdSize.ToString();

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
