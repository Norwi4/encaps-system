using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class UserRole1
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public long? RolesId { get; set; }
}
