using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class Role
{
    public virtual ICollection<User> Users { get; set; } = [];
}
