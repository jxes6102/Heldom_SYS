using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Heldom_SYS.Models;
using Heldom_SYS.Service;
using Heldom_SYS.Interface;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Dynamic;
using static Heldom_SYS.Service.AccidentService;


namespace Heldom_SYS.Controllers
{
    public class DashboardController : Controller
    {

        //Dashboard
        public IActionResult Dashboard()
        {
            string? userId = HttpContext.Session.GetString("UserID");
            string? role = HttpContext.Session.GetString("Role");
            string? userName = HttpContext.Session.GetString("UserName");

            ViewBag.userId = userId;
            ViewBag.Role = role;
            ViewBag.userName = userName;
            return View();
        }
        private readonly SqlConnection DataBase;
        private readonly IUserStoreService UserRoleStore;

        public DashboardController(SqlConnection connection, IUserStoreService _UserRoleStore)
        {
            DataBase = connection;
            UserRoleStore = _UserRoleStore;
        }


        //現場作業人員
        public async Task<IActionResult> GetActiveWorkingCount()
        {
            string query = @"
            SELECT COUNT(DISTINCT e.EmployeeID) AS ActiveWorkingCount
            FROM Employee e
            JOIN AttendanceRecord ar ON e.EmployeeID = ar.EmployeeID
            WHERE e.IsActive = 1
            AND ar.CheckInTime >= CAST(GETDATE() AS DATE)
            AND ar.CheckInTime < DATEADD(DAY, 1, CAST(GETDATE() AS DATE))
            AND ar.CheckOutTime IS NULL;";

            int count = await DataBase.ExecuteScalarAsync<int>(query);

            return Json(new { count });
        }
        //現場作業人員 popup 表格
        public async Task<IActionResult> GetActiveWorkers(int page = 1)
        {
            string query = @"
            SELECT 
                e.EmployeeID,
                COALESCE(ed.EmployeeName, t.EmployeeName) AS EmployeeName,
                COALESCE(ed.EmployeeID, t.EmployeeID) AS EmployeeID,
                ed.EmployeePhoto,
                ar.CheckInTime,
                ar.CheckOutTime
            FROM Employee e
            JOIN AttendanceRecord ar ON e.EmployeeID = ar.EmployeeID
            LEFT JOIN EmployeeDetail ed ON e.EmployeeID = ed.EmployeeID
            LEFT JOIN Temporarier t ON e.EmployeeID = t.EmployeeID
            WHERE e.IsActive = 1
            AND (
                (ar.CheckInTime >= CAST(GETDATE() AS DATE) AND ar.CheckInTime < DATEADD(DAY, 1, CAST(GETDATE() AS DATE)))
                 OR 
                (ar.CheckOutTime >= CAST(GETDATE() AS DATE) AND ar.CheckOutTime < DATEADD(DAY, 1, CAST(GETDATE() AS DATE)))
                )
            ORDER BY ar.CheckInTime ASC
            OFFSET (@Page - 1) * 10 ROWS FETCH NEXT 10 ROWS ONLY;
            ";
            //AND ar.CheckInTime >= CAST(GETDATE() AS DATE)
            //AND ar.CheckInTime < DATEADD(DAY, 1, CAST(GETDATE() AS DATE))

            var workers = await DataBase.QueryAsync(query, new { Page = page });

            dynamic response = new ExpandoObject();
            response.data = workers;

            string countSql = @"
                SELECT 
                    count(*) as Total
                FROM Employee e
                JOIN AttendanceRecord ar ON e.EmployeeID = ar.EmployeeID
                LEFT JOIN EmployeeDetail ed ON e.EmployeeID = ed.EmployeeID
                LEFT JOIN Temporarier t ON e.EmployeeID = t.EmployeeID
                WHERE e.IsActive = 1
                AND (
                    (ar.CheckInTime >= CAST(GETDATE() AS DATE) AND ar.CheckInTime < DATEADD(DAY, 1, CAST(GETDATE() AS DATE)))
                     OR 
                    (ar.CheckOutTime >= CAST(GETDATE() AS DATE) AND ar.CheckOutTime < DATEADD(DAY, 1, CAST(GETDATE() AS DATE)))
                    );";


            IEnumerable<PageData>? data = await DataBase.QueryAsync<PageData>(countSql);

            var result = data.Select(x => x.Total).ToList().First();
            int count = int.Parse(result);

            response.pageCount = count;

            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");

        }


        //人員狀態
        public async Task<IActionResult> GetWorkerCounts()
        {
            string query = @"
            -- 統計各公司人數
            SELECT 
                t.CompanyID,
                COUNT(DISTINCT e.EmployeeID) AS WorkerCount
            FROM Employee e
            JOIN AttendanceRecord ar ON e.EmployeeID = ar.EmployeeID
            JOIN Temporarier t ON e.EmployeeID = t.EmployeeID  -- 取得 CompanyID
            WHERE e.IsActive = 1
            AND ar.CheckInTime >= CAST(GETDATE() AS DATE)
            AND ar.CheckInTime < DATEADD(DAY, 1, CAST(GETDATE() AS DATE))
            AND ar.CheckOutTime IS NULL
            GROUP BY t.CompanyID;

            -- 其他類別 (PositionRole = 'e')
            SELECT 
                COUNT(DISTINCT e.EmployeeID) AS OthersCount
            FROM Employee e
            JOIN AttendanceRecord ar ON e.EmployeeID = ar.EmployeeID
            WHERE e.IsActive = 1
            AND ar.CheckInTime >= CAST(GETDATE() AS DATE)
            AND ar.CheckInTime < DATEADD(DAY, 1, CAST(GETDATE() AS DATE))
            AND ar.CheckOutTime IS NULL
            AND e.PositionRole IN ('e', 'm');"; 
            // ["others"] 統計 'e' 和 'm'

            using (var multi = await DataBase.QueryMultipleAsync(query))
            {
                var companyCounts = await multi.ReadAsync();
                int othersCount = await multi.ReadFirstOrDefaultAsync<int>();

                var result = new Dictionary<string, int>();

                foreach (var item in companyCounts)
                {
                    result[item.CompanyID] = item.WorkerCount;
                }

                result["others"] = othersCount;

                return Json(result);
            }
        }

