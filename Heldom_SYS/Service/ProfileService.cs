using Heldom_SYS.CustomModel;
using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Heldom_SYS.Service
{
    public class ProfileService : IProfileService
    {
        private const int PageSize = 10;

        private readonly IUserStoreService UserRoleStore;
        private readonly ConstructionDbContext DbContext;
        private readonly ILogger<ProfileService> Logger;

        public ProfileService(IUserStoreService _UserRoleStore, ConstructionDbContext dbContext, ILogger<ProfileService> logger)
        {
            UserRoleStore = _UserRoleStore;
            DbContext = dbContext;
            Logger = logger;
        }

        // 查詢員工個人詳細資料
        public async Task<IEnumerable<ProfileIndex>> GetIndexData()
        {
            string userID = UserRoleStore.UserID;
            return await DbContext.Employees
                .AsNoTracking()
                .Where(employee => employee.EmployeeId == userID)
                .Join(DbContext.EmployeeDetails.AsNoTracking(),
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
                    })
                .ToListAsync();
        }

        // 顯示員工個人資料
        public async Task<IEnumerable<ProfileSettings>> GetSettingsData()
        {
            try
            {
                string userID = UserRoleStore.UserID;

                return await DbContext.Employees
                    .AsNoTracking()
                    .Where(employee => employee.EmployeeId == userID)
                    .Join(DbContext.EmployeeDetails.AsNoTracking(),
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
                        })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to get profile settings for employee {EmployeeId}", UserRoleStore.UserID);
                throw;
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

                if (employeeDetail == null)
                {
                    return false;
                }

                employeeDetail.PhoneNumber = userInput.phoneNumber;
                employeeDetail.Address = userInput.address;
                employeeDetail.EmergencyContact = userInput.emergencyContact;
                employeeDetail.EmergencyContactPhone = userInput.emergencyContactPhone;

                await DbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to update profile settings for employee {EmployeeId}", UserRoleStore.UserID);
                throw;
            }
        }

        // 查詢員工們的個人資料
        public async Task<IEnumerable<ProfileAccount>> GetAccountsData(ProfileOptions options)
        {
            try
            {
                int page = GetPage(options.currentPage);
                return await ApplyAccountFilters(DbContext.Employees.AsNoTracking(), options)
                    .Join(DbContext.EmployeeDetails.AsNoTracking(),
                        employee => employee.EmployeeId,
                        detail => detail.EmployeeId,
                        (employee, detail) => new ProfileAccount
                        {
                            EmployeePhoto = $"data:image/jpeg;base64,{Convert.ToBase64String(detail.EmployeePhoto)}",
                            EmployeeName = detail.EmployeeName,
                            EmployeeId = detail.EmployeeId,
                            PhoneNumber = detail.PhoneNumber,
                            Department = detail.Department,
                            Position = employee.Position,
                            HireDate = employee.HireDate,
                            IsActive = employee.IsActive
                        })
                    .OrderBy(account => account.EmployeeId)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to get profile accounts");
                throw;
            }
        }

        public class PageNumber
        {
            public required string totalPage { get; set; }
        }

        public async Task<int> GetTotalPage(ProfileOptions options)
        {
            try
            {
                int total = await ApplyAccountFilters(DbContext.Employees.AsNoTracking(), options)
                    .Join(DbContext.EmployeeDetails.AsNoTracking(),
                        employee => employee.EmployeeId,
                        detail => detail.EmployeeId,
                        (employee, detail) => employee.EmployeeId)
                    .CountAsync();

                return GetTotalPages(total);
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Failed to get profile account total pages");
                throw;
            }
        }

        // 取得新帳號 Id
        public class NewAccountId
        {
            public required string LatestId { get; set; }
        }

        public async Task<string> GetNewId()
        {
            try
            {
                string? latestEmployeeId = await DbContext.Employees
                    .AsNoTracking()
                    .Where(employee => employee.EmployeeId.StartsWith("E"))
                    .OrderByDescending(employee => employee.EmployeeId)
                    .Select(employee => employee.EmployeeId)
                    .FirstOrDefaultAsync();

                return GenerateNextPrefixedId(latestEmployeeId, "E", 5);
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Failed to get next employee id");
                throw;
            }
        }

        public async Task<IEnumerable<ProfileNewAccountData>> GetSupervisor()
        {
            try
            {
                return await DbContext.Employees
                    .AsNoTracking()
                    .Where(employee => employee.PositionRole == "M" || employee.PositionRole == "A")
                    .Join(DbContext.EmployeeDetails.AsNoTracking(),
                        employee => employee.EmployeeId,
                        detail => detail.EmployeeId,
                        (employee, detail) => new ProfileNewAccountData
                        {
                            EmployeeId = employee.EmployeeId,
                            EmployeeName = detail.EmployeeName
                        })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to get supervisors");
                throw;
            }
        }

        // 新增員工個人帳號資料
        public async Task<string> CreateAccount(GetNewAccountEditData userInput)
        {
            await using var transaction = await DbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                bool phoneExists = await DbContext.EmployeeDetails
                    .AnyAsync(detail => detail.PhoneNumber == userInput.PhoneNumber);

                bool mailExists = await DbContext.EmployeeDetails
                    .AnyAsync(detail => detail.Mail == userInput.Mail);

                if (phoneExists)
                {
                    await transaction.RollbackAsync();
                    return "電話號碼已存在!";
                }

                if (mailExists)
                {
                    await transaction.RollbackAsync();
                    return "電子信箱已存在！";
                }

                string? latestEmployeeId = await DbContext.Employees
                    .Where(employee => employee.EmployeeId.StartsWith("E"))
                    .OrderByDescending(employee => employee.EmployeeId)
                    .Select(employee => employee.EmployeeId)
                    .FirstOrDefaultAsync();

                string employeeId = GenerateNextPrefixedId(latestEmployeeId, "E", 5);

                var employee = new Employee
                {
                    EmployeeId = employeeId,
                    IsActive = userInput.IsActive,
                    Position = userInput.Position,
                    PositionRole = userInput.PositionRole,
                    HireDate = userInput.HireDate,
                    ResignationDate = userInput.ResignationDate
                };

                DbContext.Employees.Add(employee);
                int affectedRows = await DbContext.SaveChangesAsync();
                if (affectedRows == 0)
                {
                    await transaction.RollbackAsync();
                    return "Employee表格建立失敗！";
                }

                byte[] photo = Convert.FromBase64String(userInput.EmployeePhoto);

                var employeeDetail = new EmployeeDetail
                {
                    EmployeeId = employeeId,
                    Department = userInput.Department,
                    ImmediateSupervisor = userInput.ImmediateSupervisor == "總經理" ? null: userInput.ImmediateSupervisor,
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

                DbContext.EmployeeDetails.Add(employeeDetail);
                affectedRows = await DbContext.SaveChangesAsync();
                if (affectedRows == 0)
                {
                    await transaction.RollbackAsync();
                    return "EmployeeDetail表格建立失敗！";
                }

                await transaction.CommitAsync();
                return "員工檔案建立成功！";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Logger.LogError(ex, "Failed to create employee account");
                throw;
            }
        }

        // 更新員工個人帳號資料 的 GET & UPDATE
        public async Task<IEnumerable<GetNewAccountEditData>> GetAccountData(string employeeId)
        {
            try
            {
                return await DbContext.Employees
                    .AsNoTracking()
                    .Where(employee => employee.EmployeeId == employeeId)
                    .Join(DbContext.EmployeeDetails.AsNoTracking(),
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
                        })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to get employee account {EmployeeId}", employeeId);
                throw;
            }
        }

        public async Task<string> UpdateAccount(GetNewAccountEditData userInput)
        {
            await using var transaction = await DbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                bool phoneExists = await DbContext.EmployeeDetails
                    .AnyAsync(detail => detail.PhoneNumber == userInput.PhoneNumber && detail.EmployeeId != userInput.EmployeeId);

                bool mailExists = await DbContext.EmployeeDetails
                    .AnyAsync(detail => detail.Mail == userInput.Mail && detail.EmployeeId != userInput.EmployeeId);

                if (phoneExists)
                {
                    await transaction.RollbackAsync();
                    return "電話號碼已存在!";
                }

                if (mailExists)
                {
                    await transaction.RollbackAsync();
                    return "電子信箱已存在！";
                }

                Employee? employee = await DbContext.Employees
                    .FirstOrDefaultAsync(item => item.EmployeeId == userInput.EmployeeId);

                if (employee == null)
                {
                    await transaction.RollbackAsync();
                    return "Employee表格更新失敗！";
                }

                EmployeeDetail? detail = await DbContext.EmployeeDetails
                    .FirstOrDefaultAsync(item => item.EmployeeId == userInput.EmployeeId);

                if (detail == null)
                {
                    await transaction.RollbackAsync();
                    return "EmployeeDetail表格更新失敗！";
                }

                employee.IsActive = userInput.IsActive;
                employee.Position = userInput.Position;
                employee.PositionRole = userInput.PositionRole;
                employee.HireDate = userInput.HireDate;
                employee.ResignationDate = userInput.ResignationDate;

                detail.Department = userInput.Department;
                detail.ImmediateSupervisor = userInput.ImmediateSupervisor;
                detail.EmployeePhoto = Convert.FromBase64String(userInput.EmployeePhoto);
                detail.EmployeeName = userInput.EmployeeName;
                detail.PhoneNumber = userInput.PhoneNumber;
                detail.Mail = userInput.Mail;
                detail.Password = userInput.Password;
                detail.Address = userInput.Address;
                detail.Gender = userInput.Gender;
                detail.BirthDate = userInput.BirthDate;
                detail.EmergencyContact = userInput.EmergencyContact;
                detail.EmergencyRelationship = userInput.EmergencyRelationship;
                detail.EmergencyContactPhone = userInput.EmergencyContactPhone;
                detail.AnnualLeave = (byte)userInput.AnnualLeave;

                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return "資料更新完畢！";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Logger.LogError(ex, "Failed to update employee account {EmployeeId}", userInput.EmployeeId);
                throw;
            }
        }

        private static IQueryable<Employee> ApplyAccountFilters(IQueryable<Employee> query, ProfileOptions options)
        {
            if (!string.IsNullOrEmpty(options.IsActive) && TryParseBooleanFilter(options.IsActive, out bool isActive))
            {
                query = query.Where(employee => employee.IsActive == isActive);
            }

            if (!string.IsNullOrEmpty(options.Department))
            {
                query = query.Where(employee => employee.EmployeeDetail != null && employee.EmployeeDetail.Department == options.Department);
            }

            if (!string.IsNullOrEmpty(options.EmployeeId))
            {
                query = query.Where(employee => employee.EmployeeId == options.EmployeeId);
            }

            if (!string.IsNullOrEmpty(options.EmployeeName))
            {
                query = query.Where(employee => employee.EmployeeDetail != null && employee.EmployeeDetail.EmployeeName.Contains(options.EmployeeName));
            }

            return query;
        }

        private static int GetPage(string? page)
        {
            return int.TryParse(page, out int parsedPage) && parsedPage > 0 ? parsedPage : 1;
        }

        private static bool TryParseBooleanFilter(string value, out bool result)
        {
            if (value == "1")
            {
                result = true;
                return true;
            }

            if (value == "0")
            {
                result = false;
                return true;
            }

            return bool.TryParse(value, out result);
        }

        private static int GetTotalPages(int total)
        {
            return (int)Math.Ceiling(total / (double)PageSize);
        }

        private static string GenerateNextPrefixedId(string? latestId, string prefix, int digits)
        {
            int current = 0;
            if (!string.IsNullOrEmpty(latestId) && latestId.StartsWith(prefix) && latestId.Length > prefix.Length)
            {
                int.TryParse(latestId[prefix.Length..], out current);
            }

            return prefix + (current + 1).ToString().PadLeft(digits, '0');
        }
    }
}
