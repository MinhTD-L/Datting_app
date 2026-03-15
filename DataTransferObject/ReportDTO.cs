using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObject
{
    public class ReportDTO
    {
        public string ReportId { get; set; } = "";

        public string ReporterId { get; set; } = "";

        public string ReportedUserId { get; set; } = "";

        public string Reason { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}
