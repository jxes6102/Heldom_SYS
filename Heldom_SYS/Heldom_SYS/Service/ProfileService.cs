using System.Reflection;
using Dapper;
using Heldom_SYS.CustomModel;
using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NPOI.HSSF.Record.Chart;
using NPOI.SS.Formula.Functions;
using static Heldom_SYS.Controllers.ProfileController;
using static Heldom_SYS.Service.AccidentService;

namespace Heldom_SYS.Service
{
    public class ProfileService : IProfileService
    {
        private readonly SqlConnection DataBase;
        private readonly IUserStoreService UserRoleStore;
        private readonly ConstructionDbContext DbContext;
        public ProfileService(SqlConnection connection, IUserStoreService _UserRoleStore, ConstructionDbContext dbContext)
        {
            DataBase = connection;
            UserRoleStore = _UserRoleStore;
            DbContext = dbContext;
        }
        
        // 查詢員工個人詳細資料
        public async Task<IEnumerable<ProfileIndex>> GetIndexData()
        {
            string userID = UserRoleStore.UserID;
            var employeeWithDetail = await DbContext.Employees
                .Where(employee => employee.EmployeeId == userID)
                .Join(DbContext.EmployeeDetails,
                employee => employee.EmployeeId,
                detail => detail.EmployeeId,
                (employee, detail) => new ProfileIndex
                {
                    employeeName = detail.EmployeeName,
                    position = employee.Position,
                    employeeId = employee.EmployeeId,
                    birthDate = detail.BirthDate,
                    phoneNumber = detail.PhoneNumber,
                    address = detail.Address,
                    emergencyContact = detail.EmergencyContact,
                    emergencyContactPhone = detail.EmergencyContactPhone,
                    Department = detail.Department,
                    EmployeePhoto = Convert.ToBase64String(detail.EmployeePhoto),
                    Mail = detail.Mail,
                    AnnualLeave = detail.AnnualLeave,
                    HireDate = employee.HireDate,
                    YearsBetween = (int)((employee.ResignationDate ?? DateTime.Now) - employee.HireDate).TotalDays / 365
                }).ToListAsync();
            return employeeWithDetail;
        }

        // 顯示員工個人資料
        public async Task<IEnumerable<ProfileSettings>> GetSettingsData()
        {
            try
            {
                string userID = UserRoleStore.UserID;

                var employeeWithDetail = await DbContext.Employees
                    .Where(employee => employee.EmployeeId == userID)
                    .Join(DbContext.EmployeeDetails,
                    employee => employee.EmployeeId,
                    detail => detail.EmployeeId,
                    (employee, detail) => new ProfileSettings
                    {
                        employeeName = detail.EmployeeName,
                        position = employee.Position,
                        employeeId = employee.EmployeeId,
                        birthDate = detail.BirthDate,
                        phoneNumber = detail.PhoneNumber,
                        address = detail.Address,
                        emergencyContact = detail.EmergencyContact,
                        emergencyContactPhone = detail.EmergencyContactPhone
                    }).ToListAsync();
                return employeeWithDetail;
            }
            catch (Exception ex)
            {
                throw new Exception("資料取得失敗: " + ex.Message);
            }
        }

