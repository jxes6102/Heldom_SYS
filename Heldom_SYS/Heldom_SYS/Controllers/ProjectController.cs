using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dapper;
using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;




namespace Heldom_SYS.Controllers
{

    public class ProjectController : Controller
    {
        //BP
        private readonly ConstructionDbContext db ;
        private readonly SqlConnection DataBase;
        public class UsePrintCategary
        {
            public required string  categaryID { get; set; }
            public required string categaryName { get; set; }
        }

        public class ChangeStatusData
        {
            [JsonPropertyName("id")]
            public required string id { get; set; }
            [JsonPropertyName("BPName")]
            public required string BPName { get; set; }
            [JsonPropertyName("changedVersion")]
            public required string changedVersion { get; set; }
            [JsonPropertyName("changingVersion")]
            public required string changingVersion { get; set; }
        }
        public class DisplayEditData
        {
            [JsonPropertyName("id")]
            public required string id { get; set; }
            [JsonPropertyName("BPName")]
            public required string BPName { get; set; }
        }

        public class InsertNewData
        {
            [JsonPropertyName("id")]
            public required string id { get; set; }
            [JsonPropertyName("BPName")]
            public required string BPName { get; set; }
            [JsonPropertyName("vers")]
            public required string vers { get; set; }
            [JsonPropertyName("print")]
            public required string print { get; set; }
        }

        //紀錄當前人員UserRoleStore
        private readonly IUserStoreService UserRoleStore;
        public ProjectController(SqlConnection connection,ConstructionDbContext _db,IUserStoreService _UserRoleStore)
        {
            DataBase = connection;
            db = _db;
            UserRoleStore = _UserRoleStore;
        }

        public IActionResult Issues(string type, string id, string motion)
        {
            //UserRoleStore.SetRole("A");
            ViewBag.Role = UserRoleStore.GetRole();

            //if ((ViewBag.Role == "X") || (ViewBag.Role == "P"))
            //{
            //    return RedirectToAction("Index", "Login");
            //}


            if (type.IsNullOrEmpty() || id.IsNullOrEmpty() || motion.IsNullOrEmpty()) {
                return View("Issues");
            }
            else
            {
                ViewBag.Type = type;
                ViewBag.ID = id;
                ViewBag.Motion = motion;
                return View("IssuesDetail");
            }

        }
        //[HttpGet] /Project/Myfunction
        //public async Task<IActionResult> Myfunction(data)
        //{
        //    new { 
        //    name = 123,
        //    id = 456
        //    }
        //    var bee = Convert.deserialize(data)
        //        bee.name
        //    var apple = await DataBase.QueryFirstAsync();
        //    return Ok(apple);
        //}


        //  -------------------------------------BlueprintsCategories-------------------------------------

        public IActionResult BlueprintsCategories()
        {
            //連線資料庫取categary
            string sql = @"SELECT * FROM PrintCategory";
            IEnumerable<dynamic>? PrintCategories =  DataBase.Query<dynamic>(sql);
            PrintCategories = PrintCategories.Select(p => new {
                p.PrintCategoryID,
                p.PrintCategory
            }).ToList();
            foreach (var item in PrintCategories)
            {
                //Console.WriteLine((string)item.PrintCategoryID);
                //Console.WriteLine((string)item.PrintCategory);
                string pcategary = (string)item.PrintCategory;
                string pcategaryID = item.PrintCategoryID.ToString();
                //Console.WriteLine(item.PrintCategoryID.GetType());
                HttpContext.Session.SetString(pcategaryID, JsonConvert.SerializeObject(pcategary));
            }

            //userName
            TempData["PrintOner"] = UserRoleStore.UserName;

            return View();
        }



        //  -------------------------------------Blueprints-------------------------------------



        public IActionResult Blueprints(string id)
        {
            Console.WriteLine($"傳入 id: {id}");
            Console.WriteLine($"Session 內的 Keys: {string.Join(", ", HttpContext.Session.Keys)}");

            TempData["toEdit"] = "BP";

            //  從 Session 取出並解析 JSON
            // 確保 id 不是 null 或空字串
            if (!string.IsNullOrEmpty(id))
            {
                // 嘗試從 Session 取出值
                var jsonString = HttpContext.Session.GetString(id);

                if (!string.IsNullOrEmpty(jsonString))
                {
                    // 解析 JSON
                    string? categories = JsonConvert.DeserializeObject<string>(jsonString);
                    TempData["categary"] = categories;
                    TempData["ID"] = id;
                }
                else
                {
                    // Session 內沒有該 id 的資料
                    Console.WriteLine($"Session 沒有找到 key: {id}");
                }
            }
            else
            {
                Console.WriteLine("id 不能為 null 或空字串");
            }
            //使用者當前藍圖種類usedCategary
            HttpContext.Session.SetString("usedCategary", JsonConvert.SerializeObject(id));

            //顯示藍圖實體
            string sql= @"SELECT  BlueprintName,EmployeeID, BlueprintVersion, 
            BlueprintFile, UploadTime FROM Blueprint 
            where [PrintCategoryID] = @id and PrintStatus = 1";

            IEnumerable<Blueprint> blueprints = DataBase.Query<Blueprint>(sql, new { id });
            var prints = blueprints.Select(p => new
            {
                Name = p.BlueprintName,
                Manager = p.EmployeeId,
                Version = p.BlueprintVersion,
                UploadDate = p.UploadTime,
                Image = p.BlueprintFile
            }).ToList();

            return View(prints);
        }

