using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataTransferObject
{
    public class PostMedia
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class CommentUserDto
    {
        [JsonPropertyName("user_id")]
        public string UserID { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarURL { get; set; }
    }

    public class PostCommentDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("parent_id")]
        public string ParentId { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("user")]
        public CommentUserDto User { get; set; }
    }

    public class GetPostDetailResponseDTO
    {
        [JsonPropertyName("post")]
        public PostFeedDTO Post { get; set; }
    }

    public class CreatePostDTO
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("PostMedia")]
        public List<PostMedia> Media { get; set; } = new();
    }

    public class PostSimpleDTO
    {
        public string Id { get; set; }
        public string Content { get; set; }
    }

    public class CreateResponeDTO
    {
        public PostSimpleDTO Post { get; set; }
    }
    public class DeletePostDTO
    {
        [JsonPropertyName("id")]
        public string PostID {  get; set; }
    }
    public class DeleteLikeDTO
    {
        [JsonPropertyName("post_id")]
        public string PostID { get; set; }
    }
    public class PostFeedDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("media")]
        public List<PostMedia> Media { get; set; } = new();

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("like_count")]
        public int LikeCount { get; set; }

        [JsonPropertyName("comment_count")]
        public int CommentCount { get; set; }

        [JsonPropertyName("is_liked")]
        public bool IsLiked { get; set; }

        [JsonPropertyName("comments")]
        public List<PostCommentDto> Comments { get; set; } = new();

        [JsonPropertyName("user")]
        public UserSimpleDto User { get; set; }

        // Some APIs expose "is_liked"/"liked" flags; keep extra fields to detect.
        [JsonExtensionData]
        public Dictionary<string, JsonElement> Extra { get; set; } = new();

        public bool GetIsLikedFallback()
        {
            // Prefer strongly-typed field when present
            if (IsLiked) return true;
            if (Extra == null || Extra.Count == 0) return false;

            static bool TryGetBool(Dictionary<string, JsonElement> dict, string key, out bool value)
            {
                value = false;
                if (!dict.TryGetValue(key, out var el)) return false;
                try
                {
                    if (el.ValueKind == JsonValueKind.True) { value = true; return true; }
                    if (el.ValueKind == JsonValueKind.False) { value = false; return true; }
                    if (el.ValueKind == JsonValueKind.String && bool.TryParse(el.GetString(), out var b)) { value = b; return true; }
                    if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var n)) { value = n != 0; return true; }
                }
                catch { }
                return false;
            }

            if (TryGetBool(Extra, "is_liked", out var v)) return v;
            if (TryGetBool(Extra, "isLiked", out v)) return v;
            if (TryGetBool(Extra, "liked", out v)) return v;
            if (TryGetBool(Extra, "has_liked", out v)) return v;
            return false;
        }
    }

    public class UserSimpleDto
    {
        [JsonPropertyName("user_id")]
        public string UserID { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarURL { get; set; }
    }

    public class GetPostResponseDTO
    {
        [JsonPropertyName("posts")]
        public List<PostFeedDTO> Posts { get; set; } = new();
    }

    public class CommentPostDTO
    {
        [JsonPropertyName("post_id")]
        public string PostID { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
        [JsonPropertyName("parent_id")]
        public string ParentID { get; set; }
    }

    public class CommentResonseDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("post_id")]
        public string PostID { get; set; }
        [JsonPropertyName("user_id")]
        public string UserID { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    public class CommentResponseWrapperDTO
    {
        [JsonPropertyName("comment")]
        public CommentResonseDTO Comment { get; set; }
    }

    public class LikePostDTO
    {
        [JsonPropertyName("post_id")]
        public string PostID { get; set; }
    }

    public class UploadMediaResponeDTO
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
    public class UploadMediaRequestDto
    {
        public string FilePath { get; set; }
        public string Type { get; set; }
    }

}