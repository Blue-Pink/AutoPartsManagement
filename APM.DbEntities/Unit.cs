using APM.DbEntities.Base;
using System.ComponentModel;

namespace APM.DbEntities;

[Description("计量单位")]
public class Unit : BaseEntity
{
    [Description("个、套、件、升")]
    public required string Name { get; set; }
}