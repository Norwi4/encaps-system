using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Profile
{
    public int Id { get; set; }

    public long UserId { get; set; }

    public string FName { get; set; } = null!;

    public string LName { get; set; } = null!;

    public string Patronimyc { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Mail { get; set; } = null!;
}