        //顯示status=1藍圖
        [HttpGet("Project/GetStatusTruePrint")]
        public async Task<IActionResult> GetStatusTruePrint(string id, string BPName)
        {
            try
            {
                string sql = @"SELECT Bp.BlueprintName,Bp.EmployeeID, Bp.BlueprintVersion, 
            Bp.BlueprintFile, Bp.UploadTime, ED.EmployeeName
			FROM Blueprint  BP
			join Employee E on E.EmployeeID = Bp.EmployeeID
			join EmployeeDetail ED on E.EmployeeID = ED.EmployeeID
            where [PrintCategoryID] =@id and BlueprintName = @BPName and PrintStatus = 1";

                IEnumerable<dynamic> StatusTruePrint =await DataBase.QueryAsync<dynamic>(sql, new { id, BPName });
                var prints = StatusTruePrint.Select(p => new
                {
                    BpName = p.BlueprintName,
                    UDID = p.EmployeeID,
                    Version = p.BlueprintVersion,
                    UploadDate = p.UploadTime.ToString("yyyy-MM-dd"),
                    Image = Convert.ToBase64String(p.BlueprintFile),
                    UDName = p.EmployeeName
                }).ToList();
                return Ok(prints);
            }
            catch (Exception ex)
            {
                // 捕獲並返回詳細錯誤訊息
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
        //更動啟用藍圖
        [HttpPost("/Project/ChangeIsActive")]
        public async Task<IActionResult> ChangeIsActive([FromBody] ChangeStatusData data)
        {
            if (data == null)
            {
                return BadRequest(new { message = "請求資料為空，請檢查 JSON 格式" });
            }
            try
            {
                string sql = @"update Blueprint set PrintStatus = 0
                    where [PrintCategoryID] =@id and BlueprintName = @BPName and BlueprintVersion = @changedVersion
                                    update Blueprint set PrintStatus = 1 
                    where [PrintCategoryID] =@id and BlueprintName =@BPName and BlueprintVersion = @changingVersion";

                    await DataBase.QueryFirstOrDefaultAsync<ChangeStatusData>(sql, new { 
                    id=data.id,
                    BPName=data.BPName, 
                    changedVersion = data.changedVersion, 
                    changingVersion = data.changingVersion });
                return Ok();
            }
            catch (Exception ex)
            {
                // 捕獲並返回詳細錯誤訊息
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }


        //  -------------------------------------BlueprintsDetail-------------------------------------


        public IActionResult BlueprintsDetail(string id, string BPName)
        {
            TempData["toEdit"] = "BPD";
            //Console.WriteLine("BPDcontroller");
            //Console.WriteLine(id);
            //Console.WriteLine($"{BPName}");
            //Console.WriteLine($"BPDcontroller Time {DateTime.Now}");
            //顯示藍圖實體0
            string sql = @"SELECT  BlueprintName,EmployeeID, BlueprintVersion, 
            BlueprintFile, UploadTime FROM Blueprint 
            where [PrintCategoryID] = @id and BlueprintName = @BPName and PrintStatus = 0
			ORDER BY BlueprintVersion DESC;";

            IEnumerable<Blueprint> sameNameprints = DataBase.Query<Blueprint>(sql, new { id, BPName });
            var prints = sameNameprints.Select(p => new
            {
                Name = p.BlueprintName,
                Manager = p.EmployeeId,
                Version = p.BlueprintVersion,
                UploadDate = p.UploadTime,
                Image = p.BlueprintFile
            }).ToList();
            return View(prints);
        }

        [HttpGet("Project/GetUsingCategary")]
        public string GetUsingCategary()
        {
            var jsonString = HttpContext.Session.GetString("usedCategary");
            string? categories="";

            if (!string.IsNullOrEmpty(jsonString))
            {
                 categories = JsonConvert.DeserializeObject<string>(jsonString);
                HttpContext.Session.SetString("usedCategary", jsonString);
            }


            return categories??"";
            //?? =>前面存在的話 = 後面
        }


        //  -------------------------------------BlueprintsEdit-------------------------------------

        [HttpPost("Project/DisplayPrintInfo")]
        public async Task<IActionResult> DisplayPrintInfo([FromBody] DisplayEditData data)
        {
            if (data == null)
            {
                return BadRequest(new { message = "請求資料為空，請檢查 JSON 格式" });
            }
            try
            {
               string sql = @"SELECT TOP 1 *  FROM Blueprint 
                where [PrintCategoryID] = @id and BlueprintName = @BPName 
			    ORDER BY BlueprintVersion DESC;";

                IEnumerable<Blueprint> Editprints = await DataBase.QueryAsync<Blueprint>(sql, new {
                    data.id, 
                    data.BPName 
                });
                var Editprint = Editprints.Select(ep => new
                {
                    Name = ep.BlueprintName,
                    Id = ep.PrintCategoryId,
                    Vers = ep.BlueprintVersion
                }).ToList();

                return Ok(Editprint);
            }
            catch (Exception ex)
            {
                // 捕獲並返回詳細錯誤訊息
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        //Insert new one
        [HttpPost("Project/InsertNew")]
        public async Task<IActionResult> InsertNew([FromBody] InsertNewData data)
        {
            if (data == null)
            {
                return BadRequest(new { message = "請求資料為空" });
            }
            try
            {
                //vers
                Console.WriteLine("vers type: " + (data.vers));

                if (decimal.TryParse(data.vers, out decimal versionDecimal))
                {
                    Console.WriteLine("成功轉換 data.vers 為 decimal");
                }
                else
                {
                    throw new Exception("版本號格式錯誤，請輸入有效的數值");
                }

                // --------------查BlueprintID--------------
                var latestBlueprint = await db.Blueprints
                    .OrderByDescending(b => b.BlueprintId) // 假設有 Version 欄位
                    .FirstOrDefaultAsync();
                if (latestBlueprint != null)
                {
                    Console.WriteLine( latestBlueprint.BlueprintId);
                }
                else
                {
                    Console.WriteLine("找不到對應的藍圖");
                }
                //Console.WriteLine("最新: "+latestBlueprint.BlueprintId);
                string BlueprintID="預設ID";
                if (latestBlueprint.BlueprintId.Length < 6)
                {
                    string tempId = latestBlueprint.BlueprintId.Substring(1); // B   0050
                    //Console.WriteLine("BPid: "+tempId);

                    // 將數字部分轉換為整數並加 1
                    int numericId = Convert.ToInt32(tempId) + 1;
                    // 將數字部分轉回字串，並補足 0 至 4 位數
                    string formattedId = numericId.ToString().PadLeft(4, '0');
                    // 組合成新的 BlueprintID
                     BlueprintID = "B" + formattedId;
                    //Console.WriteLine("BPid after transformation: " + BlueprintID);

                }
                // --------------判斷version是否重複--------------
                var checkVersions = await db.Blueprints
                .Where(b => b.PrintCategoryId == data.id && b.BlueprintName == data.BPName) // 篩選條件
                .OrderByDescending(b => b.BlueprintVersion) // 版本排序 大=>小
                .ToListAsync(); // 將結果轉為 List

                foreach (var bp in checkVersions)
                {
                    if(bp.BlueprintVersion== versionDecimal)
                    {
                        //return Ok("version"+ versionDecimal + " 重複");
                        //return Ok("version"+ data.id + " 重複");
                        return Ok("1");

                    }
                }

                // --------------新增--------------

                string userID = UserRoleStore.UserID;

                // 檢查print是否包含前綴，並移除
                string base64Data = data.print.Contains(",") ? data.print.Split(',')[1] : data.print;
                // 去除 Base64 內的換行與空格
                base64Data = base64Data.Trim().Replace("\n", "").Replace("\r", "");
                // 轉換為 byte[]
                byte[] fileBytes = Convert.FromBase64String(base64Data);


                Blueprint newPrint;
                // 判斷是否是版本1 與 創建新的 物件
                if (versionDecimal == 1)
                {
                     newPrint = new Blueprint
                    {
                        BlueprintId = BlueprintID,
                        EmployeeId = userID,  // 設定 Employee ID
                        PrintCategoryId = data.id,
                        BlueprintName = data.BPName,
                        BlueprintVersion = versionDecimal,
                        BlueprintFile = fileBytes,
                        UploadTime = DateTime.Now,
                        PrintStatus = true
                    };
                }
                else
                {
                     newPrint = new Blueprint
                    {
                        BlueprintId = BlueprintID,
                        EmployeeId = userID,  // 設定 Employee ID
                        PrintCategoryId = data.id,
                        BlueprintName = data.BPName,
                        BlueprintVersion = versionDecimal,
                        BlueprintFile = fileBytes,
                        UploadTime = DateTime.Now,
                        PrintStatus = false
                    };
                }


                // 加入 DbContext
                await db.Blueprints.AddAsync(newPrint);
                await db.SaveChangesAsync();

                return Ok();

            }
            catch (Exception ex)
            {
                // 捕獲並返回詳細錯誤訊息
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
        public IActionResult BlueprintsEdit()
        {
            return View();
        }



    }
}
