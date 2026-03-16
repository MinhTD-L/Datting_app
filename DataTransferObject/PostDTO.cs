using System;
using System.Collections.Generic;
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

    public class CristPostDTO
    {
        public string Content { get; set; }
        public List<PostMedia> Media { get; set; }
    }

    public class PostSimpleDTO
    {
        public string Id { get; set; }
        public string Content { get; set; }
    }

    public class CristSponeDTO
    {
        public PostSimpleDTO Post { get; set; }
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

        [JsonPropertyName("comments")]
        public List<PostCommentDto> Comments { get; set; } = new();

        [JsonPropertyName("user")]
        public UserSimpleDto User { get; set; }
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
        public string PostID { get; set; }
    }

    public class UploadMediaResponeDTO
    {
        public string Url { get; set; }
    }
}