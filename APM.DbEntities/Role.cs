using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace APM.DbEntities
{
    [Description("角色信息")]
    public class Role : BaseEntity
    {
        [Required, StringLength(20), Description("角色名")]
        public string? RoleName { get; set; }

        [StringLength(100), Description("角色描述")]
        public string? Description { get; set; }

        public virtual ICollection<UserRole>? UserRoles { get; set; } = new List<UserRole>();
    }
}
