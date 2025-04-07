namespace Heldom_SYS.CustomModel
{
    public class ProfileSettings
    {
        public string employeeName { get; set; } = null!;
        public string position { get; set; } = null!;
        public string employeeId { get; set; } = null!;
        public DateTime birthDate { get; set; }
        public string phoneNumber { get; set; } = null!;
        public string address { get; set; } = null!;
        public string emergencyContact { get; set; } = null!;
        public string emergencyContactPhone { get; set; } = null!;
    }

    public class EmployeeDetailUpdateModel
    {
        public string phoneNumber { get; set; } = null!;
        public string address { get; set; } = null!;
        public string emergencyContact { get; set; } = null!;
        public string emergencyContactPhone { get; set; } = null!;
    }
}
