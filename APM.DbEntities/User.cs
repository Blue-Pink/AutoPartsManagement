using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace APM.DbEntities
{
    [Description("用户信息")]
    public class User : BaseEntity
    {
        [Required, StringLength(50), Description("用户名")]
        public required string Username { get; set; }

        [Required, Description("存储加密后的密码"), JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(50), Description("真实姓名")]
        public required string Realname { get; set; }

        [Description("账号是否启用")]
        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        public virtual ICollection<UserRole>? UserRoles { get; set; } = new List<UserRole>();
    }
}
