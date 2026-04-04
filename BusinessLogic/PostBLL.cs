using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DataAccess;
using DataTransferObject;

namespace BusinessLogic
{
    public class PostBLL
    {
        private readonly PostDAL _postDal;

        public PostBLL(PostDAL postDal)
        {
            _postDal = postDal ?? throw new ArgumentNullException(nameof(postDal));
        }

        public Task<GetPostResponseDTO> GetFeedAsync()
        {
            return _postDal.GetPost();
        }

        public Task<GetPostResponseDTO> GetMyPostsAsync(int limit = 20, int page = 1)
        {
            return _postDal.GetMyPosts(limit, page);
        }

        public Task<GetPostResponseDTO> GetUserPostsAsync(string userId, int limit = 20, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId is required.", nameof(userId));
            
            return _postDal.GetUserPosts(userId, limit, page);
        }

        public Task<PostFeedDTO> GetPostDetailAsync(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
                throw new ArgumentException("postId is required.", nameof(postId));

            return _postDal.GetPost(postId);
        }

        public Task LikeAsync(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
                throw new ArgumentException("postId is required.", nameof(postId));

            return _postDal.LikePost(new LikePostDTO { PostID = postId });
        }

        public Task UnlikeAsync(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
                throw new ArgumentException("postId is required.", nameof(postId));

            return _postDal.UnlikePost(new DeleteLikeDTO { PostID = postId });
        }

        public Task CommentAsync(string postId, string content)
        {
            if (string.IsNullOrWhiteSpace(postId))
                throw new ArgumentException("postId is required.", nameof(postId));

            content = content?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("content is required.", nameof(content));

            return _postDal.Comment(new CommentPostDTO
            {
                PostID = postId,
                Content = content,
                ParentID = null
            });
        }

        public Task ReplyCommentAsync(string postId, string parentCommentId, string content)
        {
            if (string.IsNullOrWhiteSpace(postId))
                throw new ArgumentException("postId is required.", nameof(postId));
            if (string.IsNullOrWhiteSpace(parentCommentId))
                throw new ArgumentException("parentCommentId is required.", nameof(parentCommentId));

            content = content?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("content is required.", nameof(content));

            return _postDal.ReplyComment(new CommentPostDTO
            {
                PostID = postId,
                Content = content,
                ParentID = parentCommentId
            });
        }

        public Task<DeletePostResponseDTO> DeletePostAsync(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
                throw new ArgumentException("postId is required.", nameof(postId));

            return _postDal.DeletePost(postId);
        }

        public Task<DeletePostResponseDTO> AdminDeletePostAsync(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
                throw new ArgumentException("postId is required.", nameof(postId));

            return _postDal.AdminDeletePost(postId);
        }

        public Task<APIresponseDTO> DeleteCommentAsync(string commentId)
        {
            if (string.IsNullOrWhiteSpace(commentId))
                throw new ArgumentException("commentId is required.", nameof(commentId));

            return _postDal.DeleteComment(new DeletePostDTO { PostID = commentId });
        }

        public async Task<CreateResponeDTO> CreatePostAsync(string content, List<string> mediaPaths)
        {
            content = content?.Trim() ?? string.Empty;
            mediaPaths ??= new List<string>();

            if (string.IsNullOrWhiteSpace(content) && mediaPaths.Count == 0)
                throw new Exception("Vui lòng nhập nội dung hoặc chọn ít nhất 1 media.");

            var uploaded = new List<PostMedia>();

            foreach (var path in mediaPaths)
            {
                var uploadResult = await _postDal.UploadMedia(new UploadMediaRequestDto
                {
                    FilePath = path,
                    Type = "post"
                });

                if (uploadResult == null || string.IsNullOrWhiteSpace(uploadResult.Url))
                    throw new Exception("Upload media thất bại.");

                uploaded.Add(new PostMedia
                {
                    Url = uploadResult.Url,
                    Type = GetPostMediaType(path)
                });
            }

            var payload = new CreatePostDTO
            {
                Content = content,
                Media = uploaded
            };

            var result = await _postDal.CreatePost(payload);

            if (result?.Post == null || string.IsNullOrWhiteSpace(result.Post.Id))
                throw new Exception("Tạo bài viết thất bại.");

            return result;
        }

        private static string GetPostMediaType(string filePath)
        {
            var ext = Path.GetExtension(filePath)?.ToLowerInvariant();

            return ext switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => "image",
                ".mp4" or ".mov" or ".avi" or ".mkv" or ".wmv" or ".webm" => "video",
                _ => throw new NotSupportedException($"File không hỗ trợ để đăng bài: {ext}")
            };
        }
    }
}