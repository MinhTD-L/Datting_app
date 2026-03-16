using DataTransferObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataAccess
{
    public class PostDAL
    {
        private readonly ApiClient _api = new ApiClient();

        public async Task<GetPostResponseDTO> GetPost()
        {

            return await _api.GetAsync<GetPostResponseDTO>("interact/post");
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
