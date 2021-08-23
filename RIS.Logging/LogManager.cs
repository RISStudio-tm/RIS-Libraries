// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using NLog;
using NLog.Config;
using RIS.Providers;

namespace RIS.Logging
{
    public static class LogManager
    {
        public static event EventHandler<EventArgs> LoggingStartup;
        public static event EventHandler<EventArgs> LoggingShutdown;
        public static event EventHandler<EventArgs> LoggingPaused;
        public static event EventHandler<EventArgs> LoggingResumed;


        private static readonly object DefaultSyncRoot = new object();
        private static volatile Logger _default;
        public static Logger Default
        {
            get
            {
                if (_default == null)
                {
                    lock (DefaultSyncRoot)
                    {
                        if (_default == null)
                            _default = LogFactory.GetLogger(nameof(Default));
                    }
                }

                return _default;
            }
        }
        private static readonly object DebugSyncRoot = new object();
        private static volatile Logger _debug;
        public static Logger Debug
        {
            get
            {
                if (_debug == null)
                {
                    lock (DebugSyncRoot)
                    {
                        if (_debug == null)
                            _debug = LogFactory.GetLogger(nameof(Debug));
                    }
                }

                return _debug;
            }
        }


        private static LogFactory LogFactory { get; }
        private static bool Disposed { get; set; }
        private static object AppShutdownEventSyncRoot { get; }
        private static bool AppShutdownEventSubscribed { get; set; }
        private static object RISEventsSyncRoot { get; }
        private static bool RISEventsSubscribed { get; set; }
        private static object AppDomainEventsSyncRoot { get; }
        private static bool AppDomainEventsSubscribed { get; set; }


        public static object SyncRoot { get; }
        private static int _running;
        public static bool Running
        {
            get
            {
                return _running > 0;
            }
            private set
            {
                if (Disposed)
                    return;

                Interlocked.Exchange(ref _running, value ? 1 : 0);
            }
        }
        private static object AutoShutdownSyncRoot { get; }
        private static int _autoShutdown;
        public static bool AutoShutdown
        {
            get
            {
                return _autoShutdown > 0;
            }
            set
            {
                lock (AutoShutdownSyncRoot)
                {
                    if (Disposed)
                        return;

                    var newState = value ? 1 : 0;
                    var oldState = Interlocked.Exchange(ref _autoShutdown, newState);

                    if (oldState == newState)
                        return;

                    if (value)
                        SubscribeAppShutdownEvent();
                    else
                        UnsubscribeAppShutdownEvent();
                }
            }
        }
        public static LogLevel GlobalThreshold
        {
            get
            {
                return LogFactory.GlobalThreshold;
            }
            set
            {
                LogFactory.GlobalThreshold = value;
            }
        }


        private static string AppDirectoryName
        {
            get
            {
                if (LogFactory.Configuration == null)
                    return null;

                return LogFactory.Configuration
                    .Variables[nameof(AppDirectoryName)]
                    .Text;
            }
            set
            {
                if (LogFactory.Configuration == null)
                    return;

                LogFactory.Configuration
                    .Variables[nameof(AppDirectoryName)] = value;
            }
        }
        private static string DefaultLogDirectoryPath
        {
            get
            {
                if (LogFactory.Configuration == null)
                    return null;

                return LogFactory.Configuration
                    .Variables[nameof(DefaultLogDirectoryPath)]
                    .ToString();
            }
            set
            {
                if (LogFactory.Configuration == null)
                    return;

                LogFactory.Configuration
                    .Variables[nameof(DefaultLogDirectoryPath)] = value;
            }
        }
        private static string DebugLogDirectoryPath
        {
            get
            {
                if (LogFactory.Configuration == null)
                    return null;

                return LogFactory.Configuration
                    .Variables[nameof(DebugLogDirectoryPath)]
                    .ToString();
            }
            set
            {
                if (LogFactory.Configuration == null)
                    return;

                LogFactory.Configuration
                    .Variables[nameof(DebugLogDirectoryPath)] = value;
            }
        }



#pragma warning disable SS002 // DateTime.Now was referenced
        static LogManager()
        {
            LogFactory = new LogFactory();
            Disposed = false;
            AppShutdownEventSyncRoot = new object();
            AppShutdownEventSubscribed = false;
            RISEventsSyncRoot = new object();
            RISEventsSubscribed = false;
            AppDomainEventsSyncRoot = new object();
            AppDomainEventsSubscribed = false;

            SyncRoot = new object();
            Running = false;
            AutoShutdownSyncRoot = new object();
            AutoShutdown = false;
            GlobalThreshold = LogLevel.Trace;

            DateTime startupTime;

            try
            {
                startupTime = Environment.Process.StartTime
                    .ToLocalTime();
            }
            catch (Exception)
            {
                startupTime = DateTime.Now;
            }

            GlobalDiagnosticsContext.Set(
                "RIS-AppStartupTime",
                startupTime.ToString("yyyy.MM.dd HH-mm-ss",
                    CultureInfo.InvariantCulture));

            Startup();

            AutoShutdown = true;

            SubscribeRISEvents();
            SubscribeAppDomainEvents();
        }
#pragma warning restore SS002 // DateTime.Now was referenced



