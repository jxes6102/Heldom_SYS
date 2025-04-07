using System;
using System.Collections.Generic;

namespace Heldom_SYS.Models;

public partial class AccidentFile
{
    public string FileId { get; set; } = null!;

    public string AccidentId { get; set; } = null!;

    public byte[] FileImage { get; set; } = null!;

    public bool ResponseType { get; set; }

    public virtual Accident Accident { get; set; } = null!;
}
