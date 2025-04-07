namespace Heldom_SYS.CustomModel
{
    public class ProfileIndex:ProfileSettings
    {
        public string Department { get; set; } = null!;
        public string EmployeePhoto { get; set; } = null!;
        public string Mail { get; set; } = null!;
        public byte AnnualLeave { get; set; }
        public DateTime HireDate { get; set; }
        public int YearsBetween { get; set; }
    }
}
