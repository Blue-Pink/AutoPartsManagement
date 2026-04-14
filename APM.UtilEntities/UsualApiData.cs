using System;
using System.Collections.Generic;
using System.Text;

namespace APM.UtilEntities
{
    public class UsualApiData<T>
    {
        public T? Data { get; set; }
        public IEnumerable<T>? DataList { get; set; }
        public string? Message { get; set; }
        public dynamic? CustomData { get; set; }
        public UsualStateCode StateCode { get; set; } = UsualStateCode.Success;
    }

    public enum UsualStateCode
    {
        /// <summary>
        /// 操作异常
        /// </summary>
        Error = -1,
        /// <summary>
        /// 访问成功
        /// </summary>
        Success,
        /// <summary>
        /// 缺少权限
        /// </summary>
        AccessDenied,
        /// <summary>
        /// 未知错误
        /// </summary>
        Unknown,
    }
}
