using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DataTransferObject;

namespace DataTransferObject
{
    public class UpdateUserRestrictionsRequestDTO
    {
        [JsonPropertyName("restrictions")]
        public List<string> Restrictions { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }
    }
}

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

        public async Task<GetMyProfileResponseDTO> GetMyProfile()
        {
            return await _api.GetAsync<GetMyProfileResponseDTO>("user/profile");
        }

        public async Task<GetMyProfileResponseDTO> GetUserProfile(string userId)
        {
            return await _api.GetAsync<GetMyProfileResponseDTO>($"user/profile/{userId}");
        }

        public async Task<SetupProfileResponseDTO> SetupProfile(SetupProfileRequestDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            return await _api.PutAsync<SetupProfileRequestDTO, SetupProfileResponseDTO>("user/profile", dto);
        }

        public async Task<UploadMediaResponeDTO> UploadMedia(UploadMediaRequestDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.FilePath)) throw new ArgumentException("FilePath is required.", nameof(dto));
            if (!File.Exists(dto.FilePath)) throw new FileNotFoundException("File not found.", dto.FilePath);

            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://litmatchclone-production-944b.up.railway.app/")
            };

            if (!string.IsNullOrEmpty(SessionManager.Token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", SessionManager.Token);
            }

            using var form = new MultipartFormDataContent();
            var contextType = string.IsNullOrWhiteSpace(dto.Type) ? "avatar" : dto.Type;
            form.Add(new StringContent(contextType), "type");

            using var fs = new FileStream(dto.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var fileContent = new StreamContent(fs);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(fileContent, "file", Path.GetFileName(dto.FilePath));

            const string uploadEndpoint = "/interact/upload-media";
            using var resp = await client.PostAsync(uploadEndpoint, form);
            if (resp.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException("Token expired");

            var responseText = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Upload failed ({(int)resp.StatusCode}) {resp.ReasonPhrase}: {responseText}");

            var candidate = ExtractFirstJsonValue(responseText);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try
            {
                using var doc = JsonDocument.Parse(candidate);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("url", out var urlEl))
                        return new UploadMediaResponeDTO { Url = urlEl.GetString() };
                    if (root.TryGetProperty("error", out var errEl))
                        throw new Exception(errEl.GetString() ?? "Upload failed");
                }
                if (root.ValueKind == JsonValueKind.String)
                {
                    var url = root.GetString();
                    if (!string.IsNullOrWhiteSpace(url))
                        return new UploadMediaResponeDTO { Url = url };
                }
            }
            catch (JsonException)
            {
            }

            try
            {
                var dtoRes = JsonSerializer.Deserialize<UploadMediaResponeDTO>(candidate, options);
                if (dtoRes != null && !string.IsNullOrWhiteSpace(dtoRes.Url))
                    return dtoRes;
            }
            catch
            {
            }

            var urlText = TryExtractUrl(responseText) ?? TryExtractUrl(candidate);
            if (!string.IsNullOrWhiteSpace(urlText))
                return new UploadMediaResponeDTO { Url = urlText.Trim() };

            throw new Exception("Upload failed: cannot parse response: " + responseText);
        }

        public async Task UpdateUserRestrictionsAsync(string userId, UpdateUserRestrictionsRequestDTO dto)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required.", nameof(userId));
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Assuming the endpoint is PUT admin/users/{userId}/restrictions
            await _api.PutAsync<UpdateUserRestrictionsRequestDTO, APIresponseDTO>($"admin/users/{userId}/restrictions", dto);
        }

        private static string? TryExtractUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var s = input.Trim();

            if (s.StartsWith("/static/", StringComparison.OrdinalIgnoreCase) ||
                s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                s.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return TrimUrlToken(s);
            }

            var idx = s.IndexOf("/static/", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
                return TrimUrlToken(s.Substring(idx));

            idx = s.IndexOf("http://", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
                return TrimUrlToken(s.Substring(idx));

            idx = s.IndexOf("https://", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
                return TrimUrlToken(s.Substring(idx));

            return null;

            static string TrimUrlToken(string token)
            {
                if (string.IsNullOrEmpty(token)) return token;
                int end = token.Length;
                for (int i = 0; i < token.Length; i++)
                {
                    var ch = token[i];
                    if (char.IsWhiteSpace(ch) || ch is '"' or '\'' or '}' or ']' or ',')
                    {
                        end = i;
                        break;
                    }
                }
                return token.Substring(0, end).Trim().Trim('"');
            }
        }

        private static string ExtractFirstJsonValue(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input ?? string.Empty;

            var s = input.TrimStart();
            var start = s.IndexOfAny(new[] { '{', '[' });
            if (start < 0)
                return s.Trim();

            bool inString = false;
            bool escaped = false;
            int depth = 0;
            char open = s[start];
            char close = open == '{' ? '}' : ']';

            for (int i = start; i < s.Length; i++)
            {
                var ch = s[i];

                if (inString)
                {
                    if (escaped)
                    {
                        escaped = false;
                        continue;
                    }
                    if (ch == '\\')
                    {
                        escaped = true;
                        continue;
                    }
                    if (ch == '"')
                        inString = false;
                    continue;
                }

                if (ch == '"')
                {
                    inString = true;
                    continue;
                }

                if (ch == open) depth++;
                if (ch == close) depth--;

                if (depth == 0)
                    return s.Substring(start, i - start + 1).Trim();
            }

            return s.Substring(start).Trim();
        }

    }
}
