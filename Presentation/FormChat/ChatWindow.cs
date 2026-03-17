using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessLogic;
using DataTransferObject;

namespace Presentation.FormChat
{
    public sealed class ChatWindow : Form
    {
        private readonly ChatBLL _chatBll;
        private readonly string _withUserId;
        private readonly string _withName;
        private readonly string _withAvatar;

        private readonly FlowLayoutPanel _messages;
        private TextBox _input;
        private Button _btnSend;
        private Button _btnAttach;
        private Label _lblTyping;
        private Label _lblStatus;
        private PictureBox _avatarBox;

        private readonly UserBLL _userBll;

        private readonly System.Windows.Forms.Timer _typingTimer;
        private DateTime _lastTypingShownAt = DateTime.MinValue;

        private readonly HashSet<string> _messageIds = new(StringComparer.Ordinal);
        private readonly Dictionary<string, Control> _bubbleByTempId = new(StringComparer.Ordinal);
        private readonly Dictionary<string, Control> _bubbleByServerId = new(StringComparer.Ordinal);
        private readonly ContextMenuStrip _ownMessageMenu = new ContextMenuStrip();
        private readonly System.Collections.Generic.List<PendingOutgoing> _pendingOutgoing = new();

        private const string BaseUrl = "https://litmatchclone-production.up.railway.app";

        public ChatWindow(ChatBLL chatBll, string withUserId, string withName, string withAvatar)
        {
            _chatBll = chatBll ?? throw new ArgumentNullException(nameof(chatBll));
            _withUserId = withUserId ?? throw new ArgumentNullException(nameof(withUserId));
            _withName = withName ?? "Chat";
            _withAvatar = withAvatar;
            _userBll = BusinessLogic.AppServices.UserBll;

            Text = _withName;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 246, 250);
            MinimumSize = new Size(820, 600);

            var header = BuildHeader();
            _messages = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(245, 246, 250)
            };

            var footer = BuildFooter();

            Controls.Add(_messages);
            Controls.Add(footer);
            Controls.Add(header);

            BuildContextMenus();

            _typingTimer = new System.Windows.Forms.Timer { Interval = 2500 };
            _typingTimer.Tick += (_, __) =>
            {
                if ((DateTime.UtcNow - _lastTypingShownAt).TotalMilliseconds > 2200)
                    _lblTyping.Visible = false;
            };
            _typingTimer.Start();

