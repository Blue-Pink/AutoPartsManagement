using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace APM.DbEntities
{
    [Description("配件档案")]
    public class Part : BaseEntity
    {
        [Description("配件名称")]
        public required string PartName { get; set; }

        [Description("OE码（原厂编号），行业唯一标识")]
        public required string OECode { get; set; }

        [Description("规格型号")]
        public string? Model { get; set; }

        [Description("品牌（如：博世、德尔福）")]
        public string? Brand { get; set; }

        [Description("分类 Id")]
        public Guid CategoryId { get; set; }

        [Description("分类"), JsonIgnore]
        public virtual PartCategory? Category { get; set; }

        [Description("单位 Id")]
        public Guid UnitId { get; set; }

        [Description("单位"), JsonIgnore]
        public virtual PartUnit? Unit { get; set; }

        [Description("参考进价")]
        public decimal CostPrice { get; set; }

        [Description("标准售价")]
        public decimal SellingPrice { get; set; }

        // 库存配置（用于后续的预警逻辑）
        [Description("最低库存（安全水位）")]
        public int MinStock { get; set; } = 0;

        [Description("最高库存（积压水位）")]
        public int MaxStock { get; set; } = 999;

        [Description("备注")]
        public string? Remark { get; set; }
    }
}