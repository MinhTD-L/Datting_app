using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObject
{
    public class ReportUserDto
    {
        public string ReportedUserId { get; set; }

        public string Reason { get; set; }

        public string Description { get; set; }
    }
    public class ReportResponseDto
    {
        public string Message { get; set; }
    }
    public class ReportPostDto
    {
        public string ReportedPostId { get; set; }

        public string Reason { get; set; }

        public string Description { get; set; }
    }
    public class ReportSessionDto
    {
        public string ReportedSessionId { get; set; }

        public string Reason { get; set; }

        public string Description { get; set; }
    }
    public class ReportSessionResponseDto
    {
        public string Message { get; set; }

        public string ReportId { get; set; }
    }
    public class ReportDto
    {
        public string ReportId { get; set; }

        public string ReporterId { get; set; }

        public string TargetUserId { get; set; }

        public string TargetPostId { get; set; }

        public string TargetSessionId { get; set; }

        public string Reason { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public string ResolveNote { get; set; }

        public string ReviewedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }
    }
    public class ReportsResponseDto
    {
        public List<ReportDto> Reports { get; set; }
    }
    public class ReportDetailResponseDto
    {
        public ReportDto Report { get; set; }
    }
    public class ReviewReportDto
    {
        public string Status { get; set; }

        public string ReviewNote { get; set; }
    }

}
