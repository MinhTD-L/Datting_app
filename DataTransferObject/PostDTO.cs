using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTransferObject;

namespace DataTransferObject
{
    public class PostMedia
    {
        public string Url { get; set; }
        public string Type { get; set; }
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
        public string Id { get; set; }
        public string Content { get; set; }
        public List<PostMedia> Media { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public UserSimpleDto User { get; set; }
    }
    public class UserSimpleDto
    {
        public string UserID { get; set; }
        public string Username { get; set; }
        public string AvatarURL { get; set; }
    }
    
    public class GetPostResponseDTO
    {
        public List<PostFeedDTO> Posts { get; set; }
    }
    public class CommentPostDTO
    {
        public string PostID { get; set; }
        public string Content { get; set; }
    }
    public class CommentResonseDTO
    {
        public string Id { get; set; }
        public string PostID { get; set; }
        public string UserID { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

    }
    public class LikePostDTO
    {
        public string PostID { get; set; }
    }
    public class  UploadMediaResponeDTO
    {
        public string Url { get; set; }
    }

}