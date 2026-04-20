using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace APM.DbEntities
{
    [Description("配件档案")]
    public class Part : BaseEntity
    {
        public required string PartName { get; set; } // 配件名称

        public required string OECode { get; set; } // OE码（原厂编号），行业唯一标识

        public string? Model { get; set; } // 规格型号

        public string? Brand { get; set; } // 品牌（如：博世、德尔福）

        // 关联关系
        public Guid CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public Guid UnitId { get; set; }
        public virtual Unit? Unit { get; set; }

        // 价格信息
        public decimal CostPrice { get; set; } // 参考进价
        public decimal SellingPrice { get; set; } // 标准售价

        // 库存配置（用于后续的预警逻辑）
        public int MinStock { get; set; } = 0; // 最低库存（安全水位）
        public int MaxStock { get; set; } = 999; // 最高库存（积压水位）

        // 备注
        public string? Remark { get; set; }
    }
}
