using Dapper;
using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Data;

namespace Heldom_SYS.Controllers
{
    public class LoginController : Controller
    {
        private readonly SqlConnection DataBase;
        private readonly IUserStoreService UserRoleStore;
        private readonly ILogger<LoginController> Logger;

        public LoginController(SqlConnection connection, IUserStoreService _UserRoleStore, ILogger<LoginController> logger)
        {
            DataBase = connection;
            UserRoleStore = _UserRoleStore;
            Logger = logger;
        }

        public IActionResult Index()
        {
            HttpContext.Session.Clear();
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
                    await EnsureConnectionOpenAsync();
                    using var transaction = DataBase.BeginTransaction(IsolationLevel.Serializable);

                    try
                    {
                        string checkID = @"SELECT top(1) EmployeeID FROM Employee WITH (UPDLOCK, HOLDLOCK) where PositionRole = 'P' order by EmployeeID Desc";
                        string? resultID = await DataBase.QueryFirstOrDefaultAsync<string>(checkID, transaction: transaction);
                        int count = 1;
                        if (!string.IsNullOrEmpty(resultID) && resultID.Length > 1)
                        {
                            int.TryParse(resultID.Substring(1), out count);
                            count++;
                        }
                        string EmployeeID = "P" + count.ToString().PadLeft(5, '0');


                        string addEmployeeSql = @"INSERT INTO Employee (EmployeeID,IsActive,Position,PositionRole,HireDate,ResignationDate)
                                VALUES (@EmployeeID,@IsActive,@Position,@PositionRole,@HireDate,@ResignationDate)";

                        await DataBase.ExecuteAsync(addEmployeeSql, new
                        {
                            EmployeeID = EmployeeID,
                            IsActive = false,
                            Position = "臨時員工",
                            PositionRole = "P",
                            HireDate = DateTime.Now,
                            ResignationDate = DateTime.Now,
                        }, transaction);


                        string addTemporarierSql = @"INSERT INTO Temporarier(EmployeeID,EmployeeName,PhoneNumber,CompanyID) 
                                            VALUES  (@EmployeeID,@EmployeeName,@PhoneNumber,@CompanyID)";

                        await DataBase.ExecuteAsync(addTemporarierSql, new
                        {
                            EmployeeID = EmployeeID,
                            EmployeeName = data.Account,
                            PhoneNumber = data.PassWord,
                            CompanyID = data.CompanyID,
                        }, transaction);

                        transaction.Commit();
                        UserRoleStore.UserID = EmployeeID;
                        UserRoleStore.UserName = data.Account;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Logger.LogError(ex, "Failed to create temporary worker account for {EmployeeName}", data.Account);
                        return StatusCode(500, new { message = "臨時員工建立失敗" });
                    }

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
 
            if (!string.IsNullOrEmpty(UserRoleStore.UserID))
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
        private async Task EnsureConnectionOpenAsync()
        {
            if (DataBase.State != ConnectionState.Open)
            {
                await DataBase.OpenAsync();
            }
        }

    }
}
