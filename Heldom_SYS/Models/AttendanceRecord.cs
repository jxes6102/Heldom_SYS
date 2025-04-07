using System;
using System.Collections.Generic;

namespace Heldom_SYS.Models;

public partial class AttendanceRecord
{
    public string AttendanceId { get; set; } = null!;

    public string EmployeeId { get; set; } = null!;

    public DateTime WorkDate { get; set; }

    public DateTime CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
