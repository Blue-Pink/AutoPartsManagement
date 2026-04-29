using System;
using System.Collections.Generic;
using System.Text;
using APM.DbEntities.Base;

namespace APM.DbEntities.DTOs
{
    public class RoleDTO : APMBaseEntity
    {
        public string? RoleName { get; set; }
        public string? Description { get; set; }
    }
}
