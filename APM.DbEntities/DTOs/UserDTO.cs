using System;
using System.Collections.Generic;
using System.Text;

namespace APM.DbEntities.DTOs
{
    public class UserDTO
    {
        public string? Username { get; set; }
        public string? Realname { get; set; }
        public string? Password { get; set; }
        public List<Guid>? UserRoleIds { get; set; }
    }
}