        public static void Startup()
        {
            lock (SyncRoot)
            {
                if (Disposed)
                    return;

                if (Running)
                    return;

                Disposed = false;
                Running = true;

                LogFactory.Configuration = XmlLoggingConfiguration
                    .CreateFromXmlString(ResourceProvider
                        .GetEmbeddedAsString(@"Resources\Configs\nlog.config"));

                if (LogFactory.Configuration == null)
                {
                    Running = false;

                    return;
                }

                AppDirectoryName = Environment.Process
                    .ProcessName;
                DefaultLogDirectoryPath = GetLogsDirectoryPath(
                    LogType.Default);
                DebugLogDirectoryPath = GetLogsDirectoryPath(
                    LogType.Debug);

                LogFactory.AutoShutdown = false;

                LogFactory.Flush();

                Debug.Info("Logger initialized");
                Default.Info("Logger initialized");

                Default.Info($"Libraries Directory - {Environment.ExecAppDirectoryName}");
                Default.Info($"Execution File Directory - {Environment.ExecProcessDirectoryName}");

#if NETCOREAPP

                Default.Info($"Is Standalone App - {Environment.IsStandalone}");
                Default.Info($"Is Single File App - {Environment.IsSingleFile}");
                Default.Info($"Runtime Name - {Environment.RuntimeName}");
                Default.Info($"Runtime Version - {Environment.RuntimeVersion}");
                Default.Info($"Runtime Identifier - {Environment.RuntimeIdentifier}");

#endif

                Default.Info("Startup");

                LoggingStartup?.Invoke(
                    null, EventArgs.Empty);
            }
        }

        public static void Shutdown()
        {
            lock (SyncRoot)
            {
                if (Disposed)
                    return;

                if (!Running)
                    return;

                LoggingShutdown?.Invoke(
                    null, EventArgs.Empty);

                UnsubscribeAppShutdownEvent();

                UnsubscribeRISEvents();
                UnsubscribeAppDomainEvents();

                Default.Info("Shutdown");

                Default.Info($"Exit code - {System.Environment.ExitCode}");

                LogFactory.Shutdown();

                Running = false;
                Disposed = true;
            }
        }

        public static IDisposable Pause()
        {
            lock (SyncRoot)
            {
                var result = LogFactory.SuspendLogging();

                LoggingPaused?.Invoke(
                    null, EventArgs.Empty);

                return result;
            }
        }

        public static void Resume()
        {
            lock (SyncRoot)
            {
                LogFactory.ResumeLogging();

                LoggingResumed?.Invoke(
                    null, EventArgs.Empty);
            }
        }

        public static bool IsEnabled()
        {
            lock (SyncRoot)
            {
                if (Disposed)
                    return false;

                return LogFactory.IsLoggingEnabled();
            }
        }

        public static void Flush()
        {
            if (Disposed)
                return;

            LogFactory.Flush();
        }
        public static void Flush(TimeSpan timeout)
        {
            if (Disposed)
                return;

            LogFactory.Flush(timeout);
        }



        private static void SubscribeAppShutdownEvent()
        {
            lock (AppShutdownEventSyncRoot)
            {
                if (Disposed)
                    return;

                if (AppShutdownEventSubscribed)
                    return;

                AppDomain.CurrentDomain.DomainUnload += App_OnShutdown;
                AppDomain.CurrentDomain.ProcessExit += App_OnShutdown;

                AppShutdownEventSubscribed = true;
            }
        }

