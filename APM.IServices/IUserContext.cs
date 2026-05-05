using System;
using System.Collections.Generic;
using System.Text;

namespace APM.IServices;

public interface IUserContext
{
    public Guid? UserId { get; }
    public string? Username { get; }
    public bool IsAuthenticated { get; }
    public IEnumerable<Guid>? RoleIds { get; }
}