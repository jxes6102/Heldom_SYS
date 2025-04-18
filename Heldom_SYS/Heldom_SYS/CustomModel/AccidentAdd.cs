using Heldom_SYS.Models;

namespace Heldom_SYS.CustomModel
{
    public class AccidentAdd
    {
        public string AccidentType { get; set; } = null!;

        public string AccidentTitle { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string StartTime { get; set; } = null!;

        public string? EndTime { get; set; }

    }
}
