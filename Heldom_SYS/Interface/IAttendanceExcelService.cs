namespace Heldom_SYS.Interface
{
    public interface IAttendanceExcelService
    {
        Task<byte[]> GetAttendanceRecordsExcel(string employeeName, DateTime? startDate, DateTime? endDate);

        Task<byte[]> GetLeaveRecordsExcel(string employeeName, DateTime? startDate, DateTime? endDate);
    }
}
