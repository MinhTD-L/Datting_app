using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessLogic;
using DataTransferObject;

namespace Presentation
{
    public sealed class NotificationForm : Form
    {
        private readonly ChatBLL _chatBll;
        private readonly FlowLayoutPanel _list;
        private readonly Label _lblStatus;

        public NotificationForm(ChatBLL chatBll)
        {
            _chatBll = chatBll ?? throw new ArgumentNullException(nameof(chatBll));

            Text = "Thông báo";
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 246, 250);
            MinimumSize = new Size(400, 500);
            Size = new Size(400, 500);
            MaximizeBox = false;
            MinimizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 12)
            };

            var title = new Label
            {
                Text = "Thông báo",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(12, 18)
            };

            _lblStatus = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 24,
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Đang tải..."
            };

            header.Controls.Add(title);
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
            Controls.Add(_lblStatus);
            Controls.Add(header);

            Shown += async (_, __) => await InitializeAsync();
            FormClosed += (_, __) => Unwire();
        }

        private async Task InitializeAsync()
        {
            Wire();
            try
            {
                await _chatBll.EnsureConnectedAsync(SessionManager.Token);
                await _chatBll.LoadNotificationsAsync(0, 20);
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "Lỗi: " + ex.Message;
            }
        }

        private void Wire()
        {
            _chatBll.NotificationsLoaded += OnNotificationsLoaded;
            _chatBll.NotificationReceived += OnNotificationReceived;
        }

        private void Unwire()
        {
            _chatBll.NotificationsLoaded -= OnNotificationsLoaded;
            _chatBll.NotificationReceived -= OnNotificationReceived;
        }

        private void OnNotificationsLoaded(IReadOnlyList<ChatMessageDto> notifications, int page)
        {
            if (IsDisposed) return;
            BeginInvoke(new Action(() =>
            {
                if (page == 0) _list.Controls.Clear();
                _lblStatus.Text = "";

                if (notifications == null || notifications.Count == 0)
                {
                    if (_list.Controls.Count == 0)
                        _list.Controls.Add(MakeHint("Bạn chưa có thông báo nào."));
                    return;
                }

                foreach (var n in notifications)
                {
                    _list.Controls.Add(MakeNotificationCard(n));
                }
            }));
        }

        private void OnNotificationReceived(ChatMessageDto notification)
        {
            if (IsDisposed || notification == null) return;
            BeginInvoke(new Action(() =>
            {
                // Nếu đang hiển thị chữ "Bạn chưa có thông báo nào" thì xoá đi
                if (_list.Controls.Count > 0 && _list.Controls[0] is Label l && string.Equals(l.Tag as string, "hint", StringComparison.Ordinal))
                    _list.Controls.Clear();

                var card = MakeNotificationCard(notification);
                _list.Controls.Add(card);
                _list.Controls.SetChildIndex(card, 0); // Đẩy lên trên cùng
            }));
        }

        private Control MakeNotificationCard(ChatMessageDto n)
        {
            var card = new Panel
            {
                Width = 350,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 16)
            };

            var icon = new Label
            {
                Text = "🔔",
                Font = new Font("Segoe UI Emoji", 14),
                AutoSize = true,
                Location = new Point(12, 12),
                ForeColor = Color.FromArgb(255, 30, 100)
            };

            var lblContent = new Label
            {
                Text = n.Content ?? "Bạn có một thông báo mới",
                AutoSize = true,
                MaximumSize = new Size(280, 0),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(56, 14)
            };

            // Ép WinForms tính toán chiều cao thật của nội dung khi bị giới hạn chiều rộng là 280
            lblContent.Size = lblContent.GetPreferredSize(new Size(280, 0));

            var lblTime = new Label
            {
                Text = (n.SentAt == default ? DateTime.Now : n.SentAt.ToLocalTime()).ToString("dd/MM/yyyy HH:mm"),
                AutoSize = true,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(56, lblContent.Bottom + 6)
            };
            
            // Ép tính toán kích thước của thời gian
            lblTime.Size = lblTime.GetPreferredSize(Size.Empty);

            card.Controls.Add(icon);
            card.Controls.Add(lblContent);
            card.Controls.Add(lblTime);

            card.Height = lblTime.Bottom + 12;

            // Bo góc viền card
            card.Paint += (_, e) =>
            {
                var rect = card.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                using var pen = new Pen(Color.FromArgb(235, 235, 235), 1);
                e.Graphics.DrawRectangle(pen, rect);
                
                var r = 8;
                var path = new GraphicsPath();
                path.AddArc(rect.Left, rect.Top, r * 2, r * 2, 180, 90);
                path.AddArc(rect.Right - r * 2, rect.Top, r * 2, r * 2, 270, 90);
                path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
                path.AddArc(rect.Left, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
                path.CloseFigure();
                card.Region = new Region(path);
            };

            return card;
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
    }
}