using System;
using System.Threading.Tasks;
using DataAccess;
using DataTransferObject;

namespace BusinessLogic
{
    public class FriendBLL
    {
        private readonly FriendDAL _friendDal;

        public FriendBLL(FriendDAL friendDal)
        {
            _friendDal = friendDal ?? throw new ArgumentNullException(nameof(friendDal));
        }

        public Task<FriendsResponseDto> GetFriendsAsync() => _friendDal.GetFriendsAsync();
        
        public Task<FriendsResponseDto> GetUserFriendsAsync(string userId) => _friendDal.GetUserFriendsAsync(userId);

        public Task<FriendRequestsResponseDto> GetRequestsAsync() => _friendDal.GetRequestsAsync();

        public Task SendRequestAsync(string userId) => _friendDal.SendRequestAsync(userId);

        public Task AcceptRequestAsync(string requestId) => _friendDal.AcceptRequestAsync(requestId);

        public Task RejectRequestAsync(string requestId) => _friendDal.RejectRequestAsync(requestId);
    }
}
