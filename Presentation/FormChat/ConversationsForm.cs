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
    public sealed class ConversationsForm : Form
    {
        private readonly ChatBLL _chatBll;

        private readonly FlowLayoutPanel _list;
        private readonly Label _status;

        private List<ContactDto> _contacts = new();
        private readonly Dictionary<string, Control> _rowByUserId = new(StringComparer.Ordinal);
        private string _activeChatUserId = null;

        private const string BaseUrl = "https://litmatchclone-production.up.railway.app";

        public ConversationsForm(ChatBLL chatBll)
        {
            _chatBll = chatBll ?? throw new ArgumentNullException(nameof(chatBll));

            Text = "Tin nhắn";
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 246, 250);
            MinimumSize = new Size(820, 560);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 12)
            };

            var title = new Label
            {
                Text = "Tin nhắn",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, 0)
            };

            _status = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 18,
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            header.Controls.Add(title);
            header.Controls.Add(_status);
            header.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(235, 235, 235) });

            _list = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(245, 246, 250)
            };

            Controls.Add(_list);
            Controls.Add(header);

            Shown += async (_, __) => await InitializeAsync();
            FormClosed += (_, __) => Unwire();
        }

        private async Task InitializeAsync()
        {
            Wire();
            SetStatus("Đang kết nối...");

            try
            {
                await _chatBll.EnsureConnectedAsync(SessionManager.Token);
                SetStatus("Đang tải cuộc trò chuyện...");
                await _chatBll.LoadContactsAsync();
            }
            catch (Exception ex)
            {
                SetStatus("Không thể kết nối.");
                MessageBox.Show(this, "Không thể mở tin nhắn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void Wire()
        {
            _chatBll.Connected += OnConnected;
            _chatBll.Disconnected += OnDisconnected;
            _chatBll.Error += OnError;
            _chatBll.ContactsUpdated += OnContactsUpdated;
            _chatBll.MessageReceived += OnMessageReceived;
            _chatBll.UserOnlineChanged += OnUserOnlineChanged;
            _chatBll.SeenReceived += _ => { };
            _chatBll.TypingReceived += _ => { };
        }

        private void Unwire()
        {
            _chatBll.Connected -= OnConnected;
            _chatBll.Disconnected -= OnDisconnected;
            _chatBll.Error -= OnError;
            _chatBll.ContactsUpdated -= OnContactsUpdated;
            _chatBll.MessageReceived -= OnMessageReceived;
            _chatBll.UserOnlineChanged -= OnUserOnlineChanged;
        }

        private void OnConnected()
        {
            if (IsDisposed) return;
            BeginInvoke(new Action(() => SetStatus("Đã kết nối.")));
        }

        private void OnDisconnected()
        {
            if (IsDisposed) return;
            BeginInvoke(new Action(() => SetStatus("Mất kết nối. Đang tự kết nối lại...")));
        }

        private void OnError(string obj)
        {
            if (IsDisposed) return;
            BeginInvoke(new Action(() =>
            {
                if (!string.IsNullOrWhiteSpace(obj))
                    SetStatus(obj);
            }));
        }

        private void OnContactsUpdated(IReadOnlyList<ContactDto> contacts)
        {
            if (IsDisposed) return;
            BeginInvoke(new Action(() =>
            {
                _contacts = (contacts ?? Array.Empty<ContactDto>()).Where(c => !string.IsNullOrWhiteSpace(c?.UserID)).ToList();
                Render();
                SetStatus(_contacts.Count == 0 ? "Chưa có cuộc trò chuyện." : $"Có {_contacts.Count} cuộc trò chuyện.");
            }));
        }

        private void OnMessageReceived(ChatMessageDto msg)
        {
            if (IsDisposed) return;
            if (msg == null) return;

            BeginInvoke(new Action(() =>
            {
                // update the matching contact row quickly
                var other = GetOtherUserId(msg);
                if (string.IsNullOrWhiteSpace(other)) return;

                var c = _contacts.FirstOrDefault(x => string.Equals(x.UserID, other, StringComparison.Ordinal));
                if (c != null)
                {
                    if (string.Equals(msg.Type, "deleted", StringComparison.OrdinalIgnoreCase))
                        c.LastMessage = "Tin nhắn đã thu hồi";
                    else if (string.Equals(msg.Type, "image", StringComparison.OrdinalIgnoreCase))
                        c.LastMessage = "[Hình ảnh]";
                    else if (string.Equals(msg.Type, "file", StringComparison.OrdinalIgnoreCase))
                        c.LastMessage = "[Tệp đính kèm]";
                    else if (string.Equals(msg.Type, "call", StringComparison.OrdinalIgnoreCase))
                        c.LastMessage = "[Cuộc gọi]";
                    else
                        c.LastMessage = msg.Content;

                    c.LastTime = msg.SentAt;
                    c.LastSenderID = msg.From;
                    if (!string.Equals(msg.From, SessionManager.UserId, StringComparison.Ordinal))
                    {
                        if (!string.Equals(other, _activeChatUserId, StringComparison.Ordinal) && 
                            !string.Equals(msg.Type, "deleted", StringComparison.OrdinalIgnoreCase))
                        {
                            c.UnreadCount += 1;
                        }
                    }
                }
                else
                {
                    _ = _chatBll.LoadContactsAsync();
                    return;
                }

                _contacts = _contacts
                    .OrderByDescending(x => x.LastTime == default ? DateTime.MinValue : x.LastTime)
                    .ToList();

                Render();
            }));
        }

        private void OnUserOnlineChanged(string userId, bool online)
        {
            if (IsDisposed) return;
            if (string.IsNullOrWhiteSpace(userId)) return;

            BeginInvoke(new Action(() =>
            {
                var c = _contacts.FirstOrDefault(x => string.Equals(x.UserID, userId, StringComparison.Ordinal));
                if (c == null) return;
                c.Status = online ? "online" : "offline";
                UpdateRow(userId);
            }));
        }

        private void Render()
        {
            _list.SuspendLayout();
            try
            {
                _list.Controls.Clear();
                _rowByUserId.Clear();

                if (_contacts.Count == 0)
                {
                    _list.Controls.Add(MakeHint("Chưa có cuộc trò chuyện nào."));
                    return;
                }

                foreach (var c in _contacts)
                {
                    var row = MakeRow(c);
                    _rowByUserId[c.UserID] = row;
                    _list.Controls.Add(row);
                }
            }
            finally
            {
                _list.ResumeLayout();
            }
        }

        private void UpdateRow(string userId)
        {
            if (!_rowByUserId.TryGetValue(userId, out var row)) return;
            var c = _contacts.FirstOrDefault(x => string.Equals(x.UserID, userId, StringComparison.Ordinal));
            if (c == null) return;

            // labels were stored in Tag
            if (row.Tag is not RowRefs refs) return;
            refs.Name.Text = c.Username ?? "Unknown";
            refs.Preview.Text = BuildPreview(c);
            refs.Unread.Visible = c.UnreadCount > 0;
            refs.Unread.Text = c.UnreadCount > 99 ? "99+" : c.UnreadCount.ToString();
            refs.OnlineDot.Visible = string.Equals(c.Status, "online", StringComparison.OrdinalIgnoreCase);
            
            refs.Preview.Width = Math.Max(240, refs.Unread.Visible ? refs.Unread.Left - 16 - refs.Preview.Left : row.ClientSize.Width - row.Padding.Right - refs.Preview.Left);
        }

        private Control MakeRow(ContactDto c)
        {
            var card = new Panel
            {
                Width = 740,
                Height = 76,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(12),
                Cursor = Cursors.Hand
            };

            var avatar = new PictureBox
            {
                Size = new Size(48, 48),
                Location = new Point(12, 14),
                BackColor = Color.FromArgb(238, 238, 238),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            ApplyRound(avatar);

            if (!string.IsNullOrWhiteSpace(c.Avatar))
            {
                _ = Task.Run(async () =>
                {
                    var img = await TryLoadImageAsync(c.Avatar);
                    if (img != null && !avatar.IsDisposed)
                    {
                        try { avatar.Invoke(new Action(() => avatar.Image = img)); } catch { }
                    }
                });
            }

            var onlineDot = new Panel
            {
                Size = new Size(10, 10),
                BackColor = Color.FromArgb(30, 200, 90),
                Left = avatar.Right - 10,
                Top = avatar.Bottom - 10,
                Visible = string.Equals(c.Status, "online", StringComparison.OrdinalIgnoreCase)
            };
            ApplyRound(onlineDot);

            var lblName = new Label
            {
                Text = c.Username ?? "Unknown",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(avatar.Right + 12, 14)
            };

            var lblPreview = new Label
            {
                Text = BuildPreview(c),
                AutoSize = false,
                Width = 520,
                Height = 20,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(90, 90, 90),
                Location = new Point(avatar.Right + 12, 38)
            };

            var unread = new Label
            {
                AutoSize = false,
                Width = 44,
                Height = 22,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(255, 30, 100),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Visible = c.UnreadCount > 0,
                Text = c.UnreadCount > 99 ? "99+" : c.UnreadCount.ToString()
            };
            ApplyPill(unread);

            void LayoutRight()
            {
                unread.Left = card.ClientSize.Width - card.Padding.Right - unread.Width;
                unread.Top = (card.ClientSize.Height - unread.Height) / 2;
                lblPreview.Width = Math.Max(240, unread.Left - 16 - lblPreview.Left);
            }
            card.SizeChanged += (_, __) => LayoutRight();
            LayoutRight();

            card.Controls.Add(avatar);
            card.Controls.Add(onlineDot);
            card.Controls.Add(lblName);
            card.Controls.Add(lblPreview);
            card.Controls.Add(unread);

            card.Tag = new RowRefs { Name = lblName, Preview = lblPreview, Unread = unread, OnlineDot = onlineDot };

            card.Paint += (_, e) =>
            {
                var rect = card.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                using var pen = new Pen(Color.FromArgb(235, 235, 235), 1);
                e.Graphics.DrawRectangle(pen, rect);
            };

            async void OpenChat()
            {
                try
                {
                    c.UnreadCount = 0;
                    UpdateRow(c.UserID);
                    _activeChatUserId = c.UserID;
                    
                    _ = _chatBll.MarkSeenAsync(c.UserID);

                    using var wnd = new ChatWindow(_chatBll, c.UserID, c.Username, c.Avatar);
                    wnd.ShowDialog(this);
                    _activeChatUserId = null;
                    await _chatBll.LoadContactsAsync();
                }
                catch
                {
                    _activeChatUserId = null;
                }
            }

            foreach (Control child in card.Controls)
                child.Click += (_, __) => OpenChat();
            card.Click += (_, __) => OpenChat();

            return card;
        }

        private static string BuildPreview(ContactDto c)
        {
            if (c == null) return "";
            var msg = (c.LastMessage ?? "").Trim();
            if (msg.Length > 60) msg = msg[..60] + "…";
            if (string.IsNullOrWhiteSpace(msg)) msg = "Nhấn để bắt đầu chat";
            return msg;
        }

        private static string GetOtherUserId(ChatMessageDto m)
        {
            var me = SessionManager.UserId;
            if (string.IsNullOrWhiteSpace(me) || m == null) return "";
            if (string.Equals(m.From, me, StringComparison.Ordinal)) return m.To ?? "";
            return m.From ?? "";
        }

        private static Control MakeHint(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                Margin = new Padding(8)
            };
        }

        private void SetStatus(string s)
        {
            _status.Text = s ?? "";
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

        private static void ApplyPill(Control c)
        {
            void Update()
            {
                var rect = new Rectangle(0, 0, c.Width, c.Height);
                var r = Math.Min(c.Height / 2, 12);
                using var path = new GraphicsPath();
                path.AddArc(rect.Left, rect.Top, r * 2, r * 2, 180, 90);
                path.AddArc(rect.Right - r * 2, rect.Top, r * 2, r * 2, 270, 90);
                path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
                path.AddArc(rect.Left, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
                path.CloseFigure();
                c.Region = new Region(path);
            }
            c.SizeChanged += (_, __) => Update();
            Update();
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

        private sealed class RowRefs
        {
            public Label Name { get; init; }
            public Label Preview { get; init; }
            public Label Unread { get; init; }
            public Panel OnlineDot { get; init; }
        }
    }
}
