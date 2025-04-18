using System;
using System.Collections.Generic;

namespace Heldom_SYS.Models;

public partial class EmployeeDetail
{
    public string EmployeeId { get; set; } = null!;

    public string Department { get; set; } = null!;

    public string? ImmediateSupervisor { get; set; }

    public byte[] EmployeePhoto { get; set; } = null!;

    public string EmployeeName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Mail { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public DateTime BirthDate { get; set; }

    public string EmergencyContact { get; set; } = null!;

    public string EmergencyRelationship { get; set; } = null!;

    public string EmergencyContactPhone { get; set; } = null!;

    public byte AnnualLeave { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
