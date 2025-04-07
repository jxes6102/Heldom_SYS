using System;
using System.Collections.Generic;

namespace Heldom_SYS.Models;

public partial class Temporarier
{
    public string EmployeeId { get; set; } = null!;

    public string EmployeeName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string CompanyId { get; set; } = null!;

    public virtual Company Company { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;
}
