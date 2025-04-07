using System;
using System.Collections.Generic;

namespace Heldom_SYS.Models;

public partial class Company
{
    public string CompanyId { get; set; } = null!;

    public string CompanyName { get; set; } = null!;

    public virtual ICollection<Temporarier> Temporariers { get; set; } = new List<Temporarier>();
}
