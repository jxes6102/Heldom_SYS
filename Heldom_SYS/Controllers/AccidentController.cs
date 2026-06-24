using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Heldom_SYS.Interface;
using Heldom_SYS.CustomModel;
using System.Dynamic;

namespace Heldom_SYS.Controllers
{
    public class AccidentController : Controller
    {
        private readonly IAccidentService AccidentService;
        private readonly ILogger<AccidentController> Logger;

        public AccidentController(IAccidentService _AccidentService, ILogger<AccidentController> logger)
        {
            AccidentService = _AccidentService;
            Logger = logger;
        }


        [HttpPost]
        public async Task<IActionResult> GetReport([FromBody] AccidentReq data)
        {
            dynamic response = new ExpandoObject();
            response.data = "未動作";

            if (string.IsNullOrEmpty(data.Page)) {
                response.data = "必須指定頁數";
                string errorResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
                return Content(errorResponse, "application/json");
            }

            try
            {
                response.data = await AccidentService.GetReport(data);
                response.pageCount = await AccidentService.GetReportPage();
            }
            catch (Exception error)
            {
                Logger.LogError(error, "Failed to get accident report");
                response.data = "拿取失敗";
            }

            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");
        }


        [HttpPost]
        public async Task<IActionResult> GetTrack([FromBody] AccidentReq data)
        {
            dynamic response = new ExpandoObject();
            response.data = "未動作";

            if (string.IsNullOrEmpty(data.Page))
            {
                response.data = "必須指定頁數";
                string errorResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
                return Content(errorResponse, "application/json");
            }

            try
            {
                response.data = await AccidentService.GetTrack(data);
                response.pageCount = await AccidentService.GetTrackPage(data);
            }
            catch (Exception error)
            {
                Logger.LogError(error, "Failed to get accident tracking data");
                response.data = "拿取失敗";
            }


            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");
        }


        [HttpPost]
        public async Task<IActionResult> GetDetail([FromBody] AccidentDetailReq data)
        {
            dynamic response = new ExpandoObject();
            response.data = "未動作";

            if (string.IsNullOrEmpty(data.ID))
            {
                response.data = "未設定ID";
                string errorResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
                return Content(errorResponse, "application/json");
            }

            try
            {
                response.data = await AccidentService.GetDetail(data.ID);
                response.reportImg = await AccidentService.GetDetailFile(data.ID,false);
                response.trackImg = await AccidentService.GetDetailFile(data.ID,true);
            }
            catch (Exception error)
            {
                Logger.LogError(error, "Failed to get accident detail {AccidentId}", data.ID);
                response.data = "拿取失敗";
            }

            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> AddAccident([FromForm] string AccidentType, [FromForm] string AccidentTitle, [FromForm] string Description, [FromForm] string StartTime, [FromForm] string Id, [FromForm] List<string> Files)
        {
            dynamic response = new ExpandoObject();
            response.data = "未動作";

            int maxSize = 5 * 1024 * 1024;

            for (int i = 0; i < Files.Count; i++)
            {
                if (Files[i].Length > maxSize)
                {
                    response.data = "檔案大於5M";
                    string errorResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
                    return Content(errorResponse, "application/json");
                }
            }

            if (string.IsNullOrEmpty(AccidentType) || string.IsNullOrEmpty(AccidentTitle) || string.IsNullOrEmpty(Description) || string.IsNullOrEmpty(StartTime) || string.IsNullOrEmpty(Id))
            {
                response.data = "未設定 AccidentType | AccidentTitle | Description | StartTime | Id";
                string errorResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
                return Content(errorResponse, "application/json");
            }

            try
            {
                await AccidentService.AddAccident(AccidentType,AccidentTitle,Description,StartTime,Id,Files);
                response.data = "新增成功";
            }
            catch (Exception error)
            {
                Logger.LogError(error, "Failed to add accident");
                response.data = "新增失敗";
            }


            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> AddReply([FromForm] string Reply, [FromForm] string AccidentId, [FromForm] string Status, [FromForm] string EndTime, [FromForm] List<string> Files)
        {

            dynamic response = new ExpandoObject();
            response.data = "未動作";

            int maxSize = 5 * 1024 * 1024;

            for (int i = 0; i < Files.Count; i++) {
                if (Files[i].Length > maxSize) {
                    response.data = "檔案大於5M";
                    string errorResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
                    return Content(errorResponse, "application/json");
                }
            }

            if (string.IsNullOrEmpty(Reply) || string.IsNullOrEmpty(AccidentId) || string.IsNullOrEmpty(Status))
            {
                response.data = "未設定 Reply | AccidentId | Status";
                string errorResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
                return Content(errorResponse, "application/json");
            }

            try
            {
                await AccidentService.AddReply(Reply,AccidentId, Status, EndTime, Files);
                response.data = "修改成功";

            }
            catch (Exception error)
            {
                Logger.LogError(error, "Failed to add accident reply {AccidentId}", AccidentId);
                response.data = "修改失敗";

            }

            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");

        }


        [HttpPost]
        public async Task<IActionResult> DeleteDetail([FromBody] AccidentDetailReq data)
        {

            dynamic response = new ExpandoObject();
            response.data = "未動作";

            if (string.IsNullOrEmpty(data.ID))
            {
                response.data = "未設定ID";
                string errorResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
                return Content(errorResponse, "application/json");
            }

            try
            {
                response.data = await AccidentService.DeleteDetail(data.ID);
            }
            catch (Exception error)
            {
                Logger.LogError(error, "Failed to delete accident {AccidentId}", data.ID);
                response.data = "拿取失敗";
            }

            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");
        }
    }
}
