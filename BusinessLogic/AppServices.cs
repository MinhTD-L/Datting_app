using DataAccess;

namespace BusinessLogic
{
    public static class AppServices
    {
        private static readonly PostDAL PostDal = new PostDAL();
        private static readonly UserDAL UserDal = new UserDAL();
        private static readonly FriendDAL FriendDal = new FriendDAL();
        private static readonly ChatSocketDAL ChatSocketDal = new ChatSocketDAL();
 
        public static PostBLL PostBll { get; } = new PostBLL(PostDal);
        public static UserBLL UserBll { get; } = new UserBLL(UserDal);
        public static FriendBLL FriendBll { get; } = new FriendBLL(FriendDal);
        public static ChatBLL ChatBll { get; } = new ChatBLL(ChatSocketDal);
        public static AdminBLL AdminBll { get; } = new AdminBLL();
    }
}
