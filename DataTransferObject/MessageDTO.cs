using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DataTransferObject
{
    public class WSMessageDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("from")]
        public string From { get; set; }
        [JsonPropertyName("to")]
        public string To { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("message_ids")]
        public List<string> MessageIDs { get; set; }

        [JsonPropertyName("with")]
        public string With { get; set; }
        [JsonPropertyName("with_many")]
        public List<string> WithMany { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }
        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        [JsonPropertyName("tempId")]
        public string TempId { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("looking_for")]
        public string LookingFor { get; set; }

        [JsonPropertyName("offer")]
        public object Offer { get; set; }
        [JsonPropertyName("answer")]
        public object Answer { get; set; }

        [JsonPropertyName("candidate")]
        public IceCandidateDto Candidate { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }
    }
    public class IceCandidateDto
    {
        public string Candidate { get; set; }
        public string SdpMid { get; set; }
        public int? SdpMLineIndex { get; set; }
    }
    public class ChatMessageDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("tempId")]
        public string TempId { get; set; }
        [JsonPropertyName("from")]
        public string From { get; set; }
        [JsonPropertyName("to")]
        public string To { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("sent_at")]
        public DateTime SentAt { get; set; }

        // BE ack uses "timestamp"
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }
    }
    public class ContactDto
    {
        [JsonPropertyName("UserID")]
        public string UserID { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        [JsonPropertyName("unread_count")]
        public int UnreadCount { get; set; }

        [JsonPropertyName("last_message")]
        public string LastMessage { get; set; }
        [JsonPropertyName("last_time")]
        public DateTime LastTime { get; set; }

        [JsonPropertyName("last_sender_id")]
        public string LastSenderID { get; set; }
    }
    public class ChatHistoryDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("with")]
        public string With { get; set; }

        [JsonPropertyName("messages")]
        public List<ChatMessageDto> Messages { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }
    }

    public class ContactsDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("contacts")]
        public List<ContactDto> Contacts { get; set; } = new();

        [JsonPropertyName("page")]
        public int Page { get; set; }
    }

    public class UserDetailsDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("to")]
        public string To { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
    public class DeleteMessageDto
    {
        public string Type { get; set; } = "delete_message";

        public List<string> MessageIDs { get; set; }

        public string To { get; set; }
    }
    public class CallOfferDto
    {
        public string Type { get; set; } = "offer";

        public string To { get; set; }

        public string Content { get; set; } // voice / video

        public object Offer { get; set; }
    }
    public class CallAnswerDto
    {
        public string Type { get; set; } = "answer";

        public string To { get; set; }

        public string Id { get; set; }

        public object Answer { get; set; }
    }
    public class JoinMatchDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "join_match";

        [JsonPropertyName("looking_for")]
        public string LookingFor { get; set; }
    }

    public class WaitingDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class MatchedDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("with")]
        public string With { get; set; }

        [JsonPropertyName("session_id")]
        public string SessionId { get; set; }
    }

    public class LeftQueueDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        // Optional (backend may include these keys)
        [JsonPropertyName("with")]
        public string With { get; set; }

        [JsonPropertyName("session_id")]
        public string SessionId { get; set; }
    }

    public class LeaveMatchDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "leave_match";
    }
    public class TypingDto
    {
        public string Type { get; set; } = "typing";

        public string With { get; set; }
    }
}
