using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessLogic;
using DataTransferObject;
using Presentation.FormChat;

namespace Presentation.FormFriend
{
    public sealed class FriendsForm : Form
    {
        private readonly FriendBLL _friendBll;
        private readonly ChatBLL _chatBll;

        private readonly TabControl _tabs;
        private readonly TabPage _tabFriends;
        private readonly TabPage _tabRequests;

        private readonly FlowLayoutPanel _friendsList;
        private readonly FlowLayoutPanel _requestsList;

        private readonly Label _lblStatus;

        private const string BaseUrl = "https://litmatchclone-production-944b.up.railway.app";

        public FriendsForm(FriendBLL friendBll)
        {
            _friendBll = friendBll ?? throw new ArgumentNullException(nameof(friendBll));
            _chatBll = BusinessLogic.AppServices.ChatBll;

            Text = "Bạn bè";
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 246, 250);
            MinimumSize = new Size(760, 560);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 12)
            };

            var lblTitle = new Label
            {
                Text = "Bạn bè",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, 0)
            };

            _lblStatus = new Label
            {
                AutoSize = false,
                Height = 18,
                Dock = DockStyle.Bottom,
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            header.Controls.Add(lblTitle);
            header.Controls.Add(_lblStatus);

            var divider = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(235, 235, 235) };
            header.Controls.Add(divider);

            _tabs = new TabControl { Dock = DockStyle.Fill };
            _tabFriends = new TabPage("Danh sách bạn bè");
            _tabRequests = new TabPage("Lời mời kết bạn");

            _friendsList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(245, 246, 250)
            };

            _requestsList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(245, 246, 250)
            };

            _tabFriends.Controls.Add(_friendsList);
            _tabRequests.Controls.Add(_requestsList);

            _tabs.TabPages.Add(_tabFriends);
            _tabs.TabPages.Add(_tabRequests);

            Controls.Add(_tabs);
            Controls.Add(header);

            Shown += async (_, __) => await ReloadAllAsync();
        }

        private async Task ReloadAllAsync()
        {
            await LoadFriendsAsync();
            await LoadRequestsAsync();
        }

        private async Task LoadFriendsAsync()
        {
            _friendsList.Controls.Clear();
            _friendsList.Controls.Add(MakeHint("Đang tải danh sách bạn bè..."));

            try
            {
                var res = await _friendBll.GetFriendsAsync();
                _friendsList.Controls.Clear();

                if (res?.Friends == null || res.Friends.Count == 0)
                {
                    _friendsList.Controls.Add(MakeHint("Chưa có bạn bè nào."));
                    return;
                }

                foreach (var f in res.Friends)
                    _friendsList.Controls.Add(MakeFriendCard(f));
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(this, "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", "Hết phiên",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
            }
            catch (Exception ex)
            {
                _friendsList.Controls.Clear();
                _friendsList.Controls.Add(MakeHint("Không thể tải: " + ex.Message));
            }
        }

        private async Task LoadRequestsAsync()
        {
            _requestsList.Controls.Clear();
            _requestsList.Controls.Add(MakeHint("Đang tải lời mời..."));

            try
            {
                var res = await _friendBll.GetRequestsAsync();
                _requestsList.Controls.Clear();

                if (res?.Requests == null || res.Requests.Count == 0)
                {
                    _requestsList.Controls.Add(MakeHint("Không có lời mời nào."));
                    return;
                }

                foreach (var r in res.Requests)
                    _requestsList.Controls.Add(MakeRequestCard(r));
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(this, "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", "Hết phiên",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
            }
            catch (Exception ex)
            {
                _requestsList.Controls.Clear();
                _requestsList.Controls.Add(MakeHint("Không thể tải: " + ex.Message));
            }
        }

        private Control MakeFriendCard(FriendDto f)
        {
            var card = new Panel
            {
                Width = 680,
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

            if (!string.IsNullOrWhiteSpace(f?.AvatarUrl))
            {
                _ = Task.Run(async () =>
                {
                    var img = await TryLoadImageAsync(f.AvatarUrl);
                    if (img != null && !avatar.IsDisposed)
                    {
                        try { avatar.Invoke(new Action(() => avatar.Image = img)); } catch { }
                    }
                });
            }

            var lblName = new Label
            {
                Text = f?.Username ?? "Unknown",
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(avatar.Right + 12, 26),
                Cursor = Cursors.Hand
            };

            var btnMsg = new Button
            {
                Text = "💬 Nhắn tin",
                Width = 110,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 30, 100),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnMsg.FlatAppearance.BorderSize = 0;

            void LayoutRight()
            {
                btnMsg.Left = card.ClientSize.Width - card.Padding.Right - btnMsg.Width;
                btnMsg.Top = (card.ClientSize.Height - btnMsg.Height) / 2;
            }

            card.SizeChanged += (_, __) => LayoutRight();

            btnMsg.Click += async (_, __) =>
            {
                var withId = f?.UserId;
                if (string.IsNullOrWhiteSpace(withId)) return;

                try
                {
                    await _chatBll.EnsureConnectedAsync(SessionManager.Token);
                    using var wnd = new ChatWindow(_chatBll, withId, f?.Username, f?.AvatarUrl);
                    wnd.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Không thể mở chat: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            card.Controls.Add(avatar);
            card.Controls.Add(lblName);
            card.Controls.Add(btnMsg);
            LayoutRight();
            card.Paint += (_, e) =>
            {
                var rect = card.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                using var pen = new Pen(Color.FromArgb(235, 235, 235), 1);
                e.Graphics.DrawRectangle(pen, rect);
            };

            void OpenProfile()
            {
                if (string.IsNullOrWhiteSpace(f?.UserId)) return;
                var profileForm = new Presentation.FormProfile.UserProfile(f.UserId, this);
                profileForm.Show();
                this.Hide();
            }

            avatar.Click += (_, __) => OpenProfile();
            lblName.Click += (_, __) => OpenProfile();
            card.Click += (_, __) => btnMsg.PerformClick();

            return card;
        }

        private Control MakeRequestCard(FriendRequestDto r)
        {
            var card = new Panel
            {
                Width = 680,
                Height = 80,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(12)
            };

            var lbl = new Label
            {
                Text = $"Từ: {r?.RequesterId}\nLúc: {r.CreatedAt.ToLocalTime():dd/MM/yyyy HH:mm}",
                AutoSize = false,
                Width = 440,
                Height = 44,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(12, 12)
            };

            var btnAccept = new Button
            {
                Text = "Chấp nhận",
                Width = 100,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(24, 119, 242),
                ForeColor = Color.White,
                Left = card.Width - 220,
                Top = 24
            };
            btnAccept.FlatAppearance.BorderSize = 0;

            var btnReject = new Button
            {
                Text = "Từ chối",
                Width = 90,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(245, 246, 250),
                ForeColor = Color.FromArgb(140, 30, 30),
                Left = card.Width - 110,
                Top = 24
            };
            btnReject.FlatAppearance.BorderColor = Color.FromArgb(230, 230, 230);

            btnAccept.Click += async (_, __) =>
            {
                if (string.IsNullOrWhiteSpace(r?.Id)) return;
                SetBusy(true, "Đang chấp nhận...");
                try
                {
                    await _friendBll.AcceptRequestAsync(r.Id);
                    SetBusy(false, "Đã chấp nhận.");
                    await ReloadAllAsync();
                }
                catch (Exception ex)
                {
                    SetBusy(false, "Thất bại: " + ex.Message);
                    MessageBox.Show(this, "Không thể chấp nhận: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnReject.Click += async (_, __) =>
            {
                if (string.IsNullOrWhiteSpace(r?.Id)) return;
                SetBusy(true, "Đang từ chối...");
                try
                {
                    await _friendBll.RejectRequestAsync(r.Id);
                    SetBusy(false, "Đã từ chối.");
                    await LoadRequestsAsync();
                }
                catch (Exception ex)
                {
                    SetBusy(false, "Thất bại: " + ex.Message);
                    MessageBox.Show(this, "Không thể từ chối: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            card.Controls.Add(lbl);
            card.Controls.Add(btnAccept);
            card.Controls.Add(btnReject);

            card.SizeChanged += (_, __) =>
            {
                lbl.Width = Math.Max(240, card.ClientSize.Width - 260);
                btnAccept.Left = card.ClientSize.Width - btnReject.Width - btnAccept.Width - 20;
                btnReject.Left = card.ClientSize.Width - btnReject.Width - 12;
            };

            card.Paint += (_, e) =>
            {
                var rect = card.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                using var pen = new Pen(Color.FromArgb(235, 235, 235), 1);
                e.Graphics.DrawRectangle(pen, rect);
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
                Margin = new Padding(8)
            };
        }

        private void SetBusy(bool busy, string status)
        {
            _lblStatus.Text = status ?? "";
            UseWaitCursor = busy;
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

        private static async Task<Image> TryLoadImageAsync(string relativeOrFullUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(relativeOrFullUrl)) return null;
                var fullUrl = relativeOrFullUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? relativeOrFullUrl
                    : $"{BaseUrl}{relativeOrFullUrl}";

                using var http = new System.Net.Http.HttpClient();
                var bytes = await http.GetByteArrayAsync(fullUrl);
                using var ms = new System.IO.MemoryStream(bytes);
                return Image.FromStream(ms);
            }
            catch
            {
                return null;
            }
        }
    }
}