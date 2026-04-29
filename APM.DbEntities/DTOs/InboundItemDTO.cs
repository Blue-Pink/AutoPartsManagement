using System;
using APM.DbEntities.Base;

namespace APM.DbEntities.DTOs
{
    public class InboundItemDTO : APMBaseEntity
    {
        /// <summary>
        /// 关联配件 Id
        /// </summary>
        public Guid PartId { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 小计（可由前端传入或服务端计算）
        /// </summary>
        public decimal TotalAmount { get; set; }
    }
}