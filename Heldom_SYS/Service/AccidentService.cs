using Dapper;
using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Heldom_SYS.CustomModel;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Heldom_SYS.Service
{
    public class AccidentService: IAccidentService
    {
        private const int PageSize = 10;

        private readonly SqlConnection DataBase;
        private readonly IUserStoreService UserRoleStore;
        private readonly ILogger<AccidentService> Logger;

        public AccidentService(SqlConnection connection, IUserStoreService _UserRoleStore, ILogger<AccidentService> logger)
        {
            DataBase = connection;
            UserRoleStore = _UserRoleStore;
            Logger = logger;
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

            int total = await DataBase.QuerySingleAsync<int>(sql,
                new
                {
                    EmployeeID = UserRoleStore.UserID,
                });

            return GetTotalPages(total);
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

            if (!string.IsNullOrEmpty(req.Title)) {
                sql += @" and AccidentTitle like @AccidentTitle";
            }
            
            if (!string.IsNullOrEmpty(req.Type) && req.Type != "全部")
            {
                sql += @" and AccidentType = @AccidentType";
            }

            if (!string.IsNullOrEmpty(req.Name))
            {
                sql += @" and EmployeeName like @EmployeeName";
            }

            if (!string.IsNullOrEmpty(req.Date))
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

            if (!string.IsNullOrEmpty(req.Title))
            {
                sql += @" and AccidentTitle like @AccidentTitle";
            }

            if (!string.IsNullOrEmpty(req.Type) && req.Type != "全部")
            {
                sql += @" and AccidentType like @AccidentType";
            }

            if (!string.IsNullOrEmpty(req.Name))
            {
                sql += @" and EmployeeName like @EmployeeName";
            }

            if (!string.IsNullOrEmpty(req.Date))
            {
                sql += @" and((StartTime < @Date AND EndTime >= @Date) OR StartTime < @Date)";
            }

            int total = await DataBase.QuerySingleAsync<int>(sql,
                new
                {
                    IncidentControllerID = UserRoleStore.UserID,
                    AccidentTitle = $"%{req.Title}%",
                    AccidentType = req.Type,
                    EmployeeName = $"%{req.Name}%",
                    Date = req.Date,
                });

            return GetTotalPages(total);
        }


        public async Task<Accident> GetDetail(string id)
        {
            string sql = @"SELECT * FROM Accident where AccidentID = @AccidentID";


            Accident data = await DataBase.QueryFirstAsync<Accident>(sql,
                new
                {
                    AccidentID = id
                });

            return data;
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
            await EnsureConnectionOpenAsync();
            using var transaction = DataBase.BeginTransaction();

            try
            {
                // 尋找並生成最新ID
                string checkIDSql = @"SELECT TOP 1 AccidentID FROM Accident WITH (UPDLOCK, HOLDLOCK) ORDER BY AccidentID DESC";

                string? latestAccidentId = await DataBase.QueryFirstOrDefaultAsync<string>(checkIDSql, transaction: transaction);
                string AccidentID = Id == "000" ? GenerateNextPrefixedId(latestAccidentId, "AC", 3) : Id;

                // 尋找上司
                string checkIncidentSql = @"SELECT ImmediateSupervisor FROM EmployeeDetail where EmployeeID = @EmployeeID";
                string? immediateSupervisor = await DataBase.QueryFirstOrDefaultAsync<string>(checkIncidentSql, new
                {
                    EmployeeID = UserRoleStore.UserID
                }, transaction);

                string resultIncident = "";
                if (UserRoleStore.GetRole() != "A" && !string.IsNullOrEmpty(immediateSupervisor))
                {
                    resultIncident = immediateSupervisor;
                }

                string addSql = @"INSERT INTO Accident 
            ([AccidentID], [AccidentType], [AccidentTitle], [Description], [StartTime], [EmployeeID], [UploadTime], [IncidentControllerID], [Response], [IncidentStatus]) VALUES
            (@AccidentID, @AccidentType, @AccidentTitle, @Description, @StartTime, @EmployeeID, @StartTime, @IncidentControllerID, null, 0)";

                await DataBase.ExecuteAsync(addSql, new
                {
                    AccidentID = AccidentID,
                    AccidentType = AccidentType,
                    AccidentTitle = AccidentTitle,
                    Description = Description,
                    StartTime = StartTime,
                    EmployeeID = UserRoleStore.UserID,
                    UploadTime = StartTime,
                    IncidentControllerID = resultIncident,
                }, transaction);


                string delSql = @"DELETE FROM AccidentFile WHERE AccidentID = @AccidentID and ResponseType = @ResponseType";

                await DataBase.ExecuteAsync(delSql, new
                {
                    AccidentID = AccidentID,
                    ResponseType = false
                }, transaction);

                // 尋找並生成最新ID
                string checkFileIDSql = @"SELECT TOP 1 FileID FROM AccidentFile WITH (UPDLOCK, HOLDLOCK) ORDER BY FileID DESC";

                string? resultFileID = await DataBase.QueryFirstOrDefaultAsync<string>(checkFileIDSql, transaction: transaction);


                if (Files != null && Files.Count > 0)
                {
                    for (int i = 0; i < Files.Count; i++)
                    {

                        string base64Data = Files[i].Contains(",") ? Files[i].Split(',')[1] : Files[i];

                        byte[] fileBytes = Convert.FromBase64String(base64Data);

                        string FileID = GenerateNextPrefixedId(resultFileID, "F", 4, i + 1);

                        string addFileSql = @"INSERT INTO AccidentFile(FileID,AccidentID,FileImage,ResponseType)
                VALUES (@FileID,@AccidentID,@FileImage,@ResponseType)";

                        await DataBase.ExecuteAsync(addFileSql, new
                        {
                            FileID = FileID,
                            AccidentID = AccidentID,
                            FileImage = fileBytes,
                            ResponseType = false,
                        }, transaction);

                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Logger.LogError(ex, "Failed to add accident for employee {EmployeeId}", UserRoleStore.UserID);
                throw;
            }

        }


        public class MaxFileID
        {
            public required string FileID { get; set; }
        }
        public async Task AddReply(string Reply, string AccidentId,string Status, string EndTime, List<string> Files)
        {
            await EnsureConnectionOpenAsync();
            using var transaction = DataBase.BeginTransaction();

            try
            {
                string sql = @"UPDATE Accident SET Response = @Response ,IncidentStatus = @IncidentStatus, EndTime = @EndTime WHERE AccidentID = @AccidentID";

                await DataBase.ExecuteAsync(sql, new
                {
                    Response = Reply,
                    AccidentID = AccidentId,
                    IncidentStatus = (Status == "1") ? true : false,
                    EndTime = EndTime ?? (object)DBNull.Value,
                }, transaction);

                string delSql = @"DELETE FROM AccidentFile WHERE AccidentID = @AccidentID and ResponseType = @ResponseType";

                await DataBase.ExecuteAsync(delSql, new
                {
                    AccidentID = AccidentId,
                    ResponseType = true
                }, transaction);

                // 尋找並生成最新ID
                string checkIDSql = @"SELECT TOP 1 FileID FROM AccidentFile WITH (UPDLOCK, HOLDLOCK) ORDER BY FileID DESC";

                string? resultID = await DataBase.QueryFirstOrDefaultAsync<string>(checkIDSql, transaction: transaction);


                if (Files != null && Files.Count > 0)
                {
                    for (int i = 0; i < Files.Count; i++)
                    {

                        // 移除 Base64 開頭的 `data:image/png;base64,` 這類字串
                        string base64Data = Files[i].Contains(",") ? Files[i].Split(',')[1] : Files[i];
                        base64Data = base64Data.Trim().Replace("\n", "").Replace("\r", "");
                        // 轉換為 byte[]
                        byte[] fileBytes = Convert.FromBase64String(base64Data);


                        string FileID = GenerateNextPrefixedId(resultID, "F", 4, i + 1);

                        string addSql = @"INSERT INTO AccidentFile(FileID,AccidentID,FileImage,ResponseType)
                VALUES (@FileID,@AccidentID,@FileImage,@ResponseType)";

                        await DataBase.ExecuteAsync(addSql, new
                        {
                            FileID = FileID,
                            AccidentID = AccidentId,
                            FileImage = fileBytes,
                            ResponseType = (true),
                        }, transaction);

                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Logger.LogError(ex, "Failed to add accident reply for accident {AccidentId}", AccidentId);
                throw;
            }

        }


        public async Task<int> DeleteDetail(string id)
        {
            await EnsureConnectionOpenAsync();
            using var transaction = DataBase.BeginTransaction();

            try
            {
                string fileSql = @"DELETE FROM AccidentFile WHERE AccidentID = @AccidentID";
                int rowsFileAffected = await DataBase.ExecuteAsync(fileSql, new { AccidentID = id }, transaction);

                string sql = @"DELETE FROM Accident WHERE AccidentID = @AccidentID";
                int rowsAffected = await DataBase.ExecuteAsync(sql, new { AccidentID = id }, transaction);

                transaction.Commit();
                return rowsAffected + rowsFileAffected;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Logger.LogError(ex, "Failed to delete accident {AccidentId}", id);
                throw;
            }
        }

        private static int GetTotalPages(int total)
        {
            return (int)Math.Ceiling(total / (double)PageSize);
        }

        private static string GenerateNextPrefixedId(string? latestId, string prefix, int digits, int increment = 1)
        {
            int current = 0;
            if (!string.IsNullOrEmpty(latestId) && latestId.StartsWith(prefix) && latestId.Length > prefix.Length)
            {
                int.TryParse(latestId[prefix.Length..], out current);
            }

            return prefix + (current + increment).ToString().PadLeft(digits, '0');
        }

        private async Task EnsureConnectionOpenAsync()
        {
            if (DataBase.State != ConnectionState.Open)
            {
                await DataBase.OpenAsync();
            }
        }

    }
}
