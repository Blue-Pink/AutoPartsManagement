using APM.DbEntities.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace APM.DbEntities.Views
{
    public class AllBoundItemView : APMBaseEntity
    {
        public Guid PartId { get; set; }
        public Part? Part { get; set; }
        [Precision(18, 2)]
        public int Quantity { get; set; }
        public int Type { get; set; }
    }
}
