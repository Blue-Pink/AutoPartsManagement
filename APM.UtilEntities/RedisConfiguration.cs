using System;
using System.Collections.Generic;
using System.Text;

namespace APM.UtilEntities
{
    public class RedisConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Password { get; set; } = string.Empty;
        public int Timeout { get; set; }
        public int Db { get; set; }
    }
}
    