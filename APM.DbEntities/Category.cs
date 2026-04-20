using APM.DbEntities.Base;
using System.ComponentModel;

namespace APM.DbEntities;

[Description("配件分类")]
public class Category : BaseEntity
{
    public required string Name { get; set; } // 如：发动机系统、制动系统
    public string? Description { get; set; }
}