using Heldom_SYS.Models;

namespace Heldom_SYS.CustomModel
{
    public class EmployeeData
    {
        public string employeeId { get; set; } = null!;

        public bool isActive { get; set; }

        public string position { get; set; } = null!;

        public string positionRole { get; set; } = null!;

        public DateTime hireDate { get; set; }

        public DateTime? resignationDate { get; set; }

    }
    public class ProfileCreate
    {
        public string EmployeeId { get; set; } = null!;

        public string Department { get; set; } = null!;

        public string? ImmediateSupervisor { get; set; }

        public string EmployeePhoto { get; set; } = null!;

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

    }
    public class ProfileNewAccountData
    {
        public required string EmployeeId { get; set; }
        public required string EmployeeName { get; set; }
    }

    public class GetNewAccountEditData
    {
        public string EmployeePhoto { get; set; } = null!;
        public string EmployeeName { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string EmergencyContact { get; set; } = null!;
        public string EmergencyRelationship { get; set; } = null!;
        public string EmergencyContactPhone { get; set; } = null!;
        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; }
        public string EmployeeId { get; set; } = null!;
        public string PositionRole { get; set; } = null!;
        public string Department { get; set; } = null!;
        public string Position { get; set; } = null!;
        public string? ImmediateSupervisor { get; set; }
        public string Address { get; set; } = null!;
        public string Mail { get; set; } = null!;
        public string Password { get; set; } = null!;
        public DateTime? ResignationDate { get; set; }
        public int AnnualLeave { get; set; }
    }
}
