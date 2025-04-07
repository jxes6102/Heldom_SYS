using System;
using System.Collections.Generic;

namespace Heldom_SYS.Models;

public partial class Accident
{
    public string AccidentId { get; set; } = null!;

    public string AccidentType { get; set; } = null!;

    public string AccidentTitle { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public string EmployeeId { get; set; } = null!;

    public DateTime UploadTime { get; set; }

    public string IncidentControllerId { get; set; } = null!;

    public string? Response { get; set; }

    public DateTime? EndTime { get; set; }

    public bool IncidentStatus { get; set; }

    public virtual ICollection<AccidentFile> AccidentFiles { get; set; } = new List<AccidentFile>();

    public virtual Employee Employee { get; set; } = null!;
}
