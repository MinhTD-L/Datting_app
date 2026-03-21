using System.Threading.Tasks;
using DataTransferObject;

namespace DataAccess
{
    public class ReportDAL
    {
        private readonly ApiClient _api = new ApiClient();

        public async Task<APIresponseDTO> ReportUser(ReportUserRequestDTO dto)
        {
            return await _api.PostAsync<ReportUserRequestDTO, APIresponseDTO>("reports/user", dto);
        }

        public async Task<APIresponseDTO> ReportPost(ReportPostRequestDTO dto)
        {
            return await _api.PostAsync<ReportPostRequestDTO, APIresponseDTO>("reports/post", dto);
        }

        public async Task<GetReportsResponseDTO> GetReports()
        {
            return await _api.GetAsync<GetReportsResponseDTO>("reports/");
        }

        public async Task<APIresponseDTO> ReviewReport(string reportId, ReviewReportRequestDTO dto)
        {
            return await _api.PutAsync<ReviewReportRequestDTO, APIresponseDTO>($"reports/{reportId}", dto);
        }
    }
}