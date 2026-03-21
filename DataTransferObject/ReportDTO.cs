using System;
using System.Collections.Generic;

namespace DataTransferObject
{
    public class ReportPostRequestDTO
    {
        public string reported_post_id { get; set; }
        public string reason { get; set; }
        public string description { get; set; }
    }

    public class ReportUserRequestDTO
    {
        public string reported_user_id { get; set; }
        public string reason { get; set; }
        public string description { get; set; }
    }

    public class ReportDTO
    {
        public string ReportID { get; set; }
        public string ReporterID { get; set; }
        public string TargetUserID { get; set; }
        public string TargetPostID { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } 
        public string ResolveNote { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GetReportsResponseDTO
    {
        public List<ReportDTO> reports { get; set; }
    }

    public class ReviewReportRequestDTO
    {
        public string status { get; set; }
        public string review_note { get; set; }
    }
}