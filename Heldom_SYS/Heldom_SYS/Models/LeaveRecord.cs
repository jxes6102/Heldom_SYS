using System;
using System.Collections.Generic;

namespace Heldom_SYS.Models;

public partial class LeaveRecord
{
    public string EmployeeId { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public byte SpentHours { get; set; }

    public bool LeaveStatus { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
