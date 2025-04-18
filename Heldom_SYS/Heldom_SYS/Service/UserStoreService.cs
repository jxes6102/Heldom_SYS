using Heldom_SYS.Interface;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NPOI.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Heldom_SYS.Service
{
    public class MenuItem
    {
        public string? Title { get; set; }
        public string? IconNmae { get; set; }
        public List<KidItem>? KidList { get; set; }
    }

    public class KidItem
    {
        public string? Name { get; set; }
        public string? Link { get; set; }
        public List<string>? Access { get; set; }
    }

    public class UserStoreService : IUserStoreService
    {
        //A（管理層）：所有權限。
        //M（工地主任）：管理權限。
        //E（一般員工）：打卡、請假、異常回報、瀏覽個人資料。
        //P（臨時工）：僅打卡。
        //X (未登入情況)。

        public UserStoreService()
        {
            //預設未登入
            MenuData = new List<MenuItem>() {
            new MenuItem {
                Title = "施工監控平台",
                IconNmae = "fa-columns",
                KidList = new List<KidItem> {
                    new KidItem {
                        Name = "施工監控儀表板",
                        Link = "/dashboard/Dashboard",
                        Access = new List<string>() { "A","M" },
                    },
                    new KidItem {
                        Name = "即時監控影像",
                        Link = "/dashboard/Cctv",
                        Access = new List<string>() { "A","M" },
                    },
                }
            },
            new MenuItem {
                Title = "考勤管理",
                IconNmae = "fa-address-card",
                KidList = new List<KidItem> {
                    new KidItem {
                        Name = "打卡與請假",
                        Link = "/attendance/Records",
                        Access = new List<string>() { "E","P","M" },
                    },
                    new KidItem {
                        Name = "員工出勤紀錄",
                        Link = "/attendance/Info",
                        Access = new List<string>() { "A","M" },
                    },
                }
            },
            new MenuItem {
                Title = "現場施工管理",
                IconNmae = "fa-hammer",
                KidList = new List<KidItem>  {
                    new KidItem {
                        Name = "異常回報與追蹤",
                        Link = "/Project/Issues",
                        Access = new List<string>() { "A","M","E" },
                    },
                    new KidItem {
                        Name = "施工圖紙管理",
                        Link = "/Project/BlueprintsCategories",
                        Access = new List<string>() { "A","M" },
                    },
                }
            },
            new MenuItem {
                Title = "個人資料",
                IconNmae = "fa-user",
                KidList = new List<KidItem> {
                    new KidItem {
                        Name = "個人中心",
                        Link = "/profile/index",
                        Access = new List<string>() { "A","M","E" },
                    },
                    new KidItem {
                        Name = "個人資料設定",
                        Link = "/profile/settings",
                        Access = new List<string>() { "A","M","E" },
                    },
                    new KidItem {
                        Name = "員工帳號管理",
                        Link = "/profile/account",
                        Access = new List<string>() { "A","M" },
                    },
                    new KidItem {
                        Name = "新增員工帳號",
                        Link = "/profile/NewAccount",
                        Access = new List<string>() { "A" },
                    },
                }
            },
            new MenuItem {
                Title = "系統管理",
                IconNmae = "fa-cog",
                KidList = new List<KidItem> {
                    new KidItem {
                        Name = "系統設定",
                        Link = "/settings",
                        Access = new List<string>() {},
                    },
                }
            },
        };
            this.Role = "X";
            this.MenuStr = string.Empty;
            this.UserID = string.Empty;
            this.UserName = string.Empty;
            this.CreateMenu();
        }

        public string Role { set; get; }
        public string MenuStr { set; get; }
        public string UserID { set; get; }
        public string UserName { set; get; }

        public List<MenuItem> MenuData { set; get; }

        //SetRole 更新角色權限
        public void SetRole() {
            //在登入時從資料庫尋找該使用者的身份
            this.Role = "X";
            this.CreateMenu();
        }

        public void SetRole(string _role) {
            //可在controller 自己設定角色的權限 可拿來測試用
            string[] RoleGroup = { "A","M","E","P","X" };

            if (RoleGroup.Contains(_role)) {
                this.Role = _role;
            }
            else
            {
                //設定失敗轉未登入
                this.Role = "X";
            }

            this.CreateMenu();
        }

        public string GetRole() {
            return this.Role;
        }

        public void ClearRole() {
            this.Role = "X";
            this.CreateMenu();
        }

        public void CreateMenu()
        {

            //深拷貝
            string MenuStr = JsonConvert.SerializeObject(MenuData, Formatting.Indented);
            List<MenuItem>? Menu = JsonConvert.DeserializeObject<List<MenuItem>>(MenuStr);

            if (!(Menu == null))
            {
                
                for (int i = 0; i < Menu.Count; i++)
                {
                    if(Menu[i].KidList != null)
                    {
                        Menu[i].KidList = Menu[i].KidList?.Where((item) =>
                        {
                            if (item.Access?.Count == 0 || item.Access == null)
                            {
                                return false;
                            }

                            return item.Access.Contains(this.Role);
                        }).ToList();

                    }

                }

                Menu = Menu.Where((item) => {
                    return (item?.KidList?.Count == 0) ? false : true;
                }).ToList();

            }

            string completeMenu = JsonConvert.SerializeObject(Menu, Formatting.Indented);
            //Console.WriteLine("===================================================");
            //Console.WriteLine(this.Role);
            //Console.WriteLine(completeMenu);
            this.MenuStr = JsonConvert.SerializeObject(Menu, Formatting.Indented);

        }

        public void CreateALLMenu() {
            this.MenuStr = JsonConvert.SerializeObject(MenuData, Formatting.Indented);
        }

    }
}
