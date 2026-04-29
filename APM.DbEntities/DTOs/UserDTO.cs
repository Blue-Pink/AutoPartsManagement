using System;
using System.Collections.Generic;
using System.Text;
using APM.DbEntities.Base;

namespace APM.DbEntities.DTOs
{
    public class UserDTO : APMBaseEntity
    {
        public string? Username { get; set; }
        public string? Realname { get; set; }
        public string? Password { get; set; }
        public bool? IsActive { get; set; } = true;
        public IEnumerable<RoleDTO>? Roles { get; set; } = [];
    }
}
