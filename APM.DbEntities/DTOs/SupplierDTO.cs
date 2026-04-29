using System;
using System.ComponentModel;
using APM.DbEntities.Base;

namespace APM.DbEntities.DTOs
{
    public class SupplierDTO : APMBaseEntity
    {
        /// <summary>
        /// 供应商名称（必填）
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 联系人
        /// </summary>
        public string? Contact { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        public string? Address { get; set; }
    }
}
