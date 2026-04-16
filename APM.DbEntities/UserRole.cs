using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text;
using System.Text.Json.Serialization;

namespace APM.DbEntities
{
    [Description("用户角色配置")]
    public class UserRole : BaseEntity
    {
        [Description("用户")]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId)), JsonIgnore]
        public virtual User? User { get; set; }

        [Description("角色")]
        public Guid RoleId { get; set; }

        [ForeignKey(nameof(RoleId)), JsonIgnore]
        public virtual Role? Role { get; set; }

        [Description("角色分配时间")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
