using System.Threading.Tasks;

namespace BA.WebAPI.Model
{
    public interface IWeeklyReportService
    {
        Task GenerateWeeklyReports(string userId);
    }
}

