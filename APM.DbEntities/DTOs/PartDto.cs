using System;
using System.Collections.Generic;
using System.Text;
using APM.DbEntities.Base;

namespace APM.DbEntities.DTOs
{
    public class PartDTO : APMBaseEntity
    {
        public required string PartName { get; set; }
        public required string OECode { get; set; }
        public string? Model { get; set; }
        public string? Brand { get; set; }
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public Guid UnitId { get; set; }
        public string? UnitName { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int MinStock { get; set; }
        public int MaxStock { get; set; }
        public string? Remark { get; set; }
    }
}
