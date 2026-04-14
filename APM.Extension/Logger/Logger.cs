using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace APM.Extensions.Logger
{
    public class Logger : ILogger
    {
        private readonly ILoggerProvider _provider;

        public Logger(ILoggerProvider provider)
        {
            _provider = provider;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _disposable;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel) || exception is null) return;

            var applicationBasePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            var now = DateTime.UtcNow.ToLocalTime();
            var logDirectory = $"{applicationBasePath}/log/{now:yyyy-MM-dd}";
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            var logFilePath = $"{logDirectory}/{logLevel}.log";
            using FileStream logFileStream = new FileStream(logFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var logRecord = $"-[{now:yyyy-MM-dd HH:mm:ss fff tt}]- {exception.Message}:{exception}{Environment.NewLine}";
            logFileStream.Write(Encoding.UTF8.GetBytes(logRecord), 0, logRecord.Length);
        }

        private static readonly Disposable _disposable = new();

        private class Disposable : IDisposable
        {
            public void Dispose()
            {

            }
        }
    }
}
