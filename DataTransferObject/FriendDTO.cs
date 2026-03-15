using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObject
{
    public class SendFriendRequestDto
    {
        public string UserId { get; set; }
    }
    public class FriendRequestResponseDto
    {
        public string Status { get; set; }
    }
    public class FriendRequestDto
    {
        public string Id { get; set; }

        public string RequesterId { get; set; }

        public string ReceiverId { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
    public class FriendRequestsResponseDto
    {
        public List<FriendRequestDto> Requests { get; set; }
    }
    public class AcceptFriendRequestDto
    {
        public string RequestId { get; set; }
    }
    public class RejectFriendRequestDto
    {
        public string RequestId { get; set; }
    }
    public class RejectFriendResponseDto
    {
        public string Status { get; set; }
    }
    public class RemoveFriendDto
    {
        public string FriendId { get; set; }
    }
    public class RemoveFriendResponseDto
    {
        public string Status { get; set; }

        public string Message { get; set; }
    }
    public class FriendDto
    {
        public string UserId { get; set; }

        public string Username { get; set; }

        public string AvatarUrl { get; set; }
    }
    public class FriendsResponseDto
    {
        public List<FriendDto> Friends { get; set; }
    }
}