        private static void UnsubscribeAppShutdownEvent()
        {
            lock (AppShutdownEventSyncRoot)
            {
                if (Disposed)
                    return;

                if (!AppShutdownEventSubscribed)
                    return;

                AppDomain.CurrentDomain.DomainUnload -= App_OnShutdown;
                AppDomain.CurrentDomain.ProcessExit -= App_OnShutdown;

                AppShutdownEventSubscribed = false;
            }
        }



        public static void SubscribeRISEvents()
        {
            lock (RISEventsSyncRoot)
            {
                if (Disposed)
                    return;

                if (RISEventsSubscribed)
                    return;

                Events.Information += RIS_OnInformation;
                Events.Warning += RIS_OnWarning;
                Events.Error += RIS_OnError;

                RISEventsSubscribed = true;
            }
        }

        public static void UnsubscribeRISEvents()
        {
            lock (RISEventsSyncRoot)
            {
                if (Disposed)
                    return;

                if (!RISEventsSubscribed)
                    return;

                Events.Information -= RIS_OnInformation;
                Events.Warning -= RIS_OnWarning;
                Events.Error -= RIS_OnError;

                RISEventsSubscribed = false;
            }
        }



        public static void SubscribeAppDomainEvents()
        {
            lock (AppDomainEventsSyncRoot)
            {
                if (Disposed)
                    return;

                if (AppDomainEventsSubscribed)
                    return;

                AppDomain.CurrentDomain.UnhandledException += AppDomain_OnUnhandledException;
                AppDomain.CurrentDomain.FirstChanceException += AppDomain_OnFirstChanceException;
                AppDomain.CurrentDomain.AssemblyResolve += AppDomain_OnAssemblyResolve;
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AppDomain_OnAssemblyResolve;
                AppDomain.CurrentDomain.TypeResolve += AppDomain_OnResolve;
                AppDomain.CurrentDomain.ResourceResolve += AppDomain_OnResolve;

                AppDomainEventsSubscribed = true;
            }
        }

        public static void UnsubscribeAppDomainEvents()
        {
            lock (AppDomainEventsSyncRoot)
            {
                if (Disposed)
                    return;

                if (!AppDomainEventsSubscribed)
                    return;

                AppDomain.CurrentDomain.UnhandledException -= AppDomain_OnUnhandledException;
                AppDomain.CurrentDomain.FirstChanceException -= AppDomain_OnFirstChanceException;
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomain_OnAssemblyResolve;
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= AppDomain_OnAssemblyResolve;
                AppDomain.CurrentDomain.TypeResolve -= AppDomain_OnResolve;
                AppDomain.CurrentDomain.ResourceResolve -= AppDomain_OnResolve;

                AppDomainEventsSubscribed = false;
            }
        }



        public static string GetLogsDirectoryPath(
            LogType log)
        {
            var appDirectoryName = AppDirectoryName;
            var basePath = Environment.ExecProcessDirectoryName;
            var relativePath = "logs";

            if (appDirectoryName != null)
            {
                switch (log)
                {
                    case LogType.Default:
                        relativePath = Path.Combine(
                            relativePath, appDirectoryName, "default");
                        break;
                    case LogType.Debug:
                        relativePath = Path.Combine(
                            relativePath, appDirectoryName, "debug");
                        break;
                    default:
                        break;
                }
            }

            return Path.Combine(
                basePath, relativePath);
        }

