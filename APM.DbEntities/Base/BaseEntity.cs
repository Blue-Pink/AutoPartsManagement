using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace APM.DbEntities.Base
{
    public abstract class BaseEntity : APMBaseEntity
    {
        [Key, Required]
        public Guid Id { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; } = DateTime.UtcNow;
    }
}
