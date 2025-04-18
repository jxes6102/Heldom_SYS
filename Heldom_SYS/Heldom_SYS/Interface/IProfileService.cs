using Heldom_SYS.Controllers;
using Heldom_SYS.CustomModel;
using Heldom_SYS.Models;
using Microsoft.AspNetCore.Mvc;
using static Heldom_SYS.Controllers.ProfileController;

namespace Heldom_SYS.Interface
{
    public interface IProfileService
    {
        Task<IEnumerable<ProfileIndex>> GetIndexData();
        Task<IEnumerable<ProfileSettings>> GetSettingsData();
        Task<bool> UpdateSettingsData(EmployeeDetailUpdateModel userInput);
        Task<IEnumerable<ProfileAccount>> GetAccountsData(ProfileOptions options);
        Task<int> GetTotalPage(ProfileOptions options);
        Task<string> GetNewId();
        Task<IEnumerable<ProfileNewAccountData>> GetSupervisor();
        Task<string> CreateAccount(GetNewAccountEditData userInput);
        //Task<bool> CreateAccount2(ProfileCreate userInput);
        Task<IEnumerable<GetNewAccountEditData>> GetAccountData(string employeeId);
        Task<string> UpdateAccount(GetNewAccountEditData userInput);
    }
}
