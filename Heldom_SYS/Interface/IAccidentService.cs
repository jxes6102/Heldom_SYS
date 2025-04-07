using Heldom_SYS.Models;
using Heldom_SYS.CustomModel;
using Microsoft.AspNetCore.Mvc;

namespace Heldom_SYS.Interface
{
    public interface IAccidentService
    {
        Task<IEnumerable<Accident>> GetReport(AccidentReq req);

        Task<int> GetReportPage();

        Task<IEnumerable<AccidentRes>> GetTrack(AccidentReq req);

        Task<int> GetTrackPage(AccidentReq req);

        Task<Accident> GetDetail(string id);

        Task AddAccident(string AccidentType, string AccidentTitle,string Description,string StartTime, string AccidentId, List<string> Files);

        Task AddReply(string Reply,string AccidentId,string Status, string EndTime, List<string> Files);

        Task<IEnumerable<AccidentFile>> GetDetailFile(string id, bool type);

        Task<int> DeleteDetail(string id);

    }
}
