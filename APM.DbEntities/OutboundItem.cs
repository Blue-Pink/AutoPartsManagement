using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace APM.DbEntities
{
    [Description("出库明细")]
    public class OutboundItem : BaseEntity
    {
        [Description("出库单ID")]
        public Guid OutboundOrderId { get; set; }
        public virtual OutboundOrder? OutboundOrder { get; set; }
        [Description("配件ID")]
        public Guid PartId { get; set; }
        public virtual Part? Part { get; set; }
        [Description("出库数量")]
        public int Quantity { get; set; }
        [Description("出库单价")]
        public decimal Price { get; set; }
        [Description("小计")]
        public decimal TotalAmount { get; set; } 
    }
}
