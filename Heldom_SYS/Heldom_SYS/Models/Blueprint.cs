using System;
using System.Collections.Generic;

namespace Heldom_SYS.Models;

public partial class Blueprint
{
    public string BlueprintId { get; set; } = null!;

    public string PrintCategoryId { get; set; } = null!;

    public string BlueprintName { get; set; } = null!;

    public decimal BlueprintVersion { get; set; }

    public byte[] BlueprintFile { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime UploadTime { get; set; }

    public string EmployeeId { get; set; } = null!;

    public bool? PrintStatus { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual PrintCategory PrintCategory { get; set; } = null!;
}
