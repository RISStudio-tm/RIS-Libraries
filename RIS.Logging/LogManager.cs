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


        private static readonly object LogSyncRoot = new object();
        private static volatile Logger _log;
        public static Logger Log
        {
            get
            {
                if (_log == null)
                {
                    lock (LogSyncRoot)
                    {
                        if (_log == null)
                            _log = LogFactory.GetLogger(nameof(Log));
                    }
                }

                return _log;
            }
        }
        private static readonly object DebugLogSyncRoot = new object();
        private static volatile Logger _debugLog;
        public static Logger DebugLog
        {
            get
            {
                if (_debugLog == null)
                {
                    lock (DebugLogSyncRoot)
                    {
                        if (_debugLog == null)
                            _debugLog = LogFactory.GetLogger(nameof(DebugLog));
                    }
                }

                return _debugLog;
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

            Startup();

            AutoShutdown = true;

            SubscribeRISEvents();
            SubscribeAppDomainEvents();
        }



#pragma warning disable SS002 // DateTime.Now was referenced
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
                    "AppStartupTime",
                    startupTime.ToString("yyyy.MM.dd HH-mm-ss",
                        CultureInfo.InvariantCulture));

                LogFactory.Configuration = XmlLoggingConfiguration
                    .CreateFromXmlString(ResourceProvider
                        .GetEmbeddedAsString(@"Resources\Configs\nlog.config"));
                LogFactory.AutoShutdown = false;

                LogFactory.Flush();

                DebugLog.Info("Logger initialized");
                Log.Info("Logger initialized");

                Log.Info($"Libraries Directory - {Environment.ExecAppDirectoryName}");
                Log.Info($"Execution File Directory - {Environment.ExecProcessDirectoryName}");

#if NETCOREAPP

                Log.Info($"Is Standalone App - {Environment.IsStandalone}");
                Log.Info($"Is Single File App - {Environment.IsSingleFile}");
                Log.Info($"Runtime Name - {Environment.RuntimeName}");
                Log.Info($"Runtime Version - {Environment.RuntimeVersion}");
                Log.Info($"Runtime Identifier - {Environment.RuntimeIdentifier}");

#endif

                Log.Info("Startup");

                LoggingStartup?.Invoke(
                    null, EventArgs.Empty);
            }
        }
#pragma warning restore SS002 // DateTime.Now was referenced

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

                Log.Info("Shutdown");

                Log.Info($"Exit code - {System.Environment.ExitCode}");

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

            var logDirectoryPath = LogUtilities
                .GetLogDirectoryPath(log);

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
                    "AppStartupTime");
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
                Log.Info($"Deleted older logs[{filesDirectoryPath}] - {deletedFilesCount}");
            }
        }



        private static void App_OnShutdown(object sender, EventArgs e)
        {
            Shutdown();
        }



        private static void RIS_OnInformation(object sender, RInformationEventArgs e)
        {
            DebugLog.Info($"{e.Message}");
        }

        private static void RIS_OnWarning(object sender, RWarningEventArgs e)
        {
            Log.Warn($"{e.Message}");
        }

        private static void RIS_OnError(object sender, RErrorEventArgs e)
        {
            Log.Error($"{e.SourceException?.GetType().Name ?? "Unknown"} - Message={e.Message ?? (e.SourceException?.Message ?? "Unknown")},HResult={e.SourceException?.HResult ?? 0},StackTrace=\n{e.SourceException?.StackTrace ?? "Unknown"}");
        }



        private static void AppDomain_OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;

            Log.Fatal($"{exception?.GetType().Name ?? "Unknown"} - Message={exception?.Message ?? "Unknown"},HResult={exception?.HResult ?? 0},StackTrace=\n{exception?.StackTrace ?? "Unknown"}");

            Shutdown();
        }

        private static void AppDomain_OnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            DebugLog.Error($"{e.Exception.GetType().Name} - Message={e.Exception.Message},HResult={e.Exception.HResult},StackTrace=\n{e.Exception.StackTrace ?? "Unknown"}");
        }

        private static Assembly AppDomain_OnAssemblyResolve(object sender, ResolveEventArgs e)
        {
            DebugLog.Info($"Resolve - Name={e.Name ?? "Unknown"},RequestingAssembly={e.RequestingAssembly?.FullName ?? "Unknown"}");

            return e.RequestingAssembly;
        }

        private static Assembly AppDomain_OnResolve(object sender, ResolveEventArgs e)
        {
            DebugLog.Info($"Resolve - Name={e.Name ?? "Unknown"},RequestingAssembly={e.RequestingAssembly?.FullName ?? "Unknown"}");

            return e.RequestingAssembly;
        }
    }
}
