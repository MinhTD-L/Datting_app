using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObject
{
    public class WSMessageDto
    {
        public string Type { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Content { get; set; }

        public List<string> MessageIDs { get; set; }

        public string With { get; set; }
        public List<string> WithMany { get; set; }

        public string Status { get; set; }

        public int Page { get; set; }
        public int PageSize { get; set; }

        public string TempId { get; set; }
        public string Id { get; set; }

        public string LookingFor { get; set; }

        public object Offer { get; set; }
        public object Answer { get; set; }

        public IceCandidateDto Candidate { get; set; }

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
        public string Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }

        public DateTime SentAt { get; set; }
    }
    public class ContactDto
    {
        public string UserID { get; set; }
        public string Username { get; set; }
        public string Status { get; set; }
        public string Avatar { get; set; }

        public int UnreadCount { get; set; }

        public string LastMessage { get; set; }
        public DateTime LastTime { get; set; }

        public string LastSenderID { get; set; }
    }
    public class ChatHistoryDto
    {
        public string Type { get; set; }

        public string With { get; set; }

        public List<ChatMessageDto> Messages { get; set; }

        public int Page { get; set; }
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
        public string Type { get; set; } = "join_match";

        public string LookingFor { get; set; }
    }
    public class TypingDto
    {
        public string Type { get; set; } = "typing";

        public string With { get; set; }
    }
}
