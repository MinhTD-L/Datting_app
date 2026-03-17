using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessLogic;
using DataTransferObject;

namespace Presentation.FormProfile
{
    public partial class Profile : Form
    {
        private readonly UserBLL _userBll;
        private readonly PostBLL _postBll;
        private readonly Form _backTo;
        private UserProfileDTO _currentProfile;

        private const string BaseUrl = "https://litmatchclone-production.up.railway.app";

        public Profile()
        {
            InitializeComponent();
            _userBll = BusinessLogic.AppServices.UserBll;
            _postBll = BusinessLogic.AppServices.PostBll;
            _backTo = null;

            ApplyModernStyles();

            btnBack.Click += (_, __) => BackToDashboard();
            btnEdit.Click += async (_, __) => await OpenEditProfileAsync();
            Shown += async (_, __) => await LoadProfileAsync();
        }

        public Profile(Form backTo) : this()
        {
            _backTo = backTo;
        }

        private async Task LoadProfileAsync()
        {
            btnEdit.Enabled = false;
            flpMyPosts.Controls.Clear();
            flpMyPosts.Controls.Add(new Label
            {
                Text = "Đang tải bài đăng...",
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                Margin = new Padding(8)
            });

            try
            {
                var profile = await _userBll.GetMyProfileAsync();
                BindProfile(profile);

                var myPosts = await _postBll.GetMyPostsAsync(limit: 20, page: 1);
                RenderMyPosts(myPosts?.Posts);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(this, "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", "Hết phiên",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, "Không thể tải trang cá nhân: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                RenderMyPosts(null);
            }
            finally
            {
                btnEdit.Enabled = true;
            }
        }

        private void BindProfile(UserProfileDTO profile)
        {
            _currentProfile = profile;
            var displayName = (profile?.FullName ?? profile?.UserName ?? SessionManager.Username ?? "Bạn").Trim();
            if (string.IsNullOrWhiteSpace(displayName)) displayName = "Bạn";

            lblNameAge.Text = displayName;

            var username = (profile?.UserName ?? SessionManager.Username ?? "").Trim();
            lblUsername.Text = string.IsNullOrWhiteSpace(username) ? "" : "@" + username;

            // currently no email field in API
            lblEmail.Text = "";
            lblEmail.Visible = false;
            lblEmailHead.Visible = false;

            lblBio.Text = string.IsNullOrWhiteSpace(profile?.Bio) ? "Chưa có giới thiệu." : profile.Bio.Trim();

            lblGender.Text = string.IsNullOrWhiteSpace(profile?.Gender) ? "Chưa rõ" : profile.Gender.Trim();

            var age = TryGetAge(profile?.DateOfBirth);
            lblAge.Text = age.HasValue ? age.Value.ToString() : "—";

            _ = LoadAvatarAsync(profile?.AvatarUrl);
        }

        private async Task LoadAvatarAsync(string avatarUrl)
        {
            if (string.IsNullOrWhiteSpace(avatarUrl)) return;
            try
            {
                var img = await LoadImageFromUrl(avatarUrl);
                if (img != null && !pbAvatar.IsDisposed)
                    pbAvatar.Image = img;
            }
            catch
            {
            }
        }

        private void RenderMyPosts(System.Collections.Generic.List<PostFeedDTO> posts)
        {
            flpMyPosts.SuspendLayout();
            flpMyPosts.Controls.Clear();

            if (posts == null || posts.Count == 0)
            {
                flpMyPosts.Controls.Add(new Label
                {
                    Text = "Bạn chưa có bài đăng nào.",
                    AutoSize = true,
                    ForeColor = Color.Gray,
                    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                    Margin = new Padding(8)
                });
                flpMyPosts.ResumeLayout();
                return;
            }

            foreach (var p in posts)
            {
                var card = BuildPostCard(p);
                flpMyPosts.Controls.Add(card);
            }

            flpMyPosts.ResumeLayout();
        }

        private Panel BuildPostCard(PostFeedDTO post)
        {
            var maxWidth = Math.Min(680, Math.Max(360, flpMyPosts.ClientSize.Width - 40));
            var left = Math.Max(6, (flpMyPosts.ClientSize.Width - maxWidth) / 2);

            var card = new Panel
            {
                Width = maxWidth,
                BackColor = Color.White,
                Padding = new Padding(12),
                Margin = new Padding(left, 8, 6, 8),
                Height = 200
            };

            card.Paint += (_, e) =>
            {
                var rect = card.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                using var pen = new Pen(Color.FromArgb(235, 235, 235), 1);
                e.Graphics.DrawRectangle(pen, rect);
            };

            var picAvatar = new PictureBox
            {
                Size = new Size(44, 44),
                Location = new Point(12, 12),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(238, 238, 238)
            };

            var lblUsername = new Label
            {
                Text = post?.User?.Username ?? SessionManager.Username ?? "Bạn",
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(picAvatar.Right + 10, 12)
            };

            var lblTime = new Label
            {
                Text = post?.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "",
                AutoSize = true,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(picAvatar.Right + 10, 34)
            };

            var btnMenu = new Button
            {
                Text = "⋯",
                Width = 34,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(70, 70, 70),
                Location = new Point(card.Width - 12 - 34, 12),
                Cursor = Cursors.Hand,
                Tag = "no_open_detail"
            };
            btnMenu.FlatAppearance.BorderSize = 0;
            btnMenu.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
            btnMenu.FlatAppearance.MouseDownBackColor = Color.FromArgb(232, 232, 232);

            var menu = new ContextMenuStrip();
            var miEdit = new ToolStripMenuItem("Chỉnh sửa");
            var miDelete = new ToolStripMenuItem("Xoá");
            menu.Items.Add(miEdit);
            menu.Items.Add(miDelete);

            miEdit.Click += (_, __) =>
            {
                MessageBox.Show(this, "Chưa có API cập nhật bài viết (edit). Khi BE có endpoint update, mình sẽ nối luôn.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            miDelete.Click += async (_, __) => await DeleteMyPostAsync(post?.Id);

            btnMenu.Click += (_, __) => menu.Show(btnMenu, new Point(0, btnMenu.Height));

            var lblContent = new Label
            {
                Text = post?.Content ?? "",
                Font = new Font("Segoe UI", 10),
                AutoSize = false,
                Location = new Point(12, picAvatar.Bottom + 10),
                Width = card.Width - 24,
                Height = 1,
                ForeColor = Color.FromArgb(30, 30, 30)
            };

            var mediaHost = new Panel
            {
                Location = new Point(12, lblContent.Bottom + 10),
                Width = card.Width - 24,
                Height = 0,
                BackColor = Color.FromArgb(240, 242, 245),
                Visible = false
            };

            var lblStats = new Label
            {
                Text = $"❤️ {post?.LikeCount ?? 0}    💬 {post?.CommentCount ?? 0}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(12, 0)
            };

            var divider = new Panel
            {
                Height = 1,
                Width = card.Width - 24,
                BackColor = Color.FromArgb(235, 235, 235),
                Location = new Point(12, 0)
            };

            var actionBar = new TableLayoutPanel
            {
                Width = card.Width - 24,
                Height = 36,
                Location = new Point(12, 0),
                BackColor = Color.Transparent,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Tag = "no_open_detail"
            };
            actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            actionBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var btnLike = new Button
            {
                Text = "👍 Thích",
                AutoSize = false,
                Height = 34,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(70, 70, 70),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLike.FlatAppearance.BorderSize = 0;
            btnLike.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
            btnLike.FlatAppearance.MouseDownBackColor = Color.FromArgb(232, 232, 232);

            var btnComment = new Button
            {
                Text = "💬 Bình luận",
                AutoSize = false,
                Height = 34,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(70, 70, 70),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnComment.FlatAppearance.BorderSize = 0;
            btnComment.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
            btnComment.FlatAppearance.MouseDownBackColor = Color.FromArgb(232, 232, 232);

            actionBar.Controls.Add(btnLike, 0, 0);
            actionBar.Controls.Add(btnComment, 1, 0);

            card.Controls.Add(picAvatar);
            card.Controls.Add(lblUsername);
            card.Controls.Add(lblTime);
            card.Controls.Add(btnMenu);
            card.Controls.Add(lblContent);
            card.Controls.Add(mediaHost);
            card.Controls.Add(lblStats);
            card.Controls.Add(divider);
            card.Controls.Add(actionBar);

            card.SizeChanged += (_, __) =>
            {
                lblContent.Width = card.Width - 24;
                divider.Width = card.Width - 24;
                actionBar.Width = card.Width - 24;
                mediaHost.Width = card.Width - 24;
                btnMenu.Left = card.Width - 12 - btnMenu.Width;
            };

            ApplyRounded(picAvatar);

            card.Cursor = Cursors.Hand;
            WireClickToChildren(card, () =>
            {
                if (string.IsNullOrWhiteSpace(post?.Id))
                    return;

                using var detail = new Presentation.PostDetailForm(_postBll, post.Id);
                detail.ShowDialog(this);
            });

            btnComment.Click += (_, __) =>
            {
                if (string.IsNullOrWhiteSpace(post?.Id)) return;
                using var detail = new Presentation.PostDetailForm(_postBll, post.Id, openComments: true);
                detail.ShowDialog(this);
            };

            // assets (avatar + media)
            _ = LoadAssetsAsync();

            async Task LoadAssetsAsync()
            {
                try
                {
                    if (post?.User != null && !string.IsNullOrWhiteSpace(post.User.AvatarURL))
                    {
                        var avatar = await LoadImageFromUrl(post.User.AvatarURL);
                        if (avatar != null && !picAvatar.IsDisposed)
                            picAvatar.Image = avatar;
                    }

                    if (post?.Media != null && post.Media.Count > 0)
                    {
                        var images = post.Media.FindAll(x =>
                            !string.IsNullOrWhiteSpace(x?.Url) &&
                            string.Equals(x.Type, "image", System.StringComparison.OrdinalIgnoreCase));

                        if (images.Count > 0 && !mediaHost.IsDisposed)
                        {
                            mediaHost.Visible = true;
                            mediaHost.Controls.Clear();
                            mediaHost.Height = Math.Min(360, (mediaHost.Width * 9) / 16);
                            var pb = new PictureBox
                            {
                                Dock = DockStyle.Fill,
                                SizeMode = PictureBoxSizeMode.Zoom,
                                BackColor = Color.Black
                            };
                            mediaHost.Controls.Add(pb);
                            pb.Image = await LoadImageFromUrl(images[0].Url);

                            lblStats.Top = mediaHost.Bottom + 10;
                            divider.Top = lblStats.Bottom + 8;
                            actionBar.Top = divider.Bottom + 6;
                            card.Height = actionBar.Bottom + 10;
                        }
                    }
                }
                catch
                {
                }
            }

            return card;
        }

        private static async Task LoadThumbAsync(PictureBox pb, string url)
        {
            try
            {
                var img = await LoadImageFromUrl(url);
                if (img != null && !pb.IsDisposed)
                    pb.Image = img;
            }
            catch
            {
            }
        }

        private static int? TryGetAge(System.DateTime? dob)
        {
            if (!dob.HasValue) return null;
            var d = dob.Value.Date;
            var today = System.DateTime.Today;
            var age = today.Year - d.Year;
            if (d > today.AddYears(-age)) age--;
            return age < 0 ? null : age;
        }

        private static async Task<Image> LoadImageFromUrl(string relativeOrFullUrl)
        {
            try
            {
                using var httpClient = new HttpClient();

                var fullUrl = relativeOrFullUrl.StartsWith("http", System.StringComparison.OrdinalIgnoreCase)
                    ? relativeOrFullUrl
                    : $"{BaseUrl}{relativeOrFullUrl}";

                var bytes = await httpClient.GetByteArrayAsync(fullUrl);
                using var ms = new MemoryStream(bytes);
                return Image.FromStream(ms);
            }
            catch
            {
                return null;
            }
        }

        private void BackToDashboard()
        {
            try
            {
                if (_backTo != null && !_backTo.IsDisposed)
                {
                    _backTo.Show();
                    Close();
                    return;
                }

                var dashboard = new Presentation.MainDashboard();
                dashboard.Show();
                Close();
            }
            catch
            {
                Close();
            }
        }

        private void ApplyModernStyles()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(245, 246, 250);
            flpContent.BackColor = BackColor;

            // Header / banner
            pnlHeader.BackColor = Color.White;
            pnlPinkBanner.Height = 92;
            pnlPinkBanner.BackColor = Color.FromArgb(243, 66, 140);

            // Move back button into banner, make it minimal
            try
            {
                pnlHeader.Controls.Remove(btnBack);
                pnlPinkBanner.Controls.Add(btnBack);
            }
            catch { }

            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.BackColor = Color.Transparent;
            btnBack.ForeColor = Color.White;
            btnBack.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnBack.Size = new Size(110, 30);
            btnBack.Location = new Point(14, 14);

            pbAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            pbAvatar.BackColor = Color.FromArgb(235, 235, 235);

            lblNameAge.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            lblUsername.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblUsername.ForeColor = Color.Gray;

            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.FlatAppearance.BorderColor = Color.FromArgb(230, 230, 230);
            btnEdit.BackColor = Color.White;
            btnEdit.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            // Cards
            StyleCardPanel(pnlIntro);
            StyleCardPanel(panel2);

            lblBioHead.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblBioHead.ForeColor = Color.FromArgb(30, 30, 30);
            lblBio.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblBio.ForeColor = Color.FromArgb(60, 60, 60);
            lblBio.AutoEllipsis = true;

            // Stats row: keep but make cleaner
            tlpStats.BackColor = Color.White;
            tlpStats.Padding = new Padding(8);
            tlpStats.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            StyleStatCell(pnlGender, lblGenderHead, lblGender);
            StyleStatCell(pnlCare, lblCareHead, label2);
            StyleStatCell(pnlConnect, lblConnectHead, label3);

            label2.Text = "—";
            label3.Text = "—";

            // Posts list
            flpMyPosts.BackColor = Color.Transparent;
            flpMyPosts.Padding = new Padding(2);
            lblMyPostHead.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // Reposition elements on resize
            void LayoutHeader()
            {
                // avatar overlaps banner + white area
                pbAvatar.Size = new Size(110, 110);
                pbAvatar.Location = new Point(22, pnlPinkBanner.Bottom - (pbAvatar.Height / 2));
                ApplyRounded(pbAvatar);

                lblNameAge.Location = new Point(22, pbAvatar.Bottom + 10);
                lblUsername.Location = new Point(24, lblNameAge.Bottom + 2);

                btnEdit.Size = new Size(160, 36);
                btnEdit.Location = new Point(pnlHeader.ClientSize.Width - btnEdit.Width - 22, pbAvatar.Bottom + 12);

                // Age line to the right of name (simple)
                lblAgeHead.Visible = true;
                lblAge.Visible = true;
                lblAgeHead.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                lblAge.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                lblAgeHead.ForeColor = Color.Gray;
                lblAge.ForeColor = Color.FromArgb(30, 30, 30);
                lblAgeHead.Location = new Point(24, lblUsername.Bottom + 6);
                lblAge.Location = new Point(lblAgeHead.Right + 4, lblAgeHead.Top);

                pnlHeader.Height = Math.Max(220, lblAgeHead.Bottom + 14);
            }

            pnlHeader.SizeChanged += (_, __) => LayoutHeader();
            LayoutHeader();
        }

        private static void StyleCardPanel(Panel p)
        {
            p.BackColor = Color.White;
            p.Padding = new Padding(14);
            p.Paint += (_, e) =>
            {
                var rect = p.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                using var pen = new Pen(Color.FromArgb(235, 235, 235), 1);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(pen, rect);
            };
        }

        private static void StyleStatCell(Panel host, Label head, Label value)
        {
            host.BackColor = Color.Transparent;
            head.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            head.ForeColor = Color.Gray;
            value.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            value.ForeColor = Color.FromArgb(30, 30, 30);
        }

        private static void ApplyRounded(PictureBox pictureBox)
        {
            void UpdateRegion()
            {
                var diameter = System.Math.Min(pictureBox.Width, pictureBox.Height);
                var rect = new Rectangle(0, 0, diameter, diameter);
                using var path = new GraphicsPath();
                path.AddEllipse(rect);
                pictureBox.Region = new Region(path);
            }

            pictureBox.SizeChanged += (_, __) => UpdateRegion();
            UpdateRegion();
        }

        private static void WireClickToChildren(Control root, System.Action onClick)
        {
            if (root == null || onClick == null) return;

            static bool IsBlocked(Control c)
            {
                for (Control cur = c; cur != null; cur = cur.Parent)
                {
                    if (cur.Tag is string s && string.Equals(s, "no_open_detail", System.StringComparison.Ordinal))
                        return true;
                }
                return false;
            }

            void Attach(Control c)
            {
                if (IsBlocked(c))
                    return;
                c.Click += (_, __) => onClick();
                foreach (Control child in c.Controls)
                    Attach(child);
            }

            Attach(root);
        }

        private async Task DeleteMyPostAsync(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
                return;

            var confirm = MessageBox.Show(
                this,
                "Bạn chắc chắn muốn xoá bài đăng này?",
                "Xoá bài đăng",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                btnEdit.Enabled = false;
                await _postBll.DeletePostAsync(postId);
                await LoadProfileAsync();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(this, "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", "Hết phiên",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, "Không thể xoá bài đăng: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnEdit.Enabled = true;
            }
        }

        private async Task OpenEditProfileAsync()
        {
            try
            {
                btnEdit.Enabled = false;
                var seed = _currentProfile ?? await _userBll.GetMyProfileAsync();
                using var dlg = new EditProfile(_userBll, seed);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    await LoadProfileAsync();
            }
            finally
            {
                btnEdit.Enabled = true;
            }
        }
    }
}
