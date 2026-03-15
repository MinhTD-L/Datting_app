using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObject
{
    public class FriendDTO
    {
        public string FriendId { get; set; } = "";

        public string UserId { get; set; } = "";

        public string FriendUserId { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}
