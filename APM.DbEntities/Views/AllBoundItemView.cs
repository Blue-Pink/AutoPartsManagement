using APM.DbEntities.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace APM.DbEntities.Views
{
    [Description("出/入库单明细视图")]
    public class AllBoundItemView : BaseView
    {
        public Guid PartId { get; set; }
        public virtual Part? Part { get; set; }
        [Precision(18, 2)]
        public int Quantity { get; set; }
        [Precision(18, 2)]
        public decimal TotalAmount { get; set; }
        public int Type { get; set; }
    }
}
