using BusinessLogic;
using DataTransferObject;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using Presentation.FormProfile;
using System.Net.Http;

namespace Presentation.FormPost
{
    public class AdminUserListForm : Form
    {
        private readonly AdminBLL _adminBll;
        private FlowLayoutPanel _pnlContent;
        private Button _btnRefresh;
        private List<UserAdminViewDTO> _cachedUsers = new List<UserAdminViewDTO>();
        private int _currentPage = 1;
        private bool _isLoading = false;

        public AdminUserListForm()
        {
            _adminBll = BusinessLogic.AppServices.AdminBll;
            Text = "Quản lý Người dùng (Admin)";
            Size = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 246, 250);

            BuildUI();
            _ = LoadDataAsync();
        }

        private void BuildUI()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            
            var btnBack = new Button { Text = "🔙 Trở về", Location = new Point(20, 15), Width = 100, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(240, 240, 240), ForeColor = Color.Black };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (_, __) => Close();
            btnBack.Cursor = Cursors.Hand;

            _btnRefresh = new Button { Text = "Tải lại", Location = new Point(130, 15), Width = 100, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(255, 30, 100), ForeColor = Color.White };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.Click += async (_, __) => await LoadDataAsync(true);
            _btnRefresh.Cursor = Cursors.Hand;

            var lblTitle = new Label { Text = "Danh sách Người dùng", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(250, 15), AutoSize = true };

            pnlTop.Controls.Add(btnBack);
            pnlTop.Controls.Add(_btnRefresh);
            pnlTop.Controls.Add(lblTitle);

            _pnlContent = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            _pnlContent.SizeChanged += (_, __) => CenterCards();

            Controls.Add(_pnlContent);
            Controls.Add(pnlTop);
        }

        private void CenterCards()
        {
            if (_pnlContent.IsDisposed) return;
            foreach (Control c in _pnlContent.Controls)
            {
                c.Width = _pnlContent.ClientSize.Width - 45; 
            }
        }

        private async Task LoadDataAsync(bool refresh = false)
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                _btnRefresh.Enabled = false;
                if (refresh)
                {
                    _currentPage = 1;
                    _pnlContent.Controls.Clear();
                    _cachedUsers.Clear();
                }

                var res = await _adminBll.GetUsersAsync(page: _currentPage);
                if (res?.Users != null)
                {
                    _cachedUsers.AddRange(res.Users);
                    foreach (var user in res.Users)
                    {
                        var card = CreateUserCard(user);
                        _pnlContent.Controls.Add(card);
                    }
                    CenterCards();
                    _currentPage++;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải danh sách người dùng: " + ex.Message); }
            finally { _btnRefresh.Enabled = true; _isLoading = false; }
        }

        private Panel CreateUserCard(UserAdminViewDTO user)
        {
            var card = new Panel
            {
                Height = 100,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 15),
                Padding = new Padding(15),
                Cursor = Cursors.Hand
            };

            card.Paint += (_, e) =>
            {
                var border = Color.FromArgb(228, 228, 228);
                var rect = card.ClientRectangle;
                rect.Width -= 1; rect.Height -= 1;
                using var pen = new Pen(border, 1);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(pen, rect);
            };

            var picAvatar = new PictureBox { Size = new Size(70, 70), Location = new Point(15, 15), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(238, 238, 238) };
            ApplyRoundedAvatar(picAvatar);
            if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
            {
                _ = LoadAvatarAsync(picAvatar, user.AvatarUrl);
            }

            var lblUsername = new Label { Text = user.Username, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(100, 15), AutoSize = true };
            var lblEmail = new Label { Text = user.Email, Font = new Font("Segoe UI", 9), Location = new Point(100, 45), AutoSize = true, ForeColor = Color.Gray };
            var lblStatus = new Label { Text = $"Status: {user.Status}", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(100, 70), AutoSize = true, ForeColor = GetStatusColor(user.Status) };
            var lblRole = new Label { Text = user.Role.ToUpper(), Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(250, 70), AutoSize = true, ForeColor = user.Role == "admin" ? Color.Crimson : Color.DarkCyan };

            var btnManage = new Button { Text = "Quản lý", Width = 100, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = Color.LightGray, Anchor = AnchorStyles.Right | AnchorStyles.Top };
            btnManage.Location = new Point(card.Width - btnManage.Width - 20, 35);
            btnManage.Click += (_, __) => {
                using var dlg = new ManageUserDialog(user);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadDataAsync(true); // Refresh list on OK
                }
            };

            card.SizeChanged += (_,__) => { btnManage.Location = new Point(card.Width - btnManage.Width - 20, 35); };

            card.Click += (_, __) => { var profileForm = new UserProfile(user.Id, this.FindForm()); profileForm.FormClosed += (s, ev) => { this.FindForm()?.Show(); }; profileForm.Show(); this.FindForm()?.Hide(); };

            card.Controls.Add(picAvatar);
            card.Controls.Add(lblUsername);
            card.Controls.Add(lblEmail);
            card.Controls.Add(lblStatus);
            card.Controls.Add(lblRole);
            card.Controls.Add(btnManage);

            return card;
        }

        private Color GetStatusColor(string status) => status switch { "active" => Color.Green, "inactive" => Color.Gray, "pending" => Color.Orange, "banned" => Color.Red, _ => Color.Black };

        private static async Task LoadAvatarAsync(PictureBox pb, string url)
        {
            try { using var httpClient = new HttpClient(); var fullUrl = url.StartsWith("http") ? url : $"{BusinessLogic.AppConfig.BaseUrl}{url}"; var bytes = await httpClient.GetByteArrayAsync(fullUrl); using var ms = new System.IO.MemoryStream(bytes); var img = Image.FromStream(ms); if (!pb.IsDisposed) pb.Image = img; } catch { /* ignore avatar load errors */ }
        }

        private static void ApplyRoundedAvatar(PictureBox pictureBox)
        {
            void UpdateRegion() { if (pictureBox.IsDisposed) return; var diameter = Math.Min(pictureBox.Width, pictureBox.Height); var rect = new Rectangle(0, 0, diameter, diameter); using var path = new GraphicsPath(); path.AddEllipse(rect); pictureBox.Region = new Region(path); }
            pictureBox.SizeChanged += (_, __) => UpdateRegion();
            UpdateRegion();
        }
    }
}