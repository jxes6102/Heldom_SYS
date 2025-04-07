using Dapper;
using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Heldom_SYS.CustomModel;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using NPOI.SS.Formula.Functions;
using MathNet.Numerics;
using static NPOI.POIFS.Crypt.CryptoFunctions;
using System.Collections.Generic;
using Org.BouncyCastle.Ocsp;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Heldom_SYS.Service
{
    public class AccidentService: IAccidentService
    {
        private readonly SqlConnection DataBase;
        private readonly IUserStoreService UserRoleStore;

        public AccidentService(SqlConnection connection, IUserStoreService _UserRoleStore)
        {
            DataBase = connection;
            UserRoleStore = _UserRoleStore;
        }

        public async Task<IEnumerable<Accident>>  GetReport(AccidentReq req)
        {
            string sql = @"SELECT * FROM Accident
                    where EmployeeID = @EmployeeID
                    ORDER BY AccidentID DESC
                    OFFSET( @Page - 1) * 10 ROWS
                    FETCH NEXT 10 ROWS ONLY";


            IEnumerable<Accident>? data = await DataBase.QueryAsync<Accident>(sql,
                new
                {
                    EmployeeID = UserRoleStore.UserID,
                    Page = req.Page
                });

            return data;
        }



        public class PageData
        {
            public required string Total { get; set; }
        }

        public async Task<int> GetReportPage()
        {
            string sql = @"SELECT count(*) as Total FROM Accident
                        where EmployeeID = @EmployeeID";

            IEnumerable<PageData>? data = await DataBase.QueryAsync<PageData>(sql,
                new
                {
                    EmployeeID = UserRoleStore.UserID,
                });

            var result = data.Select(x => x.Total).ToList().First();

            int count = (int)Math.Ceiling(int.Parse(result) / 10.0);

            return count;
        }

        public async Task<IEnumerable<AccidentRes>> GetTrack(AccidentReq req)
        {
            // 需加入日期比對和人名join判斷
            string sql = @"SELECT Accident.*,EmployeeDetail.EmployeeName FROM Accident
                    left join EmployeeDetail on Accident.EmployeeID = EmployeeDetail.EmployeeID
                    where 1 = 1
                    ";

            if (UserRoleStore.GetRole() != "A") {
                sql += @" and IncidentControllerID = @IncidentControllerID";
            }

            if (!req.Title.IsNullOrEmpty()) {
                sql += @" and AccidentTitle like @AccidentTitle";
            }
            
            if (!req.Type.IsNullOrEmpty() && !(req.Type == "全部"))
            {
                sql += @" and AccidentType = @AccidentType";
            }

            if (!req.Name.IsNullOrEmpty())
            {
                sql += @" and EmployeeName like @EmployeeName";
            }

            if (!req.Date.IsNullOrEmpty())
            {
                sql += @" and((StartTime < @Date AND EndTime >= @Date) OR StartTime < @Date)";
            }
            

            sql += @" ORDER BY AccidentID DESC
                    OFFSET( @Page - 1) * 10 ROWS
                    FETCH NEXT 10 ROWS ONLY";

            IEnumerable<AccidentRes>? data = await DataBase.QueryAsync<AccidentRes>(sql,
                new
                {
                    IncidentControllerID = UserRoleStore.UserID,
                    Page = req.Page,
                    AccidentTitle = $"%{req.Title}%",
                    AccidentType = req.Type,
                    EmployeeName = $"%{req.Name}%",
                    Date = req.Date,
                });

            return data;
        }

        public async Task<int> GetTrackPage(AccidentReq req)
        {
            string sql = @"SELECT count(*) as Total FROM Accident
                        left join EmployeeDetail on Accident.EmployeeID = EmployeeDetail.EmployeeID
                        where 1 = 1";

            if (UserRoleStore.GetRole() != "A")
            {
                sql += @" and IncidentControllerID = @IncidentControllerID";
            }

            if (!req.Title.IsNullOrEmpty())
            {
                sql += @" and AccidentTitle like @AccidentTitle";
            }

            if (!req.Type.IsNullOrEmpty() && !(req.Type == "全部"))
            {
                sql += @" and AccidentType like @AccidentType";
            }

            if (!req.Name.IsNullOrEmpty())
            {
                sql += @" and EmployeeName like @EmployeeName";
            }

            if (!req.Date.IsNullOrEmpty())
            {
                sql += @" and((StartTime < @Date AND EndTime >= @Date) OR StartTime < @Date)";
            }

            IEnumerable<PageData>? data = await DataBase.QueryAsync<PageData>(sql,
                new
                {
                    IncidentControllerID = UserRoleStore.UserID,
                    AccidentTitle = $"%{req.Title}%",
                    AccidentType = req.Type,
                    EmployeeName = $"%{req.Name}%",
                    Date = req.Date,
                });

            var result = data.Select(x => x.Total).ToList().First();

            int count = (int)Math.Ceiling(int.Parse(result) / 10.0);

            return count;
        }


        public async Task<Accident> GetDetail(string id)
        {
            string sql = @"SELECT * FROM Accident where AccidentID = @AccidentID";


            IEnumerable<Accident>? data = await DataBase.QueryAsync<Accident>(sql,
                new
                {
                    AccidentID = id
                });


            return data.First();
        }


        public async Task<IEnumerable<AccidentFile>> GetDetailFile(string id, bool type)
        {
            string sql = @"SELECT * FROM AccidentFile where AccidentID = @AccidentID and ResponseType = @ResponseType";

            IEnumerable<AccidentFile> data = await DataBase.QueryAsync<AccidentFile>(sql, new
            {
                ResponseType = type,
                AccidentID = id
            });

            return data;
        }

        public class MaxData
        {
            public required string AccidentID { get; set; }
        }

        public class IncidentData
        {
            public required string ImmediateSupervisor { get; set; }
        }

        public async Task AddAccident(string AccidentType, string AccidentTitle, string Description, string StartTime, string Id, List<string> Files) {
            
            // 尋找並生成最新ID
            string checkIDSql = @"SELECT TOP 1 AccidentID FROM Accident ORDER BY AccidentID DESC";

            IEnumerable<MaxData>? dataID = await DataBase.QueryAsync<MaxData>(checkIDSql);

            string resultID = dataID.Select(x => x.AccidentID).ToList().First().ToString();
            int count = int.Parse(resultID.Substring(2)) + 1;
            string AccidentID = (Id == "000" ?  ("AC" + count.ToString().PadLeft(3, '0')) : Id);

            // 尋找上司
            string checkIncidentSql = @"SELECT ImmediateSupervisor FROM EmployeeDetail where EmployeeID = @EmployeeID";
            IEnumerable<IncidentData>? dataIncident = await DataBase.QueryAsync<IncidentData>(checkIncidentSql, new
            {
                EmployeeID = UserRoleStore.UserID
            });

            string resultIncident = "";
            var findItem = dataIncident.Select(x => x.ImmediateSupervisor).ToList().First();
            if (UserRoleStore.GetRole() != "A" && !findItem.IsNullOrEmpty())
            {
                resultIncident = findItem.ToString();
            }

            string addSql = @"INSERT INTO Accident 
            ([AccidentID], [AccidentType], [AccidentTitle], [Description], [StartTime], [EmployeeID], [UploadTime], [IncidentControllerID], [Response], [IncidentStatus]) VALUES
            (@AccidentID, @AccidentType, @AccidentTitle, @Description, @StartTime, @EmployeeID, @StartTime, @IncidentControllerID, null, 0)";

            int? dataAdd = await DataBase.QuerySingleOrDefaultAsync<int>(addSql, new
            {
                AccidentID = AccidentID,
                AccidentType = AccidentType,
                AccidentTitle = AccidentTitle,
                Description = Description,
                StartTime = StartTime,
                EmployeeID = UserRoleStore.UserID,
                UploadTime = StartTime,
                IncidentControllerID = resultIncident,
            });


            string delSql = @"DELETE FROM AccidentFile WHERE AccidentID = @AccidentID and ResponseType = @ResponseType";

            await DataBase.QuerySingleOrDefaultAsync<int>(delSql, new
            {
                AccidentID = AccidentID,
                ResponseType = false
            });

            // 尋找並生成最新ID
            string checkFileIDSql = @"SELECT TOP 1 FileID FROM AccidentFile ORDER BY FileID DESC";

            IEnumerable<MaxFileID>? dataFileID = await DataBase.QueryAsync<MaxFileID>(checkFileIDSql);

            string resultFileID = dataFileID.Select(x => x.FileID).ToList().First().ToString();


            if (Files == null || Files.Count == 0)
            {
                return;
            }


            for (int i = 0; i < Files.Count; i++)
            {

                string base64Data = Files[i].Contains(",") ? Files[i].Split(',')[1] : Files[i];

                byte[] fileBytes = Convert.FromBase64String(base64Data);

                int resultFileIDToInt = int.Parse(resultFileID.Substring(1)) + i + 1;
                string FileID = "F" + resultFileIDToInt.ToString().PadLeft(4, '0');

                string addFileSql = @"INSERT INTO AccidentFile(FileID,AccidentID,FileImage,ResponseType)
                VALUES (@FileID,@AccidentID,@FileImage,@ResponseType)";

                await DataBase.QuerySingleOrDefaultAsync<int>(addFileSql, new
                {
                    FileID = FileID,
                    AccidentID = AccidentID,
                    FileImage = fileBytes,
                    ResponseType = false,
                });

            }

        }


        public class MaxFileID
        {
            public required string FileID { get; set; }
        }
        public async Task AddReply(string Reply, string AccidentId,string Status, string EndTime, List<string> Files)
        {
            string sql = @"UPDATE Accident SET Response = @Response ,IncidentStatus = @IncidentStatus, EndTime = @EndTime WHERE AccidentID = @AccidentID";

            await DataBase.QuerySingleOrDefaultAsync<int>(sql, new
            {
                Response = Reply,
                AccidentID = AccidentId,
                IncidentStatus = (Status == "1") ? true : false,
                EndTime = EndTime ?? (object)DBNull.Value,
            });

            string delSql = @"DELETE FROM AccidentFile WHERE AccidentID = @AccidentID and ResponseType = @ResponseType";

            await DataBase.QuerySingleOrDefaultAsync<int>(delSql, new
            {
                AccidentID = AccidentId,
                ResponseType = true
            });

            // 尋找並生成最新ID
            string checkIDSql = @"SELECT TOP 1 FileID FROM AccidentFile ORDER BY FileID DESC";

            IEnumerable<MaxFileID>? dataID = await DataBase.QueryAsync<MaxFileID>(checkIDSql);

            string resultID = dataID.Select(x => x.FileID).ToList().First().ToString();


            if (Files == null || Files.Count == 0)
            {
                return ;
            }

            for (int i = 0; i < Files.Count; i++)
            {

                // 移除 Base64 開頭的 `data:image/png;base64,` 這類字串
                string base64Data = Files[i].Contains(",") ? Files[i].Split(',')[1] : Files[i];
                base64Data = base64Data.Trim().Replace("\n", "").Replace("\r", "");
                // 轉換為 byte[]
                byte[] fileBytes = Convert.FromBase64String(base64Data);


                int count = int.Parse(resultID.Substring(1)) + i + 1;
                string FileID = "F" + count.ToString().PadLeft(4, '0');

                string addSql = @"INSERT INTO AccidentFile(FileID,AccidentID,FileImage,ResponseType)
                VALUES (@FileID,@AccidentID,@FileImage,@ResponseType)";

                await DataBase.QuerySingleOrDefaultAsync<int>(addSql, new
                {
                    FileID = FileID,
                    AccidentID = AccidentId,
                    FileImage = fileBytes,
                    ResponseType = (true),
                });

            }

        }


        public async Task<int> DeleteDetail(string id)
        {

            string sql = @"DELETE FROM Accident WHERE AccidentID = @AccidentID";
            int rowsAffected = await DataBase.ExecuteAsync(sql, new { AccidentID = id });

            string fileSql = @"DELETE FROM AccidentFile WHERE AccidentID = @AccidentID";
            int rowsFileAffected = await DataBase.ExecuteAsync(fileSql, new { AccidentID = id });

            return rowsAffected + rowsFileAffected;
        }


    }
}