            Shown += async (_, __) => await InitializeAsync();
            FormClosed += (_, __) => Unwire();
        }

        private void BuildContextMenus()
        {
            _ownMessageMenu.Items.Clear();

            var miRecall = new ToolStripMenuItem("Thu hồi tin nhắn");
            miRecall.Click += async (_, __) =>
            {
                if (_ownMessageMenu.Tag is not BubbleMeta meta)
                    return;

                if (string.IsNullOrWhiteSpace(meta.ServerId))
                {
                    MessageBox.Show(this, "Tin nhắn chưa gửi xong, vui lòng đợi 1 chút.", "Chưa thể thu hồi",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    await _chatBll.RecallMessagesAsync(_withUserId, new[] { meta.ServerId });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Không thể thu hồi: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            _ownMessageMenu.Items.Add(miRecall);
        }

        private Panel BuildHeader()
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Color.White,
                Padding = new Padding(12, 10, 12, 10)
            };

            var btnBack = new Button
            {
                Text = "←",
                Width = 40,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(245, 246, 250),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(12, 18)
            };
            btnBack.FlatAppearance.BorderColor = Color.FromArgb(230, 230, 230);
            btnBack.Click += (_, __) => Close();

            _avatarBox = new PictureBox
            {
                Size = new Size(44, 44),
                Location = new Point(btnBack.Right + 10, 14),
                BackColor = Color.FromArgb(238, 238, 238),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            ApplyRound(_avatarBox);

            var lblName = new Label
            {
                Text = _withName,
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(_avatarBox.Right + 10, 16)
            };

            _lblStatus = new Label
            {
                Text = "Đang kết nối...",
                AutoSize = true,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(_avatarBox.Right + 10, 40)
            };

            _lblTyping = new Label
            {
                Text = "Đang nhập...",
                AutoSize = true,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.FromArgb(255, 30, 100),
                Location = new Point(_avatarBox.Right + 10, 56),
                Visible = false
            };

            header.Controls.Add(btnBack);
            header.Controls.Add(_avatarBox);
            header.Controls.Add(lblName);
            header.Controls.Add(_lblStatus);
            header.Controls.Add(_lblTyping);
            header.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(235, 235, 235) });

            return header;
        }

        private Panel BuildFooter()
        {
            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            _input = new TextBox
            {
                Multiline = true,
                Height = 48,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 30, 30),
                ScrollBars = ScrollBars.Vertical
            };

            _btnSend = new Button
            {
                Text = "Gửi",
                Width = 96,
                Height = 48,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 30, 100),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnSend.FlatAppearance.BorderSize = 0;

            _btnAttach = new Button
            {
                Text = "📎",
                Width = 48,
                Height = 48,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(245, 246, 250),
                ForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnAttach.FlatAppearance.BorderColor = Color.FromArgb(230, 230, 230);

            footer.Controls.Add(_input);
            footer.Controls.Add(_btnAttach);
            footer.Controls.Add(_btnSend);
            footer.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(235, 235, 235) });

            void Layout()
            {
                _btnSend.Left = footer.ClientSize.Width - footer.Padding.Right - _btnSend.Width;
                _btnSend.Top = footer.Padding.Top + 10;
                _btnAttach.Left = _btnSend.Left - 10 - _btnAttach.Width;
                _btnAttach.Top = _btnSend.Top;
                _input.Left = footer.Padding.Left;
                _input.Top = footer.Padding.Top + 10;
                _input.Width = Math.Max(200, _btnAttach.Left - 10 - _input.Left);
            }

            footer.SizeChanged += (_, __) => Layout();
            Layout();

            _btnSend.Click += async (_, __) => await SendAsync();
            _btnAttach.Click += async (_, __) => await AttachAsync();
            _input.TextChanged += async (_, __) => await DebouncedTypingAsync();
            _input.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    e.SuppressKeyPress = true;
                    await SendAsync();
                }
            };

            return footer;
        }

        private async Task InitializeAsync()
        {
            Wire();
            try
            {
                await _chatBll.EnsureConnectedAsync(SessionManager.Token);
                _lblStatus.Text = "Đang tải thông tin...";
                await _chatBll.RequestUserDetailsAsync(_withUserId);
                await TryLoadHeaderAvatarAsync(_withAvatar);

                _messages.Controls.Clear();
                _messageIds.Clear();
                _bubbleByTempId.Clear();
                _bubbleByServerId.Clear();
                _pendingOutgoing.Clear();
                _messages.Controls.Add(MakeHint("Đang tải lịch sử..."));
                await _chatBll.LoadHistoryAsync(_withUserId, page: 0, pageSize: 30);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không thể mở chat: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void Wire()
        {
            _chatBll.Connected += OnConnected;
            _chatBll.Disconnected += OnDisconnected;
            _chatBll.Error += OnError;
            _chatBll.HistoryLoaded += OnHistoryLoaded;
            _chatBll.MessageReceived += OnMessageReceived;
            _chatBll.TypingReceived += OnTypingReceived;
            _chatBll.SeenReceived += _ => { };
            _chatBll.UserOnlineChanged += OnUserOnlineChanged;
            _chatBll.MessageDeleted += OnMessageDeleted;
            _chatBll.UserDetailsReceived += OnUserDetailsReceived;
        }

        private void Unwire()
        {
            _chatBll.Connected -= OnConnected;
            _chatBll.Disconnected -= OnDisconnected;
            _chatBll.Error -= OnError;
            _chatBll.HistoryLoaded -= OnHistoryLoaded;
            _chatBll.MessageReceived -= OnMessageReceived;
            _chatBll.TypingReceived -= OnTypingReceived;
            _chatBll.UserOnlineChanged -= OnUserOnlineChanged;
            _chatBll.MessageDeleted -= OnMessageDeleted;
            _chatBll.UserDetailsReceived -= OnUserDetailsReceived;
        }

        private void OnConnected()
        {
            if (IsDisposed) return;
            BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrWhiteSpace(_lblStatus.Text) || _lblStatus.Text.Contains("kết nối", StringComparison.OrdinalIgnoreCase))
                    _lblStatus.Text = "Đang tải...";
            }));
        }

        private void OnDisconnected()
        {
            if (IsDisposed) return;
            BeginInvoke(new Action(() => _lblStatus.Text = "Mất kết nối (đang tự nối lại)..."));
        }

        private void OnError(string e)
        {
            if (IsDisposed) return;
            if (string.IsNullOrWhiteSpace(e)) return;
            BeginInvoke(new Action(() => _lblStatus.Text = e));
        }

        private void OnUserOnlineChanged(string userId, bool online)
        {
            if (IsDisposed) return;
            if (!string.Equals(userId, _withUserId, StringComparison.Ordinal)) return;
            BeginInvoke(new Action(() =>
            {
                _lblStatus.Text = online ? "Online" : "Offline";
            }));
        }

        private void OnUserDetailsReceived(UserDetailsDto dto)
        {
            if (IsDisposed) return;
            if (dto == null) return;
            if (!string.Equals(dto.To, _withUserId, StringComparison.Ordinal)) return;

            BeginInvoke(new Action(async () =>
            {
                _lblStatus.Text = string.Equals(dto.Status, "active", StringComparison.OrdinalIgnoreCase) ? "Online" : "Offline";
                if (!string.IsNullOrWhiteSpace(dto.Avatar))
                    await TryLoadHeaderAvatarAsync(dto.Avatar);
            }));
        }

        private void OnTypingReceived(string from)
        {
            if (IsDisposed) return;
            if (!string.Equals(from, _withUserId, StringComparison.Ordinal)) return;
            BeginInvoke(new Action(() =>
            {
                _lastTypingShownAt = DateTime.UtcNow;
                _lblTyping.Visible = true;
            }));
        }

        private void OnHistoryLoaded(string withUserId, IReadOnlyList<ChatMessageDto> messages, int page)
        {
            if (IsDisposed) return;
            if (!string.Equals(withUserId, _withUserId, StringComparison.Ordinal)) return;

            BeginInvoke(new Action(() =>
            {
                _messages.Controls.Clear();
                if (messages == null || messages.Count == 0)
                {
                    _messages.Controls.Add(MakeHint("Chưa có tin nhắn. Gửi tin nhắn đầu tiên nhé."));
                    return;
                }

                foreach (var m in messages.OrderBy(x => x.SentAt))
                    AddMessageToUi(m, append: true);

                ScrollToBottom();
            }));
        }

        private void OnMessageReceived(ChatMessageDto m)
        {
            if (IsDisposed) return;
            if (m == null) return;

            var me = SessionManager.UserId;
            var isMine = string.Equals(m.From, me, StringComparison.Ordinal);
            var isRelevant =
                (isMine && string.Equals(m.To, _withUserId, StringComparison.Ordinal)) ||
                (!isMine && string.Equals(m.From, _withUserId, StringComparison.Ordinal));

            if (!isRelevant) return;

            BeginInvoke(new Action(async () =>
            {
                if (!TryReconcileDuplicate(m))
                {
                    AddMessageToUi(m, append: true);
                    ScrollToBottom();
                }

                if (!isMine)
                    await _chatBll.MarkSeenAsync(_withUserId);
            }));
        }

        private void OnMessageDeleted(IReadOnlyList<string> ids)
        {
            if (IsDisposed) return;
            if (ids == null || ids.Count == 0) return;

            BeginInvoke(new Action(() =>
            {
                foreach (var id in ids)
                {
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    if (_bubbleByServerId.TryGetValue(id, out var wrap))
                    {
                        MarkBubbleAsRecalled(wrap);
                        continue;
                    }

                    foreach (Control c in _messages.Controls)
                    {
                        if (c.Tag is BubbleMeta meta && string.Equals(meta.ServerId, id, StringComparison.Ordinal))
                        {
                            MarkBubbleAsRecalled(c);
                            break;
                        }
                        if (c.Tag is string tagId && string.Equals(tagId, id, StringComparison.Ordinal))
                        {
                            MarkBubbleAsRecalled(c);
                            break;
                        }
                    }
                }
            }));
        }

        private static void MarkBubbleAsRecalled(Control wrap)
        {
            if (wrap == null) return;
            wrap.Enabled = false;
            wrap.ForeColor = Color.Gray;

            // find bubble panel (first child) then replace content
            Panel bubble = null;
            foreach (Control c in wrap.Controls)
            {
                if (c is Panel p)
                {
                    bubble = p;
                    break;
                }
            }
            if (bubble == null) return;

            bubble.Controls.Clear();
            bubble.BackColor = Color.FromArgb(245, 246, 250);
            bubble.Padding = new Padding(10, 8, 10, 8);

            var lbl = new Label
            {
                Text = "Tin nhắn đã thu hồi",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
            };
            bubble.Controls.Add(lbl);
            bubble.Width = Math.Min(520, lbl.PreferredWidth + bubble.Padding.Horizontal);
            bubble.Height = lbl.Bottom + bubble.Padding.Bottom;
        }

        private async Task DebouncedTypingAsync()
        {
            // throttle typing: only send once per ~900ms
            if (string.IsNullOrWhiteSpace(_input.Text)) return;
            var now = DateTime.UtcNow;
            if ((now - _lastTypingSentAt).TotalMilliseconds < 900)
                return;
            _lastTypingSentAt = now;
            await _chatBll.SendTypingAsync(_withUserId);
        }

        private async Task SendAsync()
        {
            var text = (_input.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            _input.Clear();
            _input.Focus();

            var myTempId = Guid.NewGuid().ToString("N");
            // optimistic UI (pending)
            var msg = new ChatMessageDto
            {
                TempId = myTempId,
                From = SessionManager.UserId,
                To = _withUserId,
                Content = text,
                Type = "text",
                SentAt = DateTime.UtcNow
            };
            AddMessageToUi(msg, append: true);
            ScrollToBottom();

            try
            {
                _pendingOutgoing.Add(new PendingOutgoing
                {
                    TempId = myTempId,
                    To = _withUserId,
                    Type = "text",
                    Content = text,
                    CreatedAtUtc = msg.SentAt
                });

                await _chatBll.SendTextAsync(_withUserId, text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không thể gửi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task AttachAsync()
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Chọn tệp để gửi",
                Filter = "Ảnh|*.png;*.jpg;*.jpeg;*.webp|Tất cả|*.*",
                Multiselect = false
            };

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            var filePath = dlg.FileName;
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;

            SetBusy(true);
            try
            {
                // Upload first, then send URL through websocket content
                var ext = Path.GetExtension(filePath)?.ToLowerInvariant();
                var isImage = ext is ".png" or ".jpg" or ".jpeg" or ".webp";
                var type = isImage ? "image" : "file";

                var url = await _userBll.UploadMediaAsync(filePath, type);
                var myTempId = Guid.NewGuid().ToString("N");
                _pendingOutgoing.Add(new PendingOutgoing
                {
                    TempId = myTempId,
                    To = _withUserId,
                    Type = type,
                    Content = url,
                    CreatedAtUtc = DateTime.UtcNow
                });

                await _chatBll.SendMediaAsync(_withUserId, type, url);

                // optimistic bubble (preview for image, link for file)
                var msg = new ChatMessageDto
                {
                    TempId = myTempId,
                    From = SessionManager.UserId,
                    To = _withUserId,
                    Content = url,
                    Type = type,
                    SentAt = DateTime.UtcNow
                };
                AddMessageToUi(msg, append: true);
                ScrollToBottom();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không thể gửi tệp: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void SetBusy(bool busy)
        {
            UseWaitCursor = busy;
            if (_btnSend != null) _btnSend.Enabled = !busy;
            if (_btnAttach != null) _btnAttach.Enabled = !busy;
        }

        private bool TryReconcileDuplicate(ChatMessageDto incoming)
        {
            if (incoming == null) return false;

            // If server echoes our message, it includes tempId. Use that to reconcile.
            if (!string.IsNullOrWhiteSpace(incoming.Id) && _bubbleByServerId.ContainsKey(incoming.Id))
                return true;

            // BE generates its own tempId (not the client's), so reconcile for my own echoes by content+time window.
            var me = SessionManager.UserId;
            if (string.Equals(incoming.From, me, StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(incoming.Id))
            {
                var match = FindPendingMatch(incoming);
                if (match != null && _bubbleByTempId.TryGetValue(match.TempId, out var bubble))
                {
                    bubble.Tag = new BubbleMeta { StableId = incoming.Id, ServerId = incoming.Id, TempId = match.TempId, Mine = true };
                    _bubbleByServerId[incoming.Id] = bubble;
                    _messageIds.Add(incoming.Id);
                    match.ServerId = incoming.Id;
                    _pendingOutgoing.Remove(match);
                    return true;
                }
            }

            return false;
        }

        private PendingOutgoing FindPendingMatch(ChatMessageDto incoming)
        {
            // match newest pending by type+to+content within 15s
            var now = DateTime.UtcNow;
            for (int i = _pendingOutgoing.Count - 1; i >= 0; i--)
            {
                var p = _pendingOutgoing[i];
                if (!string.Equals(p.To, incoming.To, StringComparison.Ordinal)) continue;
                if (!string.Equals(p.Type, incoming.Type, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.Equals((p.Content ?? "").Trim(), (incoming.Content ?? "").Trim(), StringComparison.Ordinal)) continue;
                if ((now - p.CreatedAtUtc).TotalSeconds > 15) continue;
                return p;
            }
            return null;
        }

        private void AddMessageToUi(ChatMessageDto m, bool append)
        {
            if (m == null) return;
            var stableId = !string.IsNullOrWhiteSpace(m.Id) ? m.Id : m.TempId;
            if (!string.IsNullOrWhiteSpace(stableId) && _messageIds.Contains(stableId)) return;
            if (!string.IsNullOrWhiteSpace(stableId)) _messageIds.Add(stableId);

            // remove placeholder hint if present
            if (_messages.Controls.Count == 1 && _messages.Controls[0] is Label l && string.Equals(l.Tag as string, "hint", StringComparison.Ordinal))
                _messages.Controls.Clear();

            var me = SessionManager.UserId;
            var mine = string.Equals(m.From, me, StringComparison.Ordinal);

            var wrap = new Panel
            {
                Width = _messages.ClientSize.Width - 28,
                Height = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 8),
                Tag = new BubbleMeta
                {
                    StableId = stableId,
                    ServerId = m.Id,
                    TempId = m.TempId,
                    Mine = mine
                }
            };

            var bubble = new Panel
            {
                BackColor = mine ? Color.FromArgb(255, 30, 100) : Color.White,
                Padding = new Padding(10, 8, 10, 8),
                MaximumSize = new Size(520, 0)
            };
            ApplyBubble(bubble);

            Control contentCtrl;
            if (string.Equals(m.Type, "deleted", StringComparison.OrdinalIgnoreCase))
            {
                var text = new Label
                {
                    Text = "Tin nhắn đã thu hồi",
                    AutoSize = true,
                    MaximumSize = new Size(480, 0),
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    ForeColor = Color.Gray
                };
                contentCtrl = text;
                bubble.BackColor = Color.FromArgb(245, 246, 250);
            }
            else if (string.Equals(m.Type, "image", StringComparison.OrdinalIgnoreCase))
            {
                var pb = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Width = 280,
                    Height = 180,
                    BackColor = Color.FromArgb(18, 18, 18)
                };
                contentCtrl = pb;
                _ = Task.Run(async () =>
                {
                    var img = await TryLoadImageAsync(m.Content);
                    if (img != null && !pb.IsDisposed)
                    {
                        try { pb.Invoke(new Action(() => pb.Image = img)); } catch { }
                    }
                });
            }
            else
            {
                var text = new Label
                {
                    Text = m.Content ?? "",
                    AutoSize = true,
                    MaximumSize = new Size(480, 0),
                    Font = new Font("Segoe UI", 10),
                    ForeColor = mine ? Color.White : Color.FromArgb(30, 30, 30)
                };
                contentCtrl = text;
            }

            var time = new Label
            {
                Text = (m.SentAt == default ? DateTime.Now : m.SentAt.ToLocalTime()).ToString("HH:mm"),
                AutoSize = true,
                Font = new Font("Segoe UI", 7),
                ForeColor = mine ? Color.FromArgb(255, 230, 240) : Color.Gray,
                Margin = new Padding(0, 4, 0, 0)
            };

            bubble.Controls.Add(contentCtrl);
            bubble.Controls.Add(time);
            time.Top = contentCtrl.Bottom + 4;
            time.Left = mine ? Math.Max(0, bubble.Width - bubble.Padding.Right - time.PreferredWidth) : Math.Max(0, bubble.Padding.Left);

            bubble.Width = Math.Min(520, Math.Max(70, contentCtrl.PreferredSize.Width + bubble.Padding.Horizontal));
            bubble.Height = time.Bottom + bubble.Padding.Bottom;

            void Layout()
            {
                wrap.Width = _messages.ClientSize.Width - 28;
                var left = mine ? (wrap.Width - bubble.Width) : 0;
                bubble.Left = Math.Max(0, left);
                bubble.Top = 0;
                wrap.Height = bubble.Bottom;
            }

            wrap.Controls.Add(bubble);
            wrap.SizeChanged += (_, __) => Layout();
            _messages.SizeChanged += (_, __) => Layout();
            Layout();

            if (append)
                _messages.Controls.Add(wrap);
            else
                _messages.Controls.Add(wrap);

            if (!string.IsNullOrWhiteSpace(m.TempId))
                _bubbleByTempId[m.TempId] = wrap;
            if (!string.IsNullOrWhiteSpace(m.Id))
                _bubbleByServerId[m.Id] = wrap;

            // right-click menu for my messages
            if (mine)
            {
                void ShowMenu()
                {
                    if (wrap.Tag is BubbleMeta meta)
                    {
                        _ownMessageMenu.Tag = meta;
                        _ownMessageMenu.Show(Cursor.Position);
                    }
                }
                wrap.MouseUp += (_, e) => { if (e.Button == MouseButtons.Right) ShowMenu(); };
                bubble.MouseUp += (_, e) => { if (e.Button == MouseButtons.Right) ShowMenu(); };
                contentCtrl.MouseUp += (_, e) => { if (e.Button == MouseButtons.Right) ShowMenu(); };
            }
        }

        private void ScrollToBottom()
        {
            if (_messages.Controls.Count == 0) return;
            var last = _messages.Controls[_messages.Controls.Count - 1];
            _messages.ScrollControlIntoView(last);
        }

        private static Control MakeHint(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                Margin = new Padding(8),
                Tag = "hint"
            };
        }

        private static void ApplyRound(Control c)
        {
            void Update()
            {
                var d = Math.Min(c.Width, c.Height);
                var rect = new Rectangle(0, 0, d, d);
                using var path = new GraphicsPath();
                path.AddEllipse(rect);
                c.Region = new Region(path);
            }
            c.SizeChanged += (_, __) => Update();
            Update();
        }

        private static void ApplyBubble(Panel p)
        {
            void Update()
            {
                var rect = new Rectangle(0, 0, p.Width, p.Height);
                var r = 14;
                using var path = new GraphicsPath();
                path.AddArc(rect.Left, rect.Top, r * 2, r * 2, 180, 90);
                path.AddArc(rect.Right - r * 2, rect.Top, r * 2, r * 2, 270, 90);
                path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
                path.AddArc(rect.Left, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
                path.CloseFigure();
                p.Region = new Region(path);
            }

            p.SizeChanged += (_, __) => Update();
        }

        private sealed class BubbleMeta
        {
            public string StableId { get; init; }
            public string ServerId { get; set; }
            public string TempId { get; init; }
            public bool Mine { get; init; }
        }

        private sealed class PendingOutgoing
        {
            public string TempId { get; init; }
            public string ServerId { get; set; }
            public string To { get; init; }
            public string Type { get; init; }
            public string Content { get; init; }
            public DateTime CreatedAtUtc { get; init; }
        }

        private DateTime _lastTypingSentAt = DateTime.MinValue;

        private async Task TryLoadHeaderAvatarAsync(string avatarUrl)
        {
            if (_avatarBox == null || _avatarBox.IsDisposed) return;
            if (string.IsNullOrWhiteSpace(avatarUrl)) return;
            var img = await TryLoadImageAsync(avatarUrl);
            if (img != null && !_avatarBox.IsDisposed)
                _avatarBox.Image = img;
        }

        private static async Task<Image> TryLoadImageAsync(string relativeOrFullUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(relativeOrFullUrl)) return null;
                var fullUrl = relativeOrFullUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? relativeOrFullUrl
                    : $"{BaseUrl}{relativeOrFullUrl}";

                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync(fullUrl);
                using var ms = new MemoryStream(bytes);
                return Image.FromStream(ms);
            }
            catch
            {
                return null;
            }
        }
    }
}