        // 更新員工個人資料
        public async Task<bool> UpdateSettingsData(EmployeeDetailUpdateModel userInput)
        {
            try
            {
                string userID = UserRoleStore.UserID;

                var employeeDetail = await DbContext.EmployeeDetails
                    .FirstOrDefaultAsync(ed => ed.EmployeeId == userID);

                if (employeeDetail != null)
                {
                    employeeDetail.PhoneNumber = userInput.phoneNumber;
                    employeeDetail.Address = userInput.address;
                    employeeDetail.EmergencyContact = userInput.emergencyContact;
                    employeeDetail.EmergencyContactPhone = userInput.emergencyContactPhone;

                    await DbContext.SaveChangesAsync();
                    return true;
                }
                else
                {
                    throw new Exception("找不到員工資料");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("更新失敗: " + ex.Message);
            }
        }

        // 查詢員工們的個人資料
        public async Task<IEnumerable<ProfileAccount>> GetAccountsData(ProfileOptions options)
        {
            string userID = UserRoleStore.UserID;

            string sql = @"SELECT
                            CASE 
                                WHEN ed.EmployeePhoto IS NOT NULL 
                                THEN CAST(N'' AS XML).value('xs:base64Binary(xs:hexBinary(sql:column(""ed.EmployeePhoto"")))', 'VARCHAR(MAX)')
                                ELSE NULL 
                            END as EmployeePhoto
                            ,ed.EmployeeName,ed.EmployeeID,ed.PhoneNumber,
	                        ed.Department,e.Position,e.HireDate,e.IsActive
                            FROM Employee e join EmployeeDetail ed 
                            ON e.EmployeeID = ed.EmployeeID
                            WHERE 1 = 1";
            
            if (!options.IsActive.IsNullOrEmpty())
            {
                sql += @" and e.IsActive = @IsActive";
            }

            if (!options.Department.IsNullOrEmpty())
            {
                sql += @" and ed.Department = @Department";
            }

            if (!options.EmployeeId.IsNullOrEmpty())
            {
                sql += @" and e.EmployeeId = @EmployeeId";
            }

            if (!options.EmployeeName.IsNullOrEmpty())
            {
                sql += @" and ed.EmployeeName like @EmployeeName";
            }

            sql += @" ORDER BY e.EmployeeID ASC
                        OFFSET( @Page - 1) * 10 ROWS
                        FETCH NEXT 10 ROWS ONLY";

            try
            {

                IEnumerable<ProfileAccount>? employeesWithDetail = await DataBase.QueryAsync<ProfileAccount>(sql,
                    new
                    {
                        IsActive = options.IsActive,
                        Department = options.Department,
                        EmployeeId = options.EmployeeId,
                        EmployeeName = $"%{options.EmployeeName}%",
                        Page = options.currentPage,
                    });

                foreach(var employee in employeesWithDetail)
                {
                    employee.EmployeePhoto = $"data:image/jpeg;base64,{employee.EmployeePhoto}";
                }

                return employeesWithDetail;
            }
            catch (Exception ex)
            {
                throw new Exception("資料取得失敗: " + ex.Message);
            }
        }

        public class PageNumber
        {
            public required string totalPage { get; set; }
        }

        public async Task<int> GetTotalPage(ProfileOptions options)
        {
            string sql = @"SELECT COUNT(*) as Total
                            FROM Employee e join EmployeeDetail ed 
                            ON e.EmployeeID = ed.EmployeeID
                            WHERE 1 = 1";

            if (!options.IsActive.IsNullOrEmpty())
            {
                sql += @" and e.IsActive = @IsActive";
            }

            if (!options.Department.IsNullOrEmpty())
            {
                sql += @" and ed.Department = @Department";
            }

            if (!options.EmployeeId.IsNullOrEmpty())
            {
                sql += @" and e.EmployeeId = @EmployeeId";
            }

            if (!options.EmployeeName.IsNullOrEmpty())
            {
                sql += @" and ed.EmployeeName like @EmployeeName";
            }

            try
            {
                int total = await DataBase.QueryFirstAsync<int>(sql,
                        new
                        {
                            IsActive = options.IsActive,
                            Department = options.Department,
                            EmployeeId = options.EmployeeId,
                            EmployeeName = $"%{options.EmployeeName}%",
                            Page = options.currentPage,
                        });
                int totalPage = (int)Math.Ceiling(total / 10.0);
                return totalPage;
            }
            catch(Exception ex)
            {
                throw new Exception("總頁數取得失敗: " + ex.Message);
            }
        }

        // 取得新帳號 Id
        public class NewAccountId
        {
            public required string LatestId { get; set; }
        }

        public async Task<string> GetNewId()
        {
            string sql = "SELECT TOP 1 EmployeeID FROM Employee WHERE EmployeeID like 'E%' ORDER BY EmployeeID DESC";
            try
            {
                string NewId = await DataBase.QueryFirstAsync<string>(sql);
                int NewIdNum = int.Parse(NewId.Substring(1));
                NewIdNum = NewIdNum + 1;
                NewId = "E" + NewIdNum.ToString().PadLeft(5, '0');
                return NewId;
            }
            catch(Exception ex)
            {
                throw new Exception("Id 取得失敗: "+ ex.Message);
            }
        }
        public async Task<IEnumerable<ProfileNewAccountData>> GetSupervisor()
        {
            string sql = "SELECT e.EmployeeID, ed.EmployeeName " +
                "FROM Employee e JOIN EmployeeDetail ed ON e.EmployeeID = ed.EmployeeID " +
                "WHERE e.PositionRole = 'M' OR e.PositionRole = 'A'";
            try
            {
                IEnumerable<ProfileNewAccountData> ISupervisor = await DataBase.QueryAsync<ProfileNewAccountData>(sql);

                return ISupervisor;
            }
            catch (Exception ex)
            {
                throw new Exception("Id 取得失敗: " + ex.Message);
            }
        }
        // 新增員工個人帳號資料
        public async Task<string> CreateAccount(GetNewAccountEditData userInput)
        {
            try
            {
                    string sqlCheck = "SELECT COUNT([PhoneNumber]) FROM EmployeeDetail WHERE PhoneNumber = @PhoneNumber";
                    string sqlCheck2 = "SELECT COUNT([Mail]) FROM EmployeeDetail WHERE Mail = @Mail";

                    int checkResult = await DataBase.QueryFirstAsync<int>(sqlCheck,
                            new
                            {
                                PhoneNumber = userInput.PhoneNumber
                            });

                    int checkResult2 = await DataBase.QueryFirstAsync<int>(sqlCheck2,
                        new
                        {
                            Mail = userInput.Mail
                        });

                    if (checkResult != 0)
                    {
                        return "電話號碼已存在!";
                    }else if(checkResult2 != 0)
                    {
                        return "電子信箱已存在！";
                    }
                
                    var employee = new Employee
                    {
                        EmployeeId = userInput.EmployeeId,
                        IsActive = userInput.IsActive,
                        Position = userInput.Position,
                        PositionRole = userInput.PositionRole,
                        HireDate = userInput.HireDate,
                        ResignationDate = userInput.ResignationDate
                    };

                await DbContext.Employees.AddAsync(employee);
                int affectedRows = await DbContext.SaveChangesAsync();
                if (affectedRows == 0)
                {
                    return "Employee表格建立失敗！";
                }

                try
                {
                    byte[] photo = Convert.FromBase64String(userInput.EmployeePhoto);

                    var employeeDetail = new EmployeeDetail
                    {
                        EmployeeId = userInput.EmployeeId,
                        Department = userInput.Department,
                        ImmediateSupervisor = (userInput.ImmediateSupervisor == "總經理" ? null: userInput.ImmediateSupervisor),
                        EmployeePhoto = photo,
                        EmployeeName = userInput.EmployeeName,
                        PhoneNumber = userInput.PhoneNumber,
                        Mail = userInput.Mail,
                        Password = userInput.Password,
                        Address = userInput.Address,
                        Gender = userInput.Gender,
                        BirthDate = userInput.BirthDate,
                        EmergencyContact = userInput.EmergencyContact,
                        EmergencyRelationship = userInput.EmergencyRelationship,
                        EmergencyContactPhone = userInput.EmergencyContactPhone,
                        AnnualLeave = (byte)userInput.AnnualLeave
                    };

                    await DbContext.EmployeeDetails.AddAsync(employeeDetail);
                    affectedRows = await DbContext.SaveChangesAsync();
                    if (affectedRows == 0)
                    {
                        return "EmployeeDetail表格建立失敗！";
                    }
                    return "員工檔案建立成功！";
                }
                catch (Exception ex)
                {
                    throw new Exception("EmployeeDetail表格更新失敗: " + ex.Message);
                }
            }

            catch (Exception ex)
            {
                throw new Exception("Employee表格更新失敗: " + ex.Message);
            }
        }

            // 更新員工個人帳號資料 的 GET & UPDATE
        public async Task<IEnumerable<GetNewAccountEditData>> GetAccountData(string employeeId)
        {
            try
            {
                string userID = employeeId;

                var employeesWithDetail = await DbContext.Employees
                    .Where(employee => employee.EmployeeId == userID)
                    .Join(DbContext.EmployeeDetails,
                    employee => employee.EmployeeId,
                    detail => detail.EmployeeId,
                    (employee, detail) => new GetNewAccountEditData
                    {
                        EmployeePhoto = Convert.ToBase64String(detail.EmployeePhoto),
                        EmployeeName = detail.EmployeeName,
                        Gender = detail.Gender,
                        BirthDate = detail.BirthDate,
                        PhoneNumber = detail.PhoneNumber,
                        EmergencyContact = detail.EmergencyContact,
                        EmergencyRelationship = detail.EmergencyRelationship,
                        EmergencyContactPhone = detail.EmergencyContactPhone,
                        HireDate = employee.HireDate,
                        IsActive = employee.IsActive,
                        EmployeeId = employee.EmployeeId,
                        PositionRole = employee.PositionRole,
                        Department = detail.Department,
                        Position = employee.Position,
                        ImmediateSupervisor = detail.ImmediateSupervisor,
                        Address = detail.Address,
                        Mail = detail.Mail,
                        Password = detail.Password,
                        ResignationDate = employee.ResignationDate
                    }).ToListAsync();
                return employeesWithDetail;
            }
            catch (Exception ex)
            {
                throw new Exception("資料取得失敗: " + ex.Message);
            }
        }

        public async Task<string> UpdateAccount(GetNewAccountEditData userInput)
        {
            try
            {
                string sql = "UPDATE Employee SET [IsActive] = @IsActive," +
                    "[Position] = @Position," +
                    "[PositionRole] = @PositionRole," +
                    "[HireDate] = @HireDate," +
                    "[ResignationDate] = @ResignationDate " +
                    "WHERE EmployeeID = @EmployeeID";

                string sql2 = "UPDATE EmployeeDetail SET [Department] = @Department," +
                    "[ImmediateSupervisor] = @ImmediateSupervisor," +
                    "[EmployeePhoto] = @EmployeePhoto," +
                    "[EmployeeName] = @EmployeeName," +
                    "[PhoneNumber] = @PhoneNumber," +
                    "[Mail] = @Mail," +
                    "[Password] = @Password," +
                    "[Address] = @Address," +
                    "[Gender] = @Gender," +
                    "[BirthDate] = @BirthDate," +
                    "[EmergencyContact] = @EmergencyContact," +
                    "[EmergencyRelationship] = @EmergencyRelationship," +
                    "[EmergencyContactPhone] = @EmergencyContactPhone," +
                    "[AnnualLeave] = @AnnualLeave " +
                    "WHERE EmployeeID = @EmployeeID";

                string sqlCheck = "SELECT COUNT([PhoneNumber]) FROM EmployeeDetail WHERE PhoneNumber = @PhoneNumber and EmployeeID NOT IN (@EmployeeID)";
                string sqlCheck2 = "SELECT COUNT([Mail]) FROM EmployeeDetail WHERE Mail = @Mail and EmployeeID NOT IN (@EmployeeID)";

                try
                {
                    int checkResult = await DataBase.QueryFirstAsync<int>(sqlCheck,
                        new
                        {
                            PhoneNumber = userInput.PhoneNumber,
                            EmployeeID = userInput.EmployeeId
                        });

                    int checkResult2 = await DataBase.QueryFirstAsync<int>(sqlCheck2,
                        new
                        {
                            Mail = userInput.Mail,
                            EmployeeID = userInput.EmployeeId
                        });

                    if (checkResult != 0)
                    {
                        return "電話號碼已存在!";
                    }
                    else if (checkResult2 != 0)
                    {
                        return "電子信箱已存在！";
                    }

                    var rowsAffected = await DataBase.ExecuteAsync(sql,
                        new
                        {
                            IsActive = userInput.IsActive,
                            Position = userInput.Position,
                            PositionRole = userInput.PositionRole,
                            HireDate = userInput.HireDate,
                            ResignationDate = userInput.ResignationDate,
                            EmployeeID = userInput.EmployeeId
                        });

                    if (rowsAffected == 0)
                    {
                        return "Employee表格更新失敗！";
                    }

                    rowsAffected = await DataBase.ExecuteAsync(sql2,
                        new
                        {
                            Department = userInput.Department,
                            ImmediateSupervisor = userInput.ImmediateSupervisor,
                            EmployeePhoto = Convert.FromBase64String(userInput.EmployeePhoto),
                            EmployeeName = userInput.EmployeeName,
                            PhoneNumber = userInput.PhoneNumber,
                            Mail = userInput.Mail,
                            Password = userInput.Password,
                            Address = userInput.Address,
                            Gender = userInput.Gender,
                            BirthDate = userInput.BirthDate,
                            EmergencyContact = userInput.EmergencyContact,
                            EmergencyRelationship = userInput.EmergencyRelationship,
                            EmergencyContactPhone = userInput.EmergencyContactPhone,
                            AnnualLeave = userInput.AnnualLeave,
                            EmployeeID = userInput.EmployeeId
                        });

                    if (rowsAffected == 0)
                    {
                        return "EmployeeDetail表格更新失敗！";
                    }

                    return "資料更新完畢！";
                }
                catch (Exception ex)
                {
                    throw new Exception("資料取得失敗: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("更新失敗: " + ex.Message);
            }
        }
    }
}
