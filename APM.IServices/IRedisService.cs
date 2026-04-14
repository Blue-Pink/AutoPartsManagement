using System;
using System.Collections.Generic;
using System.Text;

namespace APM.IServices
{
    public interface IRedisService
    {
        string Get(string key);
        public T? Get<T>(string key);
        public List<T>? GetList<T>(string key);
        void Set(string key, dynamic value, TimeSpan timeSpan);

    }
}
