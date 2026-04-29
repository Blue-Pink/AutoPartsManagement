using APM.DbEntities.Base;
using System.ComponentModel;

namespace APM.DbEntities
{
    [Description("供应商")]
    public class Supplier : BaseEntity
    {
        [Description("名称")]
        public required string Name { get; set; }
        [Description("联系人")]
        public string? Contact { get; set; }
        [Description("联系电话")]
        public string? Phone { get; set; }
        [Description("详细地址")]
        public string? Address { get; set; }
    }
}
