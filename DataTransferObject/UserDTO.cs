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
        [JsonPropertyName("role")]
        public string Role { get; set; }
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
        public List<string> Tags { get; set; }
    }

    public class SetupProfileRequestDTO
    {
        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        // BE expects "YYYY-MM-DD"
        [JsonPropertyName("date_of_birth")]
        public string DateOfBirth { get; set; }

        [JsonPropertyName("bio")]
        public string Bio { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }
    }

    public class SetupProfileResponseDTO
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("user")]
        public SetupProfileUserDTO User { get; set; }
    }

    public class SetupProfileUserDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }
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
        public List<string> Tags { get; set; }
    }

    public class GetMyProfileResponseDTO
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("user")]
        public MyProfileUserDTO User { get; set; }
    }

    public class MyProfileUserDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        [JsonPropertyName("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [JsonPropertyName("bio")]
        public string Bio { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }
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

    public class DeletePostResponseDTO
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}