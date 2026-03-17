using DataAccess;

namespace BusinessLogic
{
    public static class AppServices
    {
        private static readonly PostDAL PostDal = new PostDAL();
        private static readonly UserDAL UserDal = new UserDAL();

        public static PostBLL PostBll { get; } = new PostBLL(PostDal);
        public static UserBLL UserBll { get; } = new UserBLL(UserDal);
    }
}

