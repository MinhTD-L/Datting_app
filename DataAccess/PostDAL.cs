using DataTransferObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DataAccess
{
    public class PostDAL
    {
        private readonly ApiClient  _api = new ApiClient();

        public async Task<GetPostResponseDTO> GetPost()
        {

            return await _api.GetAsync<GetPostResponseDTO>("interact/post");
        }

        public async Task<GetPostResponseDTO> GetMyPosts(int limit = 20, int page = 1)
        {
            if (limit <= 0 || limit > 20) limit = 20;
            if (page <= 0) page = 1;

            return await _api.GetAsync<GetPostResponseDTO>($"interact/post/me?limit={limit}&page={page}");
        }
        public async Task<GetPostResponseDTO> GetUserPosts(string userId, int limit = 20, int page = 1)
        {
            if (limit <= 0 || limit > 20) limit = 20;
            if (page <= 0) page = 1;

            return await _api.GetAsync<GetPostResponseDTO>($"interact/user/{userId}/posts?limit={limit}&page={page}");
        }
        public async Task<CreateResponeDTO> CreatePost(CreatePostDTO dto)
        {
            return await _api.PostAsync<CreatePostDTO, CreateResponeDTO>(
                "interact/post",
                dto
                );
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

            var contextType = string.IsNullOrWhiteSpace(dto.Type) ? "chat" : dto.Type;
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
                throw new Exception($"Upload failed ({(int)resp.StatusCode}) {resp.ReasonPhrase} [{new Uri(client.BaseAddress!, uploadEndpoint)}]: {responseText}");

            var candidate = ExtractFirstJsonValue(responseText);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // 1) Try parse as JSON document (object/string)
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

            // 2) Try deserialize to DTO (best-effort)
            try
            {
                var dtoRes = JsonSerializer.Deserialize<UploadMediaResponeDTO>(candidate, options);
                if (dtoRes != null && !string.IsNullOrWhiteSpace(dtoRes.Url))
                    return dtoRes;
            }
            catch
            {
                // ignore
            }

            // 3) Plain-text extraction: find "/static/..." or "http..."
            var urlText = TryExtractUrl(responseText) ?? TryExtractUrl(candidate);
            if (!string.IsNullOrWhiteSpace(urlText))
                return new UploadMediaResponeDTO { Url = urlText.Trim() };

            throw new Exception("Upload failed: cannot parse response: " + responseText);
        }

        private static string? TryExtractUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var s = input.Trim();

            // If the entire response is the url (common)
            if (s.StartsWith("/static/", StringComparison.OrdinalIgnoreCase) ||
                s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                s.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return TrimUrlToken(s);
            }

            // Otherwise, try locate a token inside the text
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

                // stop at whitespace or quote or closing brace/bracket
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
        public async Task<PostFeedDTO> GetPost(string postId)
        {
            var wrapped = await _api.GetAsync<GetPostDetailResponseDTO>($"interact/post/{postId}");
            return wrapped?.Post;
        }
        public async Task<CommentResonseDTO> Comment(CommentPostDTO dto)
        {
            var responseText = await _api.PostRawAsync("interact/post/comment", dto);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try
            {
                using var doc = JsonDocument.Parse(responseText);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("comment", out var commentEl))
                {
                    return JsonSerializer.Deserialize<CommentResonseDTO>(commentEl.GetRawText(), options);
                }

                return JsonSerializer.Deserialize<CommentResonseDTO>(responseText, options);
            }
            catch (JsonException)
            {
                var responseJson = ExtractFirstJsonValue(responseText);

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("comment", out var commentEl))
                {
                    return JsonSerializer.Deserialize<CommentResonseDTO>(commentEl.GetRawText(), options);
                }

                return JsonSerializer.Deserialize<CommentResonseDTO>(responseJson, options);
            }
        }
        public async Task<CommentResonseDTO> ReplyComment(CommentPostDTO dto)
        {
            var responseText = await _api.PostRawAsync("interact/post/comment/reply", dto);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try
            {
                using var doc = JsonDocument.Parse(responseText);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("comment", out var commentEl))
                {
                    return JsonSerializer.Deserialize<CommentResonseDTO>(commentEl.GetRawText(), options);
                }

                return JsonSerializer.Deserialize<CommentResonseDTO>(responseText, options);
            }
            catch (JsonException)
            {
                var responseJson = ExtractFirstJsonValue(responseText);

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("comment", out var commentEl))
                {
                    return JsonSerializer.Deserialize<CommentResonseDTO>(commentEl.GetRawText(), options);
                }

                return JsonSerializer.Deserialize<CommentResonseDTO>(responseJson, options);
            }
        }
        public async Task<APIresponseDTO> DeleteComment(DeletePostDTO dto)
        {
            return await _api.PostAsync<DeletePostDTO, APIresponseDTO>(
                "interact/post/comment/delete",
                dto
            );
        }

        public async Task<DeletePostResponseDTO> DeletePost(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
                throw new ArgumentException("postId is required.", nameof(postId));

            return await _api.DeleteAsync<DeletePostResponseDTO>($"interact/post/{postId}");
        }

        public async Task<DeletePostResponseDTO> AdminDeletePost(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
                throw new ArgumentException("postId is required.", nameof(postId));

            return await _api.DeleteAsync<DeletePostResponseDTO>($"admin/posts/{postId}");
        }

        public async Task<APIresponseDTO> LikePost(LikePostDTO dto)
        {
            return await _api.PostAsync<LikePostDTO, APIresponseDTO>(
                "interact/post/like",
                dto
                );
        }
        public async Task<APIresponseDTO> UnlikePost(DeleteLikeDTO dto)
        {
            return await _api.DeleteAsync<DeleteLikeDTO, APIresponseDTO>(
                "/interact/post/like/delete",
                dto
            );
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
                {
                    return s.Substring(start, i - start + 1).Trim();
                }
            }

            return s.Substring(start).Trim();
        }
    }
}
