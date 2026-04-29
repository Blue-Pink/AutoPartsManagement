using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace APM.DbEntities.Base
{
    public abstract class BaseEntity : APMBaseEntity
    {
        [Key, Required]
        public new Guid Id { get; set; } = Guid.Empty;
    }
}
