using Azure;
using Dapper;
using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Heldom_SYS.Controllers
{
    public class ToolController : Controller
    {
        private readonly IUserStoreService UserRoleStore;
        private readonly SqlConnection DataBase;
        public ToolController(SqlConnection connection, IUserStoreService _UserRoleStore)
        {
            DataBase = connection;
            UserRoleStore = _UserRoleStore;
        }

        [HttpGet]
        public string GetMenu()
        {
            //預設角色為 A M E P X
            //UserRoleStore.SetRole("A");
            //UserRoleStore.SetRole("M");
            //UserRoleStore.SetRole("E");
            //UserRoleStore.SetRole("P");
            //UserRoleStore.SetRole("X");

            //測試拿全部
            //UserRoleStore.CreateALLMenu();
            //UserRoleStore.CreateMenu();

            return UserRoleStore.MenuStr;
        }


        [HttpGet]
        public async Task<IActionResult> GetCompany()
        {
            string sql = @"SELECT * FROM Company";
            IEnumerable<Company> ? company = await DataBase.QueryAsync<Company>(sql);

            string jsonResponse = JsonConvert.SerializeObject(company, Formatting.Indented);
            return Content(jsonResponse, "application/json");
        }

    }
}
