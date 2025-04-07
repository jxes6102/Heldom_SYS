using System;
using System.Collections.Generic;

namespace Heldom_SYS.Models;

public partial class PrintCategory
{
    public string PrintCategoryId { get; set; } = null!;

    public string PrintCategory1 { get; set; } = null!;

    public virtual ICollection<Blueprint> Blueprints { get; set; } = new List<Blueprint>();
}
