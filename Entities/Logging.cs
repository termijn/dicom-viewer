using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;
using System.IO;
using System.Reflection;

namespace Entities
{
    public static class Logging
    {
        private static ILoggerFactory factory;

        public static ILogger GetLogger(string category)
        {
            EnsureFactoryCreated();
            ILogger logger = factory.CreateLogger(category);
            return logger;
        }

        public static ILogger GetLogger<T>()
        {
            EnsureFactoryCreated();
            ILogger logger = factory.CreateLogger<T>();
            return logger;
        }

        private static void EnsureFactoryCreated()
        {
            if (factory == null)
            {
                var config = new LoggingConfiguration();
                var dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Logging)).Location);
                var dirPath = Path.Combine(dir, "Logging");
                var logFileTarget = new NLog.Targets.FileTarget()
                {
                    CreateDirs = true,
                    FileName = Path.Combine(dirPath, "dicom-viewer.log"),
                    FileNameKind = NLog.Targets.FilePathKind.Absolute,
                    Layout = "${date}|${level:uppercase=true}|${message} ${exception}|${logger}|${all-event-properties}",
                    Name = "FileLog",
                    ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.DateAndSequence,
                    MaxArchiveFiles = 10,
                    ArchiveOldFileOnStartup = true
                };
                config.AddTarget(logFileTarget);
                config.AddRuleForAllLevels(logFileTarget, "*", false);

                factory = LoggerFactory.Create(builder =>
                {
                    builder
                        .AddFilter("Microsoft", LogLevel.Warning)
                        .AddFilter("System", LogLevel.Warning)
                        .AddFilter("DicomViewer", LogLevel.Debug)
                        .AddNLog(config)
                        .AddConsole();
                });
            }
        }
    }
}