        public static int DeleteLogs(
            int retentionDaysPeriod)
        {
            var deletedFilesCount = 0;

            deletedFilesCount += DeleteLogs(
                LogType.Default, retentionDaysPeriod);
            deletedFilesCount += DeleteLogs(
                LogType.Debug, retentionDaysPeriod);

            return deletedFilesCount;
        }
        private static int DeleteLogs(LogType log,
            int retentionDaysPeriod)
        {
            var logDirectoryPath = GetLogsDirectoryPath(
                log);

            return DeleteLogsInternal(logDirectoryPath,
                retentionDaysPeriod, "log");
        }
        private static int DeleteLogsInternal(string filesDirectoryPath,
            int retentionDaysPeriod, string fileExtension)
        {
            var deletedFilesCount = 0;

            try
            {
                filesDirectoryPath = filesDirectoryPath
                    .TrimEnd(Path.DirectorySeparatorChar)
                    .TrimEnd(Path.AltDirectorySeparatorChar);

                filesDirectoryPath = !Path.IsPathRooted(filesDirectoryPath)
                    ? Path.GetFullPath(filesDirectoryPath)
                    : filesDirectoryPath;

#if NETFRAMEWORK

                fileExtension = fileExtension.StartsWith(".")
                    ? fileExtension
                    : $".{fileExtension}";

#elif NETCOREAPP

                fileExtension = fileExtension.StartsWith('.')
                    ? fileExtension
                    : $".{fileExtension}";

#endif

                if (retentionDaysPeriod < 0)
                    return 0;

                if (!Directory.Exists(filesDirectoryPath))
                {
                    var exception = new DirectoryNotFoundException($"Cannot start log files deletion. Directory '{filesDirectoryPath}' not found");
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }

                var currentFileNameDate = GlobalDiagnosticsContext.Get(
                    "RIS-AppStartupTime");
                var nowDate = DateTime.UtcNow;
                var logFiles = Directory.GetFiles(
                    filesDirectoryPath, $"*{fileExtension}");

                for (int i = 0; i < logFiles.Length; ++i)
                {
                    ref var logFile = ref logFiles[i];

                    if (logFile == null)
                        continue;

                    try
                    {
                        if (Path.GetFileNameWithoutExtension(logFile)?.StartsWith(currentFileNameDate) != false)
                            continue;

                        var logFileDate = DateTime
                            .ParseExact(
                                Path.GetFileNameWithoutExtension(logFile)?.Substring(0, 19) ?? string.Empty,
                                "yyyy.MM.dd HH-mm-ss", CultureInfo.CurrentCulture)
                            .ToUniversalTime();

                        if (retentionDaysPeriod > 0
                            && logFileDate.AddDays(retentionDaysPeriod) > nowDate)
                        {
                            continue;
                        }

                        File.Delete(logFile);

                        ++deletedFilesCount;
                    }
                    catch (Exception ex)
                    {
                        Events.OnError(new RErrorEventArgs(ex, ex.Message));
                    }
                }

                return deletedFilesCount;
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex, ex.Message));
                throw;
            }
            finally
            {
                Default.Info($"Deleted older logs[{filesDirectoryPath}] - {deletedFilesCount}");
            }
        }



        private static void App_OnShutdown(object sender, EventArgs e)
        {
            Shutdown();
        }



        private static void RIS_OnInformation(object sender, RInformationEventArgs e)
        {
            Debug.Info($"{e.Message}");
        }

        private static void RIS_OnWarning(object sender, RWarningEventArgs e)
        {
            Default.Warn($"{e.Message}");
        }

        private static void RIS_OnError(object sender, RErrorEventArgs e)
        {
            Default.Error($"{e.SourceException?.GetType().Name ?? "Unknown"} - Message={e.Message ?? (e.SourceException?.Message ?? "Unknown")},HResult={e.SourceException?.HResult ?? 0},StackTrace=\n{e.SourceException?.StackTrace ?? "Unknown"}");
        }



        private static void AppDomain_OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;

            Default.Fatal($"{exception?.GetType().Name ?? "Unknown"} - Message={exception?.Message ?? "Unknown"},HResult={exception?.HResult ?? 0},StackTrace=\n{exception?.StackTrace ?? "Unknown"}");

            Shutdown();
        }

        private static void AppDomain_OnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            Debug.Error($"{e.Exception.GetType().Name} - Message={e.Exception.Message},HResult={e.Exception.HResult},StackTrace=\n{e.Exception.StackTrace ?? "Unknown"}");
        }

        private static Assembly AppDomain_OnAssemblyResolve(object sender, ResolveEventArgs e)
        {
            Debug.Info($"Resolve - Name={e.Name ?? "Unknown"},RequestingAssembly={e.RequestingAssembly?.FullName ?? "Unknown"}");

            return e.RequestingAssembly;
        }

        private static Assembly AppDomain_OnResolve(object sender, ResolveEventArgs e)
        {
            Debug.Info($"Resolve - Name={e.Name ?? "Unknown"},RequestingAssembly={e.RequestingAssembly?.FullName ?? "Unknown"}");

            return e.RequestingAssembly;
        }
    }
}
