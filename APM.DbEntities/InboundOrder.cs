using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace APM.DbEntities
{
    [Description("采购入库单")]
    public class InboundOrder : BaseEntity
    {
        [Description("订单号")]
        public required string OrderNo { get; set; }
        [Description("供应商")]
        public Guid SupplierId { get; set; }
        public virtual Supplier? Supplier { get; set; }
        [Description("总金额")]
        public decimal TotalAmount { get; set; }
        [Description("经办人")]
        public Guid OperatorUserId { get; set; }
        public virtual User? OperatorUser { get; set; }
        [Description("备注")]
        public string? Remark { get; set; }
        [JsonIgnore]
        public virtual ICollection<InboundItem>? InboundItems { get; set; } = new List<InboundItem>();
    }
}
