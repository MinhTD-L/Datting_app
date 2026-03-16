using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace DataTransferObject
{

    public class LoginInputDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class LoginResultDTO
    {
        public string Status { get; set; }
        public string Token { get; set; }
        public UserBasicDTO User { get; set; }
        public string Error { get; set; }
    }
    public class RegisterInput
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class UserBasicDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }
    }
    public class verifyEmailDTO
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
    public class SetupProfileDTO
    {
        public string AvatarUrl { get; set; }
        public string FullName { get; set; }
        public string Bio { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
    public class UserProfileDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        public string FullName { get; set; }
        public string Bio { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
    public class ChangePasswordDTO
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
    public class ResetPasswordRequestDTO
    {
        public string Email { get; set; }
    }
    public class ResetPasswordDTO
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }
    public class APIresponseDTO
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }
}