using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace APM.DbEntities
{
    [Description("入库明细")]
    public class InboundItem : BaseEntity
    {
        [Description("入库单")]
        public Guid InboundOrderId { get; set; }
        [JsonIgnore]
        public virtual InboundOrder? InboundOrder { get; set; }
        [Description("配件")]
        public Guid PartId { get; set; }
        public virtual Part? Part { get; set; }
        [Description("入库数量")]
        public int Quantity { get; set; }
        [Description("入库单价")]
        public decimal Price { get; set; }
        [Description("合计金额(入库数量x入库单价)")]
        public decimal TotalAmount { get; set; }
    }
}
