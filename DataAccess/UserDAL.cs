using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTransferObject;


namespace DataAccess
{
    public class UserDAL
    {
        private readonly ApiClient _api=new ApiClient();
        public async Task<LoginResultDTO> Login(LoginInputDTO dto)
        {
            return await _api.PostAsync<LoginInputDTO, LoginResultDTO>(
                "user/login",
                dto
            );
        }
        public async Task<APIresponseDTO> Register(RegisterInput registerInput)
        {
            return await _api.PostAsync<RegisterInput, APIresponseDTO>(
                "user/register",
                registerInput
                );
        }
        public async Task<APIresponseDTO> VerifyEmail(verifyEmailDTO dto)
        {
            return await _api.PostAsync<verifyEmailDTO, APIresponseDTO>(
                "user/verify",
                dto
                );
        }
        public async Task<APIresponseDTO> ResetPassReq(ResetPasswordRequestDTO dto)
        {
            return await _api.PostAsync<ResetPasswordRequestDTO, APIresponseDTO>(
                "user/reset_password",
                dto
                );
        }
        public async Task<APIresponseDTO> ResetPass(ResetPasswordDTO dto)
        {
            return await _api.PostAsync<ResetPasswordDTO, APIresponseDTO>(
                "user/reset_password",
                dto
                );
        }
        public async Task<UserProfileDTO> GetProfile()
        {
            return await _api.GetAsync<UserProfileDTO>(
               "user/profile"
                );
        }

    }
}
