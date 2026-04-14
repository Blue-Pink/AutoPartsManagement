using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace APM.DbEntities
{
    [Description("角色权限配置")]
    public class RolePermission : BaseEntity
    {
        public Guid RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;

        [Description("权限所属的实体名称，例如：User、Role等")]
        public Guid EntityId { get; set; }
        public virtual EntityRecord Entity { get; set; } = null!;

        // 细分权限（增删改查）
        public bool CanRead { get; set; } = false;
        public bool CanCreate { get; set; } = false;
        public bool CanUpdate { get; set; } = false;
        public bool CanDelete { get; set; } = false;
    }
}
