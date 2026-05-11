using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace APM.UtilEntities
{
    public class APMException : Exception
    {
        public override string Message { get; }
        public bool LogForFile { get; }
        public APMException(string message) : base(message)
        {
            Message = message;
        }

        public APMException(string message,bool logForFile) : base(message)
        {
            Message = message;
            LogForFile = logForFile;
        }

        public APMException(string message, Exception innerException) : base(message, innerException)
        {
            Message = message;
        }
    }
}
