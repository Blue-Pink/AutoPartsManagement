using APM.DbEntities.Base;
using System.ComponentModel;

namespace APM.DbEntities;

[Description("配件分类")]
public class PartCategory : BaseEntity
{
    [Description("名称")]
    public required string Name { get; set; }
    [Description("备注")]
    public string? Description { get; set; }
}