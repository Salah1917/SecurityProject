using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities;

public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;

    public List<RolePermission> RolePermissions { get; set; } = new();
}