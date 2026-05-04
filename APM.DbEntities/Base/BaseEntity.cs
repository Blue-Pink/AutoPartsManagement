using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace APM.DbEntities.Base
{
    public abstract class BaseEntity : APMBaseEntity
    {
        [Key, Required]
        public new Guid Id { get; set; } = Guid.Empty;

        [Description("经办人")] public Guid OperatorUserId { get; set; } = new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4");
        public virtual User? OperatorUser { get; set; }
    }
}
