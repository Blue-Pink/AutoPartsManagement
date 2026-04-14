using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace APM.DbEntities
{
    public class UserRoleView : BaseView
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string RealName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? RoleDescription { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
