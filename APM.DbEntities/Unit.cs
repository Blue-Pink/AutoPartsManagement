using APM.DbEntities.Base;
using System.ComponentModel;

namespace APM.DbEntities;

[Description("计量单位")]
public class Unit : BaseEntity
{
    public required string Name { get; set; } // 如：个、套、件、升
}