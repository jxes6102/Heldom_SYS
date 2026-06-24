using Heldom_SYS.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Heldom_SYS.Filter
{
    public class UserFilter: System.Attribute, IAuthorizationFilter
    {
        private readonly IUserStoreService UserRoleStore;

        public UserFilter(SqlConnection connection, IUserStoreService _UserRoleStore)
        {
            UserRoleStore = _UserRoleStore;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //var headers = context.HttpContext.Request.Headers;
            //var Routers = context.RouteData.Routers;
            //var Values = context.RouteData.Values;

            UserRoleStore.UserID = context.HttpContext.Session.GetString("UserID") ?? "";
            UserRoleStore.UserName = context.HttpContext.Session.GetString("UserName") ?? "";
            UserRoleStore.SetRole(context.HttpContext.Session.GetString("Role") ?? "X");

            string controller = context.RouteData.Values["controller"]?.ToString()?.ToLower() ?? "";
            string action = context.RouteData.Values["action"]?.ToString()?.ToLower() ?? "";
            string realMenu = JsonConvert.SerializeObject(UserRoleStore.MenuStr, Formatting.Indented).ToLower();
            string allMenu = JsonConvert.SerializeObject(UserRoleStore.MenuData, Formatting.Indented).ToLower();
            string routeStr = controller + "/" + action;


            if(allMenu.Contains(routeStr) && !realMenu.Contains(routeStr))
            {
                context.Result = new RedirectResult("/Login");
            }


        }

    }
}
