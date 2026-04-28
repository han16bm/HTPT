using System.Collections.Generic;

namespace netcore.Entities.Entities;

/// <summary>
/// Navigation properties cho Role entity
/// </summary>
public partial class Role
{
    public virtual ICollection<User> Users { get; set; } = [];
}


