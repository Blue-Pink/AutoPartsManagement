using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace APM.DbEntities
{
    [Description("实体记录")]
    public class EntityRecord : BaseEntity
    {
        [Required, StringLength(100), Description("实体类名")]
        public string? EntityName { get; set; }

        [Description("记录实体是否还在使用")]
        public bool IsActive { get; set; } = true;

        [Required, StringLength(200), Description("实体完整类名")]
        public string? FullName { get; set; }

        [StringLength(100), Description("实体描述信息")]
        public string? Description { get; set; }
    }
}
