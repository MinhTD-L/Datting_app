using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObject
{
    public class FriendRequestDTO
    {
        public string RequestId { get; set; } = "";

        public string SenderId { get; set; } = "";

        public string ReceiverId { get; set; } = "";

        public string Status { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}
