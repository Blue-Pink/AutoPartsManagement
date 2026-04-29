using System;
using System.Collections.Generic;
using APM.DbEntities.Base;

namespace APM.DbEntities.DTOs
{
    public class InboundOrderDTO : APMBaseEntity
    {
        /// <summary>
        /// 关联供应商 Id
        /// </summary>
        public Guid SupplierId { get; set; }

        /// <summary>
        /// 操作用户 Id
        /// </summary>
        public Guid OperatorUserId { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string? OrderNo { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 明细列表
        /// </summary>
        public IEnumerable<InboundItemDTO>? Items { get; set; }

        /// <summary>
        /// 合计金额（可由前端传，服务端在保存时会校验/计算）
        /// </summary>
        public decimal TotalAmount { get; set; }
    }
}