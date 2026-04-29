using System;
using System.Collections.Generic;
using System.Text;

namespace APM.DbEntities.Base
{
    public abstract class APMBaseEntity
    {
        public Guid? Id { get; set; } = Guid.Empty;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; } = DateTime.UtcNow;
    }
}
