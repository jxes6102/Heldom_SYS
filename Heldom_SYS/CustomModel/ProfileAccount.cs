using Heldom_SYS.Models;

namespace Heldom_SYS.CustomModel
{
    public class ProfileAccount
    {
        public string EmployeePhoto { get; set; } = null!;
        public string EmployeeName { get; set; } = null!;
        public string EmployeeId { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Department { get; set; } = null!;
        public string Position { get; set; } = null!;
        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProfileOptions
    {
        public required string currentPage { get; set; }
        public string? Department { get; set; }
        public string? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? IsActive { get; set; }
    }
}