        //臨時工入場待核可
        public async Task<IActionResult> GetTempWorkerCount()
        {
            string query = @"
            SELECT COUNT(*) AS TempWorkerCount
            FROM Employee
            WHERE IsActive = 0
            AND PositionRole = 'p';";

            int count = await DataBase.ExecuteScalarAsync<int>(query);

            return Json(new { count });
        }

        //臨時工入場待核可 pop-up
        public async Task<IActionResult> GetPendingTempWorkers()
        {
            string query = @"
        SELECT 
            t.EmployeeID,
            t.EmployeeName,
            t.PhoneNumber,
            c.CompanyName
        FROM Employee e
        JOIN Temporarier t ON e.EmployeeID = t.EmployeeID
        JOIN Company c ON t.CompanyID = c.CompanyID
        WHERE e.IsActive = 0
        AND e.PositionRole = 'p'
        ORDER BY t.EmployeeName;";

            var tempWorkers = await DataBase.QueryAsync(query);
            return Json(tempWorkers);
        }
        // 改變 IsActive 狀態 0 => 1
        [HttpPost]
        public async Task<IActionResult> ApproveTempWorkers([FromBody] List<string> employeeIds)
        {
            if (employeeIds == null || employeeIds.Count == 0)
            {
                return BadRequest("沒有選擇任何員工");
            }

            string query = "UPDATE Employee SET IsActive = 1 WHERE EmployeeID IN @EmployeeIDs";

            int rowsAffected = await DataBase.ExecuteAsync(query, new { EmployeeIDs = employeeIds });

            return Json(new { success = true, updatedCount = rowsAffected });
        }


        //未處理異常

        //取得異常數量
        public async Task<IActionResult> GetIssueCounts()
        {
            string queryA = @"
        SELECT COUNT(*) AS IssueCount
        FROM Accident
        WHERE IncidentStatus = 0;";

            string queryM = @"
        SELECT COUNT(*) AS IssueCount
        FROM Accident
        WHERE IncidentStatus = 0
        AND IncidentControllerID = @EmployeeID;";

            int totalIssues = await DataBase.ExecuteScalarAsync<int>(queryA);
            int managerIssues = await DataBase.ExecuteScalarAsync<int>(queryM, new { EmployeeID = UserRoleStore.UserID });

            var IssuesCount = new { A = totalIssues, M = managerIssues };
            return Ok(IssuesCount);
            //return Ok(new { A = totalIssues, M = managerIssues });
        }

        //未處理異常 Pop-up

        //表格
        public async Task<IActionResult> GetPendingIssues(int page = 1)
        {
            string query = @"
                SELECT 
                    a.AccidentID,
                    a.AccidentType,
                    a.AccidentTitle,
                    FORMAT(a.StartTime, 'yyyy-MM-dd HH:mm') AS StartTime,
                    FORMAT(a.UploadTime, 'yyyy-MM-dd HH:mm') AS UploadTime,
                    e.EmployeeName, 
                    a.IncidentControllerID
                FROM Accident a JOIN EmployeeDetail e ON a.EmployeeID = e.EmployeeID
                WHERE a.IncidentStatus = 0";

            if (UserRoleStore.GetRole() == "M") {
                query += @" and a.IncidentControllerID = @IncidentControllerID";
            }

            query += @" ORDER BY a.StartTime DESC
                OFFSET (@Page - 1) * 10 ROWS FETCH NEXT 10 ROWS ONLY;";


            var issues = await DataBase.QueryAsync(query, new { Page = page, IncidentControllerID = UserRoleStore.UserID });

            dynamic response = new ExpandoObject();
            response.data = issues;

            string countSql = @"
                SELECT 
                    count(*) as Total
                FROM Accident a JOIN EmployeeDetail e ON a.EmployeeID = e.EmployeeID
                WHERE a.IncidentStatus = 0 ";

            if (UserRoleStore.GetRole() == "M")
            {
                countSql += @" and a.IncidentControllerID = @IncidentControllerID";
            }

            IEnumerable<PageData>? data = await DataBase.QueryAsync<PageData>(countSql,
                new
                {
                    IncidentControllerID = UserRoleStore.UserID
                });

            var result = data.Select(x => x.Total).ToList().First();
            int count = int.Parse(result);

            response.pageCount = count;

            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");

        }

        //Cctv
        public IActionResult Cctv(int? index)
        {
            ViewData["InitialIndex"] = index ?? 0;
            return View();
        }

        //public IActionResult test()
        //{
        //    return View();
        //}

    }
}
