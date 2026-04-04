using System;
using System.Threading.Tasks;
using DataTransferObject;

namespace DataAccess
{
    public class FriendDAL
    {
        private readonly ApiClient _api = new ApiClient();

        public Task<FriendRequestResponseDto> SendRequestAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required.", nameof(userId));

            // BE expects JSON key: user_id
            return _api.PostAsync<object, FriendRequestResponseDto>(
                "friend/request",
                new { user_id = userId }
            );
        }

        public Task<FriendRequestResponseDto> AcceptRequestAsync(string requestId)
        {
            if (string.IsNullOrWhiteSpace(requestId))
                throw new ArgumentException("requestId is required.", nameof(requestId));

            return _api.PostAsync<object, FriendRequestResponseDto>(
                "friend/accept",
                new { request_id = requestId }
            );
        }

        public Task<RejectFriendResponseDto> RejectRequestAsync(string requestId)
        {
            if (string.IsNullOrWhiteSpace(requestId))
                throw new ArgumentException("requestId is required.", nameof(requestId));

            return _api.PostAsync<object, RejectFriendResponseDto>(
                "friend/reject",
                new { request_id = requestId }
            );
        }

        public Task<FriendRequestsResponseDto> GetRequestsAsync()
        {
            return _api.GetAsync<FriendRequestsResponseDto>("friend/requests");
        }

        public Task<FriendsResponseDto> GetFriendsAsync()
        {
            return _api.GetAsync<FriendsResponseDto>("friend/list");
        }

        public Task<FriendsResponseDto> GetUserFriendsAsync(string userId)
        {
            return _api.GetAsync<FriendsResponseDto>($"friend/list/{userId}");
        }
    }
}
