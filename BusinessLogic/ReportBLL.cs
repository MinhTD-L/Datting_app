using System.Threading.Tasks;
using DataAccess;
using DataTransferObject;

namespace BusinessLogic
{
    public class ReportBLL
    {
        private readonly ReportDAL _dal = new ReportDAL();

        public async Task ReportUserAsync(string userId, string reason, string desc)
        {
            await _dal.ReportUser(new ReportUserRequestDTO { reported_user_id = userId, reason = reason, description = desc });
        }

        public async Task ReportPostAsync(string postId, string reason, string desc)
        {
            await _dal.ReportPost(new ReportPostRequestDTO { reported_post_id = postId, reason = reason, description = desc });
        }

        public async Task<GetReportsResponseDTO> GetReportsAsync()
        {
            return await _dal.GetReports();
        }

        public async Task ReviewReportAsync(string reportId, string status, string note)
        {
            await _dal.ReviewReport(reportId, new ReviewReportRequestDTO { status = status, review_note = note });
        }
    }
}