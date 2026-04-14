using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace APM.UtilEntities
{
    public class APMException : Exception
    {
        public override string Message { get; }
        public APMException(string message) : base(message)
        {
            Message = message;
        }
    }
}
