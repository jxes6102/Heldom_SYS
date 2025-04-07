using Dapper;
using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using MathNet.Numerics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NPOI.HSSF.Record;
using NPOI.SS.Formula.Functions;
using System.Reflection;
using System;
using Heldom_SYS.CustomModel;

namespace Heldom_SYS.Controllers
{
    public class LoginController : Controller
    {
        private readonly SqlConnection DataBase;
        private readonly IUserStoreService UserRoleStore;
        public LoginController(SqlConnection connection, IUserStoreService _UserRoleStore)
        {
            DataBase = connection;
            UserRoleStore = _UserRoleStore;
        }

        public IActionResult Index()
        {
            UserRoleStore.UserID = "";
            UserRoleStore.UserName = "";
            UserRoleStore.SetRole("X");
            return View();
        }

        public class LogInData
        {
            public required string Type { get; set; }
            public required string Account { get; set; }
            public required string PassWord { get; set; }
            public required string CompanyID { get; set; }
            
        }

        public class PIDData
        {
            public required string EmployeeID { get; set; }

        }

        [HttpPost]
        public async Task<IActionResult> Enter([FromBody] LogInData data)
        {
            UserRoleStore.UserID = "";
            UserRoleStore.UserName = "";
            UserRoleStore.SetRole("X");
            //await DataBase.OpenAsync();

            if (data.Type == "visitor") {
                
                string sql = @"SELECT * FROM Temporarier
                    where EmployeeName = @EmployeeName and PhoneNumber = @PhoneNumber"
                ;

                //await _DataBase.QueryAsync<type>(sql);
                Temporarier? user = await DataBase.QueryFirstOrDefaultAsync<Temporarier>(sql, 
                    new { 
                        EmployeeName = data.Account,
                        PhoneNumber = data.PassWord,
                    });

                if (user != null)
                {
                    UserRoleStore.UserID = user.EmployeeId;
                    UserRoleStore.UserName = user.EmployeeName;
                }
                else {
                    string checkID = @"SELECT top(1) EmployeeID FROM Employee where PositionRole = 'P' order by EmployeeID Desc";
                    IEnumerable<PIDData> ? PIDData = await DataBase.QueryAsync<PIDData>(checkID);

                    // 新增最新PID
                    string resultID = PIDData.Select(x => x.EmployeeID).ToList().First().ToString();
                    int count = int.Parse(resultID.Substring(1)) + 1;
                    string EmployeeID = "P" + count.ToString().PadLeft(5, '0');


                    string addEmployeeSql = @"INSERT INTO Employee (EmployeeID,IsActive,Position,PositionRole,HireDate,ResignationDate)
                                VALUES (@EmployeeID,@IsActive,@Position,@PositionRole,@HireDate,@ResignationDate)";

                    await DataBase.QuerySingleOrDefaultAsync<int>(addEmployeeSql, new
                    {
                        EmployeeID = EmployeeID,
                        IsActive = false,
                        Position = "臨時員工",
                        PositionRole = "P",
                        HireDate = DateTime.Now,
                        ResignationDate = DateTime.Now,
                    });


                    string addTemporarierSql = @"INSERT INTO Temporarier(EmployeeID,EmployeeName,PhoneNumber,CompanyID) 
                                            VALUES  (@EmployeeID,@EmployeeName,@PhoneNumber,@CompanyID)";

                    await DataBase.QuerySingleOrDefaultAsync<int>(addTemporarierSql, new
                    {
                        EmployeeID = EmployeeID,
                        EmployeeName = data.Account,
                        PhoneNumber = data.PassWord,
                        CompanyID = data.CompanyID,
                    });

                    UserRoleStore.UserID = EmployeeID;
                    UserRoleStore.UserName = data.Account;

                }

            }
            else if(data.Type == "employee")
            {
                string sql = @"SELECT * FROM EmployeeDetail
                    where EmployeeName = @EmployeeName and Password = @Password"
                ;

                EmployeeDetail? user = await DataBase.QueryFirstOrDefaultAsync<EmployeeDetail>(sql,
                    new
                    {
                        EmployeeName = data.Account,
                        Password = data.PassWord,
                    });

                if (user != null)
                {
                    UserRoleStore.UserID = user.EmployeeId;
                    UserRoleStore.UserName = user.EmployeeName;
                }
            }
 
            if (!UserRoleStore.UserID.IsNullOrEmpty())
            {
                string roleSql = @"SELECT * FROM Employee where EmployeeID = @EmployeeID";
                Employee? role = await DataBase.QueryFirstOrDefaultAsync<Employee>(roleSql,
                    new
                    {
                        EmployeeID = UserRoleStore.UserID,
                    });

                if (role != null) {
                    string roleName = role.PositionRole;
                    UserRoleStore.SetRole(roleName);
                    HttpContext.Session.SetString("UserID", UserRoleStore.UserID);
                    HttpContext.Session.SetString("UserName", UserRoleStore.UserName);
                    HttpContext.Session.SetString("Role", UserRoleStore.GetRole());
                }
                
            }

            string url = "";

            if (UserRoleStore.Role == "A" || UserRoleStore.Role == "M") {
                url = "Dashboard/Dashboard";
            }
            else if (UserRoleStore.Role == "E" || UserRoleStore.Role == "P"){
                url = "Attendance/Records";
            }

            var response = new
            {
                route = url
            };

            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");

        }

    }
}
