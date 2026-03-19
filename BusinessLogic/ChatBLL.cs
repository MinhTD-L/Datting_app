using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DataTransferObject;

namespace BusinessLogic
{
    public sealed class ChatBLL
    {
        private readonly ChatSocketDAL _socket;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        private bool _onlineSent = false;

        public bool IsConnected => _socket?.IsConnected == true;

        public event Action Connected;
        public event Action Disconnected;
        public event Action<string> Error;

        // Matchmaking (realtime)
        public event Action WaitingReceived;
        public event Action<string, string> MatchedReceived; // withUserId, sessionId
        // withUserId + sessionId are optional depending on backend payload.
        public event Action<string, string> LeftQueueReceived; // withUserId, sessionId

        public event Action<IReadOnlyList<ContactDto>> ContactsUpdated;
        public event Action<string, IReadOnlyList<ChatMessageDto>, int> HistoryLoaded; 
        public event Action<ChatMessageDto> MessageReceived;
        public event Action<string> TypingReceived; 
        public event Action<string> SeenReceived; 
        public event Action<string, bool> UserOnlineChanged; 
        public event Action<IReadOnlyList<string>> MessageDeleted; 
        public event Action<UserDetailsDto> UserDetailsReceived;

        public ChatBLL(ChatSocketDAL socket)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));

            _socket.OnConnected += async () =>
            {
                Connected?.Invoke();
                try
                {
                    if (!_onlineSent)
                    {
                        _onlineSent = true;
                        await SetOnlineAsync();
                    }
                }
                catch
                {
                }
            };
            _socket.OnDisconnected += () => Disconnected?.Invoke();
            _socket.OnError += (e) => Error?.Invoke(e);
            _socket.OnMessageReceived += HandleIncoming;
        }

        public async Task EnsureConnectedAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("token is required.", nameof(token));

            if (IsConnected) return;
            await _socket.Connect(token);
        }

        public Task SendTextAsync(string toUserId, string content)
        {
            if (string.IsNullOrWhiteSpace(toUserId))
                throw new ArgumentException("toUserId is required.", nameof(toUserId));

            content = content?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(content))
                return Task.CompletedTask;
            return _socket.SendMessage(new WSMessageDto
            {
                Type = "text",
                From = SessionManager.UserId,
                To = toUserId,
                Content = content,
                TempId = Guid.NewGuid().ToString("N")
            });
        }

        public Task SendMediaAsync(string toUserId, string type, string urlOrText)
        {
            if (string.IsNullOrWhiteSpace(toUserId))
                throw new ArgumentException("toUserId is required.", nameof(toUserId));

            var payload = (urlOrText ?? "").Trim();
            if (string.IsNullOrWhiteSpace(payload))
                return Task.CompletedTask;

            return _socket.SendMessage(new WSMessageDto
            {
                Type = string.IsNullOrWhiteSpace(type) ? "file" : type,
                From = SessionManager.UserId,
                To = toUserId,
                Content = payload,
                TempId = Guid.NewGuid().ToString("N")
            });
        }

        public Task SendTypingAsync(string withUserId)
        {
            if (string.IsNullOrWhiteSpace(withUserId))
                return Task.CompletedTask;

            return _socket.SendMessage(new WSMessageDto
            {
                Type = "typing",
                With = withUserId
            });
        }

        public Task LoadHistoryAsync(string withUserId, int page = 0, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(withUserId))
                throw new ArgumentException("withUserId is required.", nameof(withUserId));

            return _socket.SendMessage(new WSMessageDto
            {
                Type = "load_history",
                With = withUserId,
                Page = page,
                PageSize = pageSize
            });
        }

        public Task LoadContactsAsync()
        {
            return _socket.SendMessage(new WSMessageDto { Type = "load_contacts" });
        }

        public Task SetOnlineAsync()
        {
            return _socket.SendMessage(new WSMessageDto { Type = "set_online" });
        }

        public Task SetOfflineAsync()
        {
            return _socket.SendMessage(new WSMessageDto { Type = "set_offline" });
        }

        public Task JoinMatchAsync(string lookingFor)
        {
            if (string.IsNullOrWhiteSpace(lookingFor))
                return Task.CompletedTask;

            // Backend contract: { type: "join_match", looking_for: "male"/"female"/"other" }
            return _socket.SendMessage(new JoinMatchDto { LookingFor = lookingFor });
        }

        public Task LeaveMatchAsync()
        {
            // Backend contract: { type: "leave_match" }
            return _socket.SendMessage(new LeaveMatchDto());
        }

        public Task RequestUserDetailsAsync(string toUserId)
        {
            if (string.IsNullOrWhiteSpace(toUserId))
                return Task.CompletedTask;

            return _socket.SendMessage(new WSMessageDto { Type = "user_details", To = toUserId });
        }

        public Task MarkSeenAsync(string withUserId)
        {
            if (string.IsNullOrWhiteSpace(withUserId))
                return Task.CompletedTask;

            return _socket.SendMessage(new WSMessageDto { Type = "seen", With = withUserId });
        }

        public Task RecallMessagesAsync(string toUserId, IReadOnlyList<string> messageIds)
        {
            if (string.IsNullOrWhiteSpace(toUserId))
                throw new ArgumentException("toUserId is required.", nameof(toUserId));
            if (messageIds == null || messageIds.Count == 0)
                return Task.CompletedTask;

            var ids = new List<string>();
            foreach (var id in messageIds)
            {
                var s = (id ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(s))
                    ids.Add(s);
            }
            if (ids.Count == 0)
                return Task.CompletedTask;

            return _socket.SendMessage(new WSMessageDto
            {
                Type = "delete_message",
                To = toUserId,
                MessageIDs = ids
            });
        }

        private void HandleIncoming(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("type", out var typeEl))
                    return;

                var type = typeEl.GetString() ?? "";

                switch (type)
                {
                    case "waiting":
                    {
                        // Waiting payload has no extra fields.
                        WaitingReceived?.Invoke();
                        break;
                    }
                    case "matched":
                    {
                        var dto = JsonSerializer.Deserialize<MatchedDto>(json, _jsonOptions);
                        if (dto != null)
                            MatchedReceived?.Invoke(dto.With ?? "", dto.SessionId ?? "");
                        break;
                    }
                    case "left_queue":
                    {
                        var dto = JsonSerializer.Deserialize<LeftQueueDto>(json, _jsonOptions);
                        if (dto != null)
                            LeftQueueReceived?.Invoke(dto.With ?? "", dto.SessionId ?? "");
                        else
                            LeftQueueReceived?.Invoke("", "");
                        break;
                    }
                    case "contacts":
                    {
                        var dto = JsonSerializer.Deserialize<ContactsDto>(json, _jsonOptions);
                        ContactsUpdated?.Invoke(dto?.Contacts ?? new List<ContactDto>());
                        break;
                    }
                    case "history":
                    {
                        var dto = JsonSerializer.Deserialize<ChatHistoryDto>(json, _jsonOptions);
                        HistoryLoaded?.Invoke(dto?.With ?? "", dto?.Messages ?? new List<ChatMessageDto>(), dto?.Page ?? 0);
                        break;
                    }
                    case "typing":
                    {
                        var msg = JsonSerializer.Deserialize<WSMessageDto>(json, _jsonOptions);
                        if (!string.IsNullOrWhiteSpace(msg?.From))
                            TypingReceived?.Invoke(msg.From);
                        break;
                    }
                    case "seen":
                    {
                        var msg = JsonSerializer.Deserialize<WSMessageDto>(json, _jsonOptions);
                        if (!string.IsNullOrWhiteSpace(msg?.From))
                            SeenReceived?.Invoke(msg.From);
                        break;
                    }
                    case "set_online":
                    case "set_offline":
                    {
                        var msg = JsonSerializer.Deserialize<WSMessageDto>(json, _jsonOptions);
                        if (!string.IsNullOrWhiteSpace(msg?.From))
                            UserOnlineChanged?.Invoke(msg.From, type == "set_online");
                        break;
                    }
                    case "message_deleted":
                    {
                        var msg = JsonSerializer.Deserialize<WSMessageDto>(json, _jsonOptions);
                        MessageDeleted?.Invoke(msg?.MessageIDs ?? new List<string>());
                        break;
                    }
                    case "user_details":
                    {
                        var dto = JsonSerializer.Deserialize<UserDetailsDto>(json, _jsonOptions);
                        if (dto != null)
                            UserDetailsReceived?.Invoke(dto);
                        break;
                    }
                    default:
                    {
                        if (type is "text" or "image" or "video" or "voice" or "file" or "deleted" or "call")
                        {
                            var m = JsonSerializer.Deserialize<ChatMessageDto>(json, _jsonOptions);
                            if (m != null)
                            {
                                if (m.SentAt == default && m.Timestamp != default)
                                    m.SentAt = m.Timestamp;
                                MessageReceived?.Invoke(m);
                            }
                        }
                        break;
                    }
                }
            }
            catch
            {
                // ignore malformed messages
            }
        }
    }
}
