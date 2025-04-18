using Heldom_SYS.CustomModel;
using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.Record;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Heldom_SYS.Service
{
    public class AttendanceExcelService : IAttendanceExcelService
    {
        private readonly SqlConnection DataBase;
        private readonly ConstructionDbContext _context;

        public AttendanceExcelService(SqlConnection connection, ConstructionDbContext context)
        {
            DataBase = connection;
            _context = context;
        }

        public async Task<byte[]> GetAttendanceRecordsExcel(string employeeName, DateTime? startDate, DateTime? endDate)
        {

            var query = _context.AttendanceRecords
                .Include(ar => ar.Employee)
                .ThenInclude(e => e.Temporarier) // 臨時員工資料
                .ThenInclude(ir => ir.Company)
                .AsQueryable();

            if (!string.IsNullOrEmpty(employeeName))
            {
                query = query.Where(ar =>
                    (ar.Employee != null && ar.Employee.EmployeeDetail != null && ar.Employee.EmployeeDetail.EmployeeName.Contains(employeeName)) ||
                    (ar.Employee != null && ar.Employee.Temporarier != null && ar.Employee.Temporarier.EmployeeName.Contains(employeeName)));
            }
            if (startDate.HasValue)
            {
                query = query.Where(ar => ar.WorkDate >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                query = query.Where(ar => ar.WorkDate <= endDate.Value.Date);
            }

            var records = await query.Where((item)=> item.EmployeeId.Contains("P"))
                .OrderByDescending(ar => ar.CheckOutTime == null) // 未簽退優先
                .ThenBy(ar => (ar.Employee.Temporarier != null) ? ar.Employee.Temporarier.Company.CompanyId : "0")
                .Select(ar => new
                {
                    id = ar.AttendanceId,
                    status = ar.CheckOutTime != null,
                    workDate = ar.WorkDate.ToString("yyyy/MM/dd"),
                    employeeName = ar.Employee != null
                        ? (ar.EmployeeId.StartsWith("E") && ar.Employee.EmployeeDetail != null
                            ? ar.Employee.EmployeeDetail.EmployeeName
                            : (ar.Employee.Temporarier != null
                                ? ar.Employee.Temporarier.EmployeeName
                                : "未知"))
                        : "未知",
                    employeeId = ar.EmployeeId,
                    startTime = ar.CheckInTime.ToString("HH:mm"),
                    endTime = ar.CheckOutTime.HasValue ? ar.CheckOutTime.Value.ToString("HH:mm") : "未簽退",
                    countTime = ar.CheckOutTime.HasValue ?
                                    ar.CheckOutTime.Value.Hour > 12 ? 
                                        ((ar.CheckOutTime.Value - ar.CheckInTime).TotalHours - 1).ToString("F1") 
                                        : (ar.CheckOutTime.Value - ar.CheckInTime).TotalHours.ToString("F1")
                                : "0",
                    companyName = ((ar.Employee != null) && (ar.Employee.Temporarier != null)) ? ar.Employee.Temporarier.Company.CompanyName : " ",
                })
                .ToListAsync();


            //.Company.CompanyName
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Products");
            // 設定置中樣式
            ICellStyle centeredStyle = workbook.CreateCellStyle();
            centeredStyle.Alignment = HorizontalAlignment.Center;  // 水平置中
            centeredStyle.VerticalAlignment = VerticalAlignment.Center;  // 垂直置中


            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("序號");
            headerRow.CreateCell(1).SetCellValue("打卡日期");
            headerRow.CreateCell(2).SetCellValue("員工姓名");
            headerRow.CreateCell(3).SetCellValue("員工編號");
            headerRow.CreateCell(4).SetCellValue("上班簽到");
            headerRow.CreateCell(5).SetCellValue("下班簽退");
            headerRow.CreateCell(6).SetCellValue("總時數");
            headerRow.CreateCell(7).SetCellValue("廠商");

            headerRow.GetCell(0).CellStyle = centeredStyle;
            headerRow.GetCell(1).CellStyle = centeredStyle;
            headerRow.GetCell(2).CellStyle = centeredStyle;
            headerRow.GetCell(3).CellStyle = centeredStyle;
            headerRow.GetCell(4).CellStyle = centeredStyle;
            headerRow.GetCell(5).CellStyle = centeredStyle;
            headerRow.GetCell(6).CellStyle = centeredStyle;
            headerRow.GetCell(7).CellStyle = centeredStyle;

            for (int i = 0; i < records.Count; i++)
            {
                var row = sheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(i + 1);
                row.CreateCell(1).SetCellValue(records[i].workDate);
                row.CreateCell(2).SetCellValue(records[i].employeeName);
                row.CreateCell(3).SetCellValue(records[i].employeeId);
                row.CreateCell(4).SetCellValue(records[i].startTime);
                row.CreateCell(5).SetCellValue(records[i].endTime);
                row.CreateCell(6).SetCellValue(records[i].countTime);
                row.CreateCell(7).SetCellValue(records[i].companyName);

                row.GetCell(0).CellStyle = centeredStyle;
                row.GetCell(1).CellStyle = centeredStyle;
                row.GetCell(2).CellStyle = centeredStyle;
                row.GetCell(3).CellStyle = centeredStyle;
                row.GetCell(4).CellStyle = centeredStyle;
                row.GetCell(5).CellStyle = centeredStyle;
                row.GetCell(6).CellStyle = centeredStyle;
                row.GetCell(7).CellStyle = centeredStyle;
            }

            // **設定所有欄位自適應寬度**
            for (int col = 0; col < 8; col++)
            {
                sheet.SetColumnWidth(col, 20 * 256);
            }

            var memoryStream = new MemoryStream();

            workbook.Write(memoryStream);
            byte[] bytes = memoryStream.ToArray();
            return bytes;

        }


        public async Task<byte[]> GetLeaveRecordsExcel(string employeeName, DateTime? startDate, DateTime? endDate)
        {

            var query = _context.LeaveRecords
                .Include(lr => lr.Employee)
                .ThenInclude(e => e.EmployeeDetail)
                .AsQueryable();

            if (!string.IsNullOrEmpty(employeeName))
            {
                query = query.Where(lr => lr.Employee.EmployeeDetail.EmployeeName.Contains(employeeName));
            }
            if (startDate.HasValue)
            {
                query = query.Where(lr => lr.StartTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(lr => lr.EndTime <= endDate.Value);
            }

            var records = await query
                .OrderBy(lr => lr.EmployeeId.StartsWith("E") ? 0 : 1) // 正式員工 (E) 優先
                .ThenBy(lr => lr.LeaveStatus) // 未核准 (0) 優先
                .ThenByDescending(lr => lr.StartTime) // 開始時間降序
                .Select(lr => new
                {
                    id = lr.EmployeeId + lr.StartTime.ToString("yyyyMMddHHmmss"),
                    photo = lr.Employee.EmployeeDetail.EmployeePhoto != null
                        ? Convert.ToBase64String(lr.Employee.EmployeeDetail.EmployeePhoto)
                        : null,
                    employeeName = lr.Employee.EmployeeDetail.EmployeeName,
                    employeeId = lr.EmployeeId,
                    startDate = lr.StartTime.ToString("yyyy/MM/dd"),
                    startTime = lr.StartTime.ToString("HH:mm"),
                    endDate = lr.EndTime.ToString("yyyy/MM/dd"),
                    endTime = lr.EndTime.ToString("HH:mm"),
                    status = lr.LeaveStatus,
                    leaveType = "特休",
                    countTime = lr.SpentHours
                })
                .ToListAsync();


            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Products");
            // 設定置中樣式
            ICellStyle centeredStyle = workbook.CreateCellStyle();
            centeredStyle.Alignment = HorizontalAlignment.Center;  // 水平置中
            centeredStyle.VerticalAlignment = VerticalAlignment.Center;  // 垂直置中

            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("序號");
            headerRow.CreateCell(1).SetCellValue("員工姓名");
            headerRow.CreateCell(2).SetCellValue("開始時間");
            headerRow.CreateCell(3).SetCellValue("結束時間");
            headerRow.CreateCell(4).SetCellValue("狀態");
            headerRow.CreateCell(5).SetCellValue("假別");
            headerRow.CreateCell(6).SetCellValue("總時數");

            headerRow.GetCell(0).CellStyle = centeredStyle;
            headerRow.GetCell(1).CellStyle = centeredStyle;
            headerRow.GetCell(2).CellStyle = centeredStyle;
            headerRow.GetCell(3).CellStyle = centeredStyle;
            headerRow.GetCell(4).CellStyle = centeredStyle;
            headerRow.GetCell(5).CellStyle = centeredStyle;
            headerRow.GetCell(6).CellStyle = centeredStyle;

            for (int i = 0; i < records.Count; i++)
            {
                var row = sheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(i + 1);
                row.CreateCell(1).SetCellValue(records[i].employeeName);
                row.CreateCell(2).SetCellValue(records[i].startTime);
                row.CreateCell(3).SetCellValue(records[i].endTime);
                row.CreateCell(4).SetCellValue(records[i].status ? "完成" : "處理中");
                row.CreateCell(5).SetCellValue(records[i].leaveType);
                row.CreateCell(6).SetCellValue(records[i].countTime);

                row.GetCell(0).CellStyle = centeredStyle;
                row.GetCell(1).CellStyle = centeredStyle;
                row.GetCell(2).CellStyle = centeredStyle;
                row.GetCell(3).CellStyle = centeredStyle;
                row.GetCell(4).CellStyle = centeredStyle;
                row.GetCell(5).CellStyle = centeredStyle;
                row.GetCell(6).CellStyle = centeredStyle;
            }

            // **設定所有欄位自適應寬度**
            for (int col = 0; col < 7; col++)
            {
                sheet.SetColumnWidth(col, 15 * 256);
            }

            var memoryStream = new MemoryStream();

            workbook.Write(memoryStream);
            byte[] bytes = memoryStream.ToArray();
            return bytes;
        }


    }
}
