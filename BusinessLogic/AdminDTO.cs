using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace DataTransferObject
{
    public class PaginationDTO
    {
        [JsonPropertyName("limit")]
        public int Limit { get; set; }
        [JsonPropertyName("page")]
        public int Page { get; set; }
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class RestrictionInfoDTO
    {
        [JsonPropertyName("feature")]
        public string Feature { get; set; }

        [JsonPropertyName("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }
    }

    public class UserAdminViewDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }
        [JsonPropertyName("role")]
        public string Role { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("restrictions")]
        public List<RestrictionInfoDTO> Restrictions { get; set; }

        public List<string> GetRestrictionFeatures() =>
            Restrictions?.Select(r => r.Feature).ToList() ?? new List<string>();
    }

    public class GetUsersAdminResponseDTO
    {
        [JsonPropertyName("pagination")]
        public PaginationDTO Pagination { get; set; }
        [JsonPropertyName("users")]
        public List<UserAdminViewDTO> Users { get; set; }
    }

    // --- DTOs for Dashboard Stats ---

    public class UserStatsDTO
    {
        [JsonPropertyName("total")] public long Total { get; set; }
        [JsonPropertyName("by_status")] public Dictionary<string, int> ByStatus { get; set; }
        [JsonPropertyName("new_today")] public long NewToday { get; set; }
        [JsonPropertyName("new_last_7_days")] public long NewLast7Days { get; set; }
    }

    public class MatchingStatsDTO
    {
        [JsonPropertyName("successful_pairs")] public long SuccessfulPairs { get; set; }
        [JsonPropertyName("users_in_queue")] public int UsersInQueue { get; set; }
    }

    public class EngagementStatsDTO
    {
        [JsonPropertyName("posts")] public long Posts { get; set; }
        [JsonPropertyName("likes")] public long Likes { get; set; }
        [JsonPropertyName("comments")] public long Comments { get; set; }
        [JsonPropertyName("messages")] public long Messages { get; set; }
        [JsonPropertyName("calls")] public long Calls { get; set; }
    }

    public class SocialStatsDTO
    {
        [JsonPropertyName("friendships")] public long Friendships { get; set; }
        [JsonPropertyName("pending_friend_requests")] public long PendingFriendRequests { get; set; }
    }

    public class ModerationStatsDTO
    {
        [JsonPropertyName("total_reports")] public long TotalReports { get; set; }
        [JsonPropertyName("by_status")] public Dictionary<string, int> ByStatus { get; set; }
    }

    public class RealtimeStatsDTO
    {
        [JsonPropertyName("online_users")] public int OnlineUsers { get; set; }
    }

    public class DashboardStatsDTO
    {
        [JsonPropertyName("users")] public UserStatsDTO Users { get; set; }
        [JsonPropertyName("matching")] public MatchingStatsDTO Matching { get; set; }
        [JsonPropertyName("engagement")] public EngagementStatsDTO Engagement { get; set; }
        [JsonPropertyName("social")] public SocialStatsDTO Social { get; set; }
        [JsonPropertyName("moderation")] public ModerationStatsDTO Moderation { get; set; }
        [JsonPropertyName("realtime")] public RealtimeStatsDTO Realtime { get; set; }
    }

    // --- DTOs for Time Series Stats ---

    public class TimeSeriesDataPointDTO
    {
        [JsonPropertyName("date")] public string Date { get; set; }
        [JsonPropertyName("count")] public long Count { get; set; }
    }

    public class TimeSeriesResponseDTO
    {
        [JsonPropertyName("metric")] public string Metric { get; set; }
        [JsonPropertyName("data")] public List<TimeSeriesDataPointDTO> Data { get; set; }
    }

    // --- DTOs for Popular Tags Stats ---

    public class PopularTagStatDTO
    {
        [JsonPropertyName("tag")] public string Tag { get; set; }
        [JsonPropertyName("total_likes")] public long TotalLikes { get; set; }
        [JsonPropertyName("total_posts")] public long TotalPosts { get; set; }
        [JsonPropertyName("avg_likes")] public double AvgLikes { get; set; }
    }

    public class PopularTagsResponseDTO
    {
        [JsonPropertyName("data")]
        public List<PopularTagStatDTO> Data { get; set; }
    }
}