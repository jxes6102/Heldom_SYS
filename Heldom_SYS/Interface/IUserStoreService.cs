using Heldom_SYS.Service;

namespace Heldom_SYS.Interface
{
    public interface IUserStoreService
    {
        string Role { set; get; }
        string MenuStr { set; get; }
        string UserID { set; get; }
        string UserName { set; get; }
        List<MenuItem> MenuData { set; get; }

        void SetRole();

        void SetRole(string name);

        string GetRole();

        void ClearRole();

        void CreateMenu();

        void CreateALLMenu();
    }
}
