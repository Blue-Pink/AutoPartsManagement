using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace APM.DbEntities
{
    [Description("角色信息")]
    public class Role : BaseEntity
    {
        [Required, StringLength(20), Description("角色名")]
        public required string RoleName { get; set; }

        [StringLength(100), Description("角色描述")]
        public required string Description { get; set; }

        [JsonIgnore]
        public virtual ICollection<UserRole>? UserRoles { get; set; } = new List<UserRole>();
    }
}
