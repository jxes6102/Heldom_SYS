using System;
using System.Collections.Generic;

namespace Heldom_SYS.Models;

public partial class Employee
{
    public string EmployeeId { get; set; } = null!;

    public bool IsActive { get; set; }

    public string Position { get; set; } = null!;

    public string PositionRole { get; set; } = null!;

    public DateTime HireDate { get; set; }

    public DateTime? ResignationDate { get; set; }

    public virtual ICollection<Accident> Accidents { get; set; } = new List<Accident>();

    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();

    public virtual ICollection<Blueprint> Blueprints { get; set; } = new List<Blueprint>();

    public virtual EmployeeDetail? EmployeeDetail { get; set; }

    public virtual ICollection<LeaveRecord> LeaveRecords { get; set; } = new List<LeaveRecord>();

    public virtual Temporarier? Temporarier { get; set; }
}
