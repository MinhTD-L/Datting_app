using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObject
{
    public class PairSessionDTO
    {
        public string SessionId { get; set; } = "";

        public string User1Id { get; set; } = "";

        public string User2Id { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}
