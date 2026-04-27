using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace APM.DbEntities.Views
{
    [Description("配件详情视图")]
    public class PartView : BaseView
    {
        // 配件基础信息
        public Guid Id { get; set; }
        public required string PartName { get; set; }
        public required string OECode { get; set; }
        public string? Model { get; set; }
        public string? Brand { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int MinStock { get; set; }
        public int MaxStock { get; set; }
        public DateTime CreatedAt { get; set; }

        // 联查得到的名称（关键展示字段）
        public string? CategoryName { get; set; }
        public string? UnitName { get; set; }

        // 如果以后需要显示分类ID以便过滤，也可以加上
        public Guid CategoryId { get; set; }
        public Guid UnitId { get; set; }
    }
}
