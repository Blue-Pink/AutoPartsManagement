using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace APM.DbEntities
{
    [Description("销售出库单")]
    public class OutboundOrder : BaseEntity
    {
        [Description("出库单号")]
        public required string OrderNo { get; set; } 
        [Description("客户ID")]
        public Guid CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }
        [Description("总金额")]
        public decimal TotalAmount { get; set; }
        [Description("出库日期")]
        public DateTime OutboundDate { get; set; }
        [Description("备注")]
        public string? Remark { get; set; }
        public virtual ICollection<OutboundItem> OutboundItems { get; set; } = new List<OutboundItem>();
    }
}
