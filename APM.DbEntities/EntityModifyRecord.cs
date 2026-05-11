using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using APM.DbEntities.Base;

namespace APM.DbEntities
{
    [Description("实体变更记录")]
    public class EntityModifyRecord : BaseEntity
    {
        public required Guid EntityId { get; set; }
        public virtual EntityRecord? Entity { get; set; }
        public required string Operation { get; set; }
        public required string FieldName { get; set; }
        public required string OldValue { get; set; }
        public required string NewValue { get; set; }
        public required Guid ModifiedUserId { get; set; }
        public virtual User? ModifiedUser { get; set; }
    }
}
