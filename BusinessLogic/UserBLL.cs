using System;
using System.Threading.Tasks;
using DataAccess;
using DataTransferObject;

namespace BusinessLogic
{
    public class UserBLL
    {
        private readonly UserDAL _userDal;

        public UserBLL(UserDAL userDal)
        {
            _userDal = userDal ?? throw new ArgumentNullException(nameof(userDal));
        }

        public Task<LoginResultDTO> LoginAsync(LoginInputDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            return _userDal.Login(dto);
        }

        public Task<APIresponseDTO> RegisterAsync(RegisterInput dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            return _userDal.Register(dto);
        }

        public Task<APIresponseDTO> VerifyEmailAsync(verifyEmailDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            return _userDal.VerifyEmail(dto);
        }

        public async Task<UserProfileDTO> GetProfileAsync()
        {
            return await _userDal.GetProfile();
        }

        public async Task<UserProfileDTO> GetMyProfileAsync()
        {
            var res = await _userDal.GetMyProfile();
            var u = res?.User;

            return new UserProfileDTO
            {
                Id = u?.Id,
                UserName = u?.Username,
                FullName = u?.FullName,
                AvatarUrl = u?.AvatarUrl,
                Gender = u?.Gender,
                DateOfBirth = u?.DateOfBirth,
                Bio = u?.Bio
            };
        }

        public async Task<UserProfileDTO> GetUserProfileAsync(string userId)
        {
            var res = await _userDal.GetUserProfile(userId);
            var u = res?.User;

            return new UserProfileDTO
            {
                Id = u?.Id,
                UserName = u?.Username,
                FullName = u?.FullName,
                AvatarUrl = u?.AvatarUrl,
                Gender = u?.Gender,
                DateOfBirth = u?.DateOfBirth,
                Bio = u?.Bio
            };
        }

        public async Task<UserProfileDTO> SetupProfileAsync(SetupProfileDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            string dob = null;
            if (dto.DateOfBirth.HasValue)
                dob = dto.DateOfBirth.Value.ToString("yyyy-MM-dd");

            var res = await _userDal.SetupProfile(new SetupProfileRequestDTO
            {
                FullName = dto.FullName?.Trim(),
                AvatarUrl = dto.AvatarUrl?.Trim(),
                Gender = dto.Gender?.Trim(),
                DateOfBirth = dob,
                Bio = dto.Bio?.Trim()
            });

            var u = res?.User;
            return new UserProfileDTO
            {
                Id = u?.Id,
                UserName = u?.Username,
                FullName = u?.FullName,
                AvatarUrl = u?.AvatarUrl
            };
        }

        public async Task<string> UploadAvatarAsync(string filePath)
        {
            var res = await _userDal.UploadMedia(new UploadMediaRequestDto
            {
                FilePath = filePath,
                Type = "avatar"
            });

            if (res == null || string.IsNullOrWhiteSpace(res.Url))
                throw new Exception("Upload avatar thất bại.");

            return res.Url;
        }

        public async Task<string> UploadMediaAsync(string filePath, string type)
        {
            var res = await _userDal.UploadMedia(new UploadMediaRequestDto
            {
                FilePath = filePath,
                Type = string.IsNullOrWhiteSpace(type) ? "file" : type
            });

            if (res == null || string.IsNullOrWhiteSpace(res.Url))
                throw new Exception("Upload media thất bại.");

            return res.Url;
        }
    }
}