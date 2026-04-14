using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace APM.Extensions.Logger
{
    internal class LoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new Logger(this);
        }

        public void Dispose()
        {

        }
    }
}
