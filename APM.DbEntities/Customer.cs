using APM.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace APM.DbEntities
{
    [Description("客户")]
    public class Customer : BaseEntity
    {
        [Description("客户名称")]
        public required string Name { get; set; }

        [Description("联系人")]
        public string? ContactPerson { get; set; }

        [Description("联系电话")]
        public string? Phone { get; set; }

        [Description("联系地址")]
        public string? Address { get; set; }

        [Description("备注")]
        public string? Remark { get; set; }
    }
}
