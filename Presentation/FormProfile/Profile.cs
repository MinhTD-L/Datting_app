using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System;
using System.Linq;
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
        private readonly FriendBLL _friendBll;
        private readonly ChatBLL _chatBll;
        private readonly Form _backTo;
        private System.Collections.Generic.HashSet<string> _friendIds = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
        private System.Collections.Generic.HashSet<string> _sentFriendRequests = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);

        private UserProfileDTO _currentProfile;

        // New UI elements
        private Button btnCreatePost;
        private Label lblFriendCount;
        private FlowLayoutPanel pnlLeftColumn;
        private FlowLayoutPanel pnlRightColumn;
        private Panel pnlFriends;
        private FlowLayoutPanel flpFriendsGrid;
        private Label lblFullName;
        private Label lblDob;
        private FlowLayoutPanel flpTags;

        private const int FeedMaxWidth = 680;

        public Profile()
        {
            InitializeComponent();
            _userBll = BusinessLogic.AppServices.UserBll;
            _postBll = BusinessLogic.AppServices.PostBll;
            _friendBll = BusinessLogic.AppServices.FriendBll;
            _chatBll = BusinessLogic.AppServices.ChatBll;
            _backTo = null;

            ApplyModernStyles();

            btnBack.Click += (_, __) => BackToDashboard();
            // btnCreatePost click is wired in ApplyModernStyles
            btnEdit.Click += async (_, __) => await OpenEditProfileAsync();
            Shown += async (_, __) => await LoadProfileAsync();
        }

        public Profile(Form backTo) : this()
        {
            _backTo = backTo;
        }

        private async Task OpenCreatePostAsync()
        {
            // Assuming you have a UserBLL instance available or can get it from AppServices
            using var createForm = new CreatePostForm(_postBll, BusinessLogic.AppServices.UserBll);
            if (createForm.ShowDialog(this) == DialogResult.OK)
            {
                await LoadProfileAsync(); 
            }
        }

        private async Task LoadProfileAsync()
        {
            btnEdit.Enabled = false;
            if (flpMyPosts != null)
            {
                flpMyPosts.Controls.Clear();
                flpMyPosts.Controls.Add(new Label { Text = "Đang tải bài đăng...", AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 10, FontStyle.Italic), Margin = new Padding(8) });
            }

            try
            {
                await LoadFriendIdsAsync();
                var profile = await _userBll.GetMyProfileAsync();
                BindProfile(profile);

                var friendResult = await _friendBll.GetFriendsAsync();
                RenderFriends(friendResult?.Friends);
                if (lblFriendCount != null)
                    lblFriendCount.Text = $"{(friendResult?.Friends?.Count ?? 0)} người bạn";
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

        private async Task LoadFriendIdsAsync()
        {
            try
            {
                var res = await _friendBll.GetFriendsAsync();
                var set = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
                if (res?.Friends != null)
                {
                    foreach (var f in res.Friends)
                    {
                        if (!string.IsNullOrWhiteSpace(f?.UserId))
                            set.Add(f.UserId);
                    }
                }
                _friendIds = set;
            }
            catch
            {
                _friendIds = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
            }
        }
        private void BindProfile(UserProfileDTO profile)
        {
            _currentProfile = profile;
            var displayName = (profile?.UserName ?? SessionManager.Username ?? "Bạn").Trim();
            if (string.IsNullOrWhiteSpace(displayName)) displayName = "Bạn";

            // Header
            lblNameAge.Text = displayName;
            // lblFriendCount is updated in LoadProfileAsync

            // About Card
            if (lblFullName != null) lblFullName.Text = $"Tên thật: {profile?.FullName ?? "Chưa có"}";
            if (lblDob != null) lblDob.Text = profile?.DateOfBirth.HasValue == true ? $"Sinh nhật: {profile.DateOfBirth.Value.ToString("dd/MM/yyyy")}" : "Sinh nhật: Chưa rõ";
            if (lblBio != null) lblBio.Text = string.IsNullOrWhiteSpace(profile?.Bio) ? "Chưa có giới thiệu." : profile.Bio.Trim();

            _ = LoadAvatarAsync(profile?.AvatarUrl);

            if (flpTags != null)
            {
                flpTags.Controls.Clear();
                if (profile?.Tags != null && profile.Tags.Count > 0)
                {
                    foreach (var tag in profile.Tags)
                    {
                        var lblTag = new Label
                        {
                            Text = $"#{tag.Trim()}",
                            AutoSize = true,
                            Font = new Font("Segoe UI", 9, FontStyle.Regular),
                            ForeColor = Color.FromArgb(24, 119, 242),
                            BackColor = Color.FromArgb(230, 242, 255),
                            Padding = new Padding(4),
                            Margin = new Padding(0, 0, 5, 5)
                        };
                        flpTags.Controls.Add(lblTag);
                    }
                }
            }
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

        private void RenderFriends(List<FriendDto> friends)
        {
            if (flpFriendsGrid == null) return;

            flpFriendsGrid.SuspendLayout();
            flpFriendsGrid.Controls.Clear();

            if (friends == null || friends.Count == 0)
            {
                var lblNoFriends = new Label
                {
                    Text = "Chưa có bạn bè nào.",
                    AutoSize = true,
                    ForeColor = Color.Gray,
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    Margin = new Padding(8)
                };
                flpFriendsGrid.Controls.Add(lblNoFriends);
                flpFriendsGrid.ResumeLayout();
                pnlFriends.Height = flpFriendsGrid.Bottom + 12;
                return;
            }

            // Render friend avatars
            foreach (var friend in friends.Take(9)) // Limit to 9 friends for display
            {
                var friendPanel = new Panel
                {
                    Width = 60,
                    Height = 80,
                    Margin = new Padding(4),
                    BackColor = Color.Transparent
                };

                var pbFriendAvatar = new PictureBox
                {
                    Size = new Size(50, 50),
                    Location = new Point(5, 0),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.FromArgb(238, 238, 238),
                    Cursor = Cursors.Hand
                };
                ApplyRounded(pbFriendAvatar);

                var lblFriendName = new Label
                {
                    Text = friend.Username,
                    Font = new Font("Segoe UI", 8, FontStyle.Regular),
                    AutoSize = false,
                    TextAlign = ContentAlignment.TopCenter,
                    Width = 60,
                    Height = 20,
                    Location = new Point(0, pbFriendAvatar.Bottom + 4),
                    ForeColor = Color.FromArgb(50, 50, 50),
                    AutoEllipsis = true // Add ellipsis for long names
                };

                friendPanel.Controls.Add(pbFriendAvatar);
                friendPanel.Controls.Add(lblFriendName);
                flpFriendsGrid.Controls.Add(friendPanel);

                // Load avatar asynchronously
                if (!string.IsNullOrWhiteSpace(friend.AvatarUrl))
                {
                    _ = LoadThumbAsync(pbFriendAvatar, friend.AvatarUrl);
                }

                // TODO: Add click event to open friend's profile
            }

            flpFriendsGrid.ResumeLayout();
            pnlFriends.Height = flpFriendsGrid.Bottom + 12;
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
            var maxWidth = Math.Max(360, pnlRightColumn.ClientSize.Width - 50);
            var left = 10;

            var card = new Panel
            {
                Width = maxWidth,
                Height = 200,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(left, 10, 10, 10),
                Padding = new Padding(12),
            };
            card.Cursor = Cursors.Hand;

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
                Text = FormatTime(post.CreatedAt),
                AutoSize = true,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(picAvatar.Right + 10, 34)
            };

            var btnMore = new Button
            {
                Text = "⋮",
                AutoSize = true,
                Height = 26,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.Gray,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Tag = "no_open_detail"
            };
            btnMore.FlatAppearance.BorderSize = 0;

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

            btnMore.Click += (s, e) => {
                menu.Show(btnMore, new Point(0, btnMore.Height));
            };

            var btnAddFriend = new Button
            {
                Text = "Kết bạn",
                AutoSize = true,
                Height = 26,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 30, 100),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Visible = false,
                Tag = "no_open_detail"
            };
            btnAddFriend.FlatAppearance.BorderSize = 0;

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
            var contentHeight = MeasureLabelHeight(lblContent.Text, lblContent.Font, lblContent.Width, maxLines: 6);
            lblContent.Height = Math.Max(22, contentHeight);

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
                Padding = Padding.Empty
            };
            actionBar.Tag = "no_open_detail";
            actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            actionBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            
            var btnLike = CreateActionButton("👍 Thích");
            var btnComment = CreateActionButton("💬 Bình luận");
            btnLike.Dock = DockStyle.Fill;
            btnComment.Dock = DockStyle.Fill;

            var isLiked = post?.IsLiked ?? false;
            post.IsLiked = isLiked;

            void UpdateLikeUi()
            {
                btnLike.Text = post.IsLiked ? "👍 Đã thích" : "👍 Thích";
                btnLike.ForeColor = post.IsLiked ? Color.FromArgb(24, 119, 242) : Color.FromArgb(70, 70, 70);
                lblStats.Text = $"❤️ {post?.LikeCount ?? 0}    💬 {post?.CommentCount ?? 0}";
            }

            UpdateLikeUi();

            btnLike.Click += async (_, __) =>
            {
                if (string.IsNullOrWhiteSpace(post.Id))
                    return;

                btnLike.Enabled = false;

                var prevLiked = post.IsLiked;
                var prevCount = post.LikeCount;
                post.IsLiked = !post.IsLiked;
                post.LikeCount = Math.Max(0, post.LikeCount + (post.IsLiked ? 1 : -1));
                UpdateLikeUi();

                try
                {
                    if (post.IsLiked)
                    {
                        await _postBll.LikeAsync(post.Id);
                    }
                    else
                    {
                        await _postBll.UnlikeAsync(post.Id);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    post.IsLiked = prevLiked;
                    post.LikeCount = prevCount;
                    UpdateLikeUi();
                    MessageBox.Show("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
                }
                catch (Exception ex)
                {
                    post.IsLiked = prevLiked;
                    post.LikeCount = prevCount;
                    UpdateLikeUi();
                    MessageBox.Show("Không thể thực hiện: " + ex.Message);
                }
                finally
                {
                    btnLike.Enabled = true;
                }
            };

            actionBar.Controls.Add(btnLike, 0, 0);
            actionBar.Controls.Add(btnComment, 1, 0);

            card.Controls.Add(picAvatar);
            card.Controls.Add(lblUsername);
            card.Controls.Add(lblTime);
            card.Controls.Add(btnMore);
            card.Controls.Add(btnAddFriend);
            card.Controls.Add(lblContent);
            card.Controls.Add(mediaHost);
            card.Controls.Add(lblStats);
            card.Controls.Add(divider);
            card.Controls.Add(actionBar);

            card.Paint += (_, e) =>
            {
                var border = Color.FromArgb(228, 228, 228);
                var rect = card.ClientRectangle;
                rect.Width -= 1; rect.Height -= 1;
                using var pen = new Pen(border, 1);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(pen, rect);
            };

            void LayoutHeaderRow()
            {
                btnMore.Left = card.ClientSize.Width - card.Padding.Right - btnMore.Width;
                btnMore.Top = 12;

                btnAddFriend.Left = lblUsername.Right + 10;
                btnAddFriend.Top = lblUsername.Top - 1;

                var maxLeft = btnMore.Left - btnAddFriend.Width - 10;
                if (btnAddFriend.Left > maxLeft)
                    btnAddFriend.Left = maxLeft;
            }

            card.SizeChanged += (_, __) =>
            {
                lblContent.Width = card.Width - 24;
                divider.Width = card.Width - 24;
                actionBar.Width = card.Width - 24;
                mediaHost.Width = card.Width - 24;
                LayoutHeaderRow();
            };

            ApplyRounded(picAvatar);

            card.Cursor = Cursors.Hand;
            WireClickToChildren(card, async () => await OpenPostDetailAsync(post.Id));
            btnComment.Click += async (_, __) => await OpenPostDetailAsync(post.Id, openComments: true);

            // assets (avatar + media)
            _ = LoadCardAssetsAsync();

            var authorId = post?.User?.UserID;
            var meId = SessionManager.UserId;
            var canShowAddFriend =
                !string.IsNullOrWhiteSpace(authorId) &&
                !string.IsNullOrWhiteSpace(meId) &&
                !string.Equals(authorId, meId, StringComparison.Ordinal) &&
                !_friendIds.Contains(authorId) &&
                !_sentFriendRequests.Contains(authorId);

            btnAddFriend.Visible = canShowAddFriend;
            LayoutHeaderRow();

            btnAddFriend.Click += async (_, __) =>
            {
                if (string.IsNullOrWhiteSpace(authorId)) return;
                btnAddFriend.Enabled = false;
                btnAddFriend.Text = "Đang gửi...";
                try { await _friendBll.SendRequestAsync(authorId); _sentFriendRequests.Add(authorId); btnAddFriend.Text = "Đã gửi"; }
                catch (Exception ex) { btnAddFriend.Enabled = true; btnAddFriend.Text = "Kết bạn"; MessageBox.Show("Không thể gửi lời mời: " + ex.Message); }
            };


            async Task LoadCardAssetsAsync()
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
                            await BuildMediaGridAsync(mediaHost, images);

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

            lblStats.Top = (mediaHost.Visible ? mediaHost.Bottom : lblContent.Bottom) + 10;
            divider.Top = lblStats.Bottom + 8;
            actionBar.Top = divider.Bottom + 6;
            card.Height = actionBar.Bottom + 10;

            actionBar.BringToFront();

            return card;
        }

        private static int MeasureLabelHeight(string text, Font font, int width, int maxLines)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            var proposed = new Size(width, int.MaxValue);
            var flags = TextFormatFlags.WordBreak;
            var size = TextRenderer.MeasureText(text, font, proposed, flags);

            var lineHeight = TextRenderer.MeasureText("A", font).Height;
            var maxHeight = lineHeight * Math.Max(1, maxLines);
            return Math.Min(size.Height, maxHeight);
        }

        private async Task BuildMediaGridAsync(Panel host, System.Collections.Generic.List<PostMedia> images)
        {
            host.Controls.Clear();
            host.Visible = true;

            var count = images.Count;
            var gap = 3;
            var maxShow = Math.Min(4, count);

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = new Padding(gap),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            if (count == 1)
            {
                grid.RowCount = 1;
                grid.ColumnCount = 1;
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                host.Height = Math.Min(420, (host.Width * 9) / 16);
            }
            else if (count == 2)
            {
                grid.RowCount = 1;
                grid.ColumnCount = 2;
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                host.Height = Math.Min(420, (host.Width * 3) / 4);
            }
            else if (count == 3)
            {
                grid.RowCount = 2;
                grid.ColumnCount = 2;
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
                host.Height = Math.Min(420, host.Width);
            }
            else
            {
                grid.RowCount = 2;
                grid.ColumnCount = 2;
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                host.Height = Math.Min(420, host.Width);
            }

            host.Controls.Add(grid);

            PictureBox CreateMediaBox()
            {
                return new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Black,
                    Margin = Padding.Empty
                };
            }

            Control WrapCell(Control inner)
            {
                var cell = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent,
                    Margin = Padding.Empty,
                    Padding = new Padding(gap / 2)
                };
                inner.Dock = DockStyle.Fill;
                cell.Controls.Add(inner);
                return cell;
            }

            async Task<Image> TryLoad(string url) => await LoadImageFromUrl(url);

            if (count == 1)
            {
                var pb = CreateMediaBox();
                grid.Controls.Add(WrapCell(pb), 0, 0);
                pb.Image = await TryLoad(images[0].Url);
                return;
            }

            if (count == 2)
            {
                for (var i = 0; i < 2; i++)
                {
                    var pb = CreateMediaBox();
                    grid.Controls.Add(WrapCell(pb), i, 0);
                    pb.Image = await TryLoad(images[i].Url);
                }
                return;
            }

            if (count == 3)
            {
                var pb0 = CreateMediaBox();
                var c0 = WrapCell(pb0);
                grid.Controls.Add(c0, 0, 0);
                grid.SetRowSpan(c0, 2);

                var pb1 = CreateMediaBox();
                grid.Controls.Add(WrapCell(pb1), 1, 0);

                var pb2 = CreateMediaBox();
                grid.Controls.Add(WrapCell(pb2), 1, 1);

                pb0.Image = await TryLoad(images[0].Url);
                pb1.Image = await TryLoad(images[1].Url);
                pb2.Image = await TryLoad(images[2].Url);
                return;
            }

            var cells = new (int col, int row)[] { (0, 0), (1, 0), (0, 1), (1, 1) };
            for (var i = 0; i < maxShow; i++)
            {
                var pb = CreateMediaBox();
                var cell = WrapCell(pb);
                grid.Controls.Add(cell, cells[i].col, cells[i].row);
                pb.Image = await TryLoad(images[i].Url);

                if (i == 3 && count > 4)
                {
                    var overlay = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(120, 0, 0, 0), Margin = Padding.Empty };
                    var lbl = new Label { Text = $"+{count - 4}", ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 16, FontStyle.Bold), BackColor = Color.Transparent };
                    overlay.Controls.Add(lbl);
                    cell.Controls.Add(overlay);
                    overlay.BringToFront();
                }
            }
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

                var fullUrl = relativeOrFullUrl.StartsWith("http", System.StringComparison.OrdinalIgnoreCase) ?
                    relativeOrFullUrl :
                    $"{BusinessLogic.AppConfig.BaseUrl}{relativeOrFullUrl}";

                var bytes = await httpClient.GetByteArrayAsync(fullUrl);
                using var ms = new MemoryStream(bytes);
                return Image.FromStream(ms);
            }
            catch
            {
                return null;
            }
        }

        private async Task OpenPostDetailAsync(string postId, bool openComments = false)
        {
            if (string.IsNullOrWhiteSpace(postId))
                return;

            using var detail = new PostDetailForm(_postBll, postId, openComments);
            detail.ShowDialog(this);
        }

        private static Button CreateActionButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = false,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(70, 70, 70),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(232, 232, 232);
            return btn;
        }

        private string FormatTime(DateTime createdAt)
        {
            var localTime = createdAt.ToLocalTime();
            var span = DateTime.Now - localTime;

            if (span.TotalMinutes < 1) return "Vừa xong";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} phút trước";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} giờ trước";

            return localTime.ToString("dd/MM/yyyy HH:mm");
        }

        private void CenterColumns()
        {
            var tlpColumns = Controls.OfType<TableLayoutPanel>().FirstOrDefault(c => c.Name == "tlpColumns");
            if (tlpColumns == null) return;
            var totalWidth = this.ClientSize.Width;
            var contentWidth = (int)(totalWidth * 0.95);
            var paddingSize = Math.Max(0, (totalWidth - contentWidth) / 2);
            tlpColumns.Padding = new Padding(paddingSize, 24, paddingSize, 12);
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
            // 1. FORM & MAIN LAYOUT
            DoubleBuffered = true;
            BackColor = Color.FromArgb(240, 242, 245); // Light gray background
            pnlHeader.SendToBack();

            // Replace flpContent with a two-column layout
            if (Controls.Contains(flpContent))
            {
                Controls.Remove(flpContent);
                flpContent.Dispose();
            }

            var tlpColumns = new TableLayoutPanel
            {
                Name = "tlpColumns",
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 2,
                RowCount = 1,
            };
            tlpColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            tlpColumns.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); 
            Controls.Add(tlpColumns);
            tlpColumns.BringToFront();
            this.SizeChanged += (_, __) => CenterColumns();
            CenterColumns();

            pnlLeftColumn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 0, 12, 0) };
            pnlRightColumn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(12, 0, 0, 0) };

            tlpColumns.Controls.Add(pnlLeftColumn, 0, 0);
            tlpColumns.Controls.Add(pnlRightColumn, 1, 0);

            // 2. HEADER
            pnlHeader.BackColor = Color.White;
            pnlHeader.Height = 150;
            pnlHeader.Paint += (s, e) => { using var pen = new Pen(Color.FromArgb(220, 220, 220), 1); e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1); };

            // Show back button
            btnBack.Visible = true;
            btnBack.Text = "← Quay lại";
            btnBack.Width = 100;
            btnBack.Height = 36;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.BackColor = Color.FromArgb(240, 242, 245);
            btnBack.ForeColor = Color.FromArgb(30, 30, 30);
            btnBack.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnBack.Cursor = Cursors.Hand;

            // Avatar
            pbAvatar.Size = new Size(100, 100);
            pbAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            pbAvatar.BackColor = Color.FromArgb(235, 235, 235);
            ApplyRounded(pbAvatar);

            // Name & Stats
            lblNameAge.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblNameAge.Text = "Username"; // Placeholder

            lblFriendCount = new Label { AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Regular), ForeColor = Color.Gray, Text = "0 người bạn" };
            pnlHeader.Controls.Add(lblFriendCount);

            // Buttons
            btnEdit.Text = "Chỉnh sửa hồ sơ";
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            btnEdit.BackColor = Color.FromArgb(245, 245, 245);
            btnEdit.ForeColor = Color.FromArgb(50, 50, 50);
            btnEdit.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            btnCreatePost = new Button { Text = "Thêm bài đăng", FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(236, 72, 153), ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold), };
            btnCreatePost.FlatAppearance.BorderSize = 0;
            pnlHeader.Controls.Add(btnCreatePost);
            btnCreatePost.Click += async (_, __) => await OpenCreatePostAsync();

            // 3. LEFT COLUMN
            // About Card
            pnlIntro.BackColor = Color.White;
            pnlIntro.Margin = new Padding(0, 0, 0, 16);
            StyleCardPanel(pnlIntro);
            pnlIntro.Controls.Clear();

            var lblAboutHead = new Label { Text = "Giới thiệu", Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = true, Location = new Point(12, 12) };
            lblFullName = new Label { Text = "Tên thật: ...", Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(12, 40) };
            lblDob = new Label { Text = "Sinh nhật: ...", Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(12, 60) };

            flpTags = new FlowLayoutPanel
            {
                Location = new Point(12, 85),
                Width = pnlIntro.Width - 24,
                AutoSize = true,
                WrapContents = true,
                BackColor = Color.Transparent
            };

            lblBio.Text = "Chưa có giới thiệu.";
            lblBio.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblBio.ForeColor = Color.FromArgb(80, 80, 80);
            lblBio.Location = new Point(12, 110);
            lblBio.MaximumSize = new Size(pnlIntro.Width - 24, 0);
            lblBio.AutoSize = true;

            pnlIntro.Controls.Add(lblAboutHead);
            pnlIntro.Controls.Add(lblFullName);
            pnlIntro.Controls.Add(lblDob);
            pnlIntro.Controls.Add(flpTags);
            pnlIntro.Controls.Add(lblBio);

            void LayoutIntro()
            {
                lblBio.MaximumSize = new Size(pnlIntro.Width - 24, 0);
                flpTags.MaximumSize = new Size(pnlIntro.Width - 24, 0);
                int currentY = lblDob.Bottom + 10;
                if (flpTags.Controls.Count > 0) { flpTags.Visible = true; flpTags.Top = currentY; currentY = flpTags.Bottom + 10; }
                else { flpTags.Visible = false; }
                lblBio.Top = currentY;
                pnlIntro.Height = lblBio.Bottom + 12;
            }

            pnlIntro.SizeChanged += (_, __) => LayoutIntro();
            flpTags.SizeChanged += (_, __) => LayoutIntro();
            lblBio.SizeChanged += (_, __) => LayoutIntro();

            pnlLeftColumn.Controls.Add(pnlIntro);

            // Friends Card
            pnlFriends = new Panel { BackColor = Color.White, Margin = new Padding(0, 0, 0, 16) };
            StyleCardPanel(pnlFriends);
            var lblFriendsHead = new Label { Text = "Bạn bè", Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = true, Location = new Point(12, 12) };
            flpFriendsGrid = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Location = new Point(12, 40), BackColor = Color.Transparent, WrapContents = true };
            pnlFriends.Controls.Add(lblFriendsHead);
            pnlFriends.Controls.Add(flpFriendsGrid);
            pnlFriends.SizeChanged += (_, __) => { flpFriendsGrid.Width = pnlFriends.ClientSize.Width - 24; pnlFriends.Height = flpFriendsGrid.Bottom + 12; };
            pnlLeftColumn.Controls.Add(pnlFriends);

            // 4. RIGHT COLUMN
            var pnlPostsHeader = new Panel { BackColor = Color.White, Margin = new Padding(0, 0, 0, 16), Height = 50 };
            StyleCardPanel(pnlPostsHeader);
            lblMyPostHead.Text = "Bài viết của bạn";
            lblMyPostHead.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblMyPostHead.Location = new Point(12, 12);
            pnlPostsHeader.Controls.Add(lblMyPostHead);
            pnlRightColumn.Controls.Add(pnlPostsHeader);

            flpMyPosts.BackColor = Color.Transparent;
            flpMyPosts.Margin = new Padding(0);
            flpMyPosts.AutoScroll = false;
            flpMyPosts.AutoSize = true;
            flpMyPosts.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlRightColumn.Controls.Add(flpMyPosts);

            // Layout Resizing
            pnlLeftColumn.SizeChanged += (_, __) => { foreach (Control c in pnlLeftColumn.Controls) c.Width = pnlLeftColumn.ClientSize.Width - 12; };
            pnlRightColumn.SizeChanged += (_, __) => { 
                pnlPostsHeader.Width = pnlRightColumn.ClientSize.Width - 24; 
                flpMyPosts.Width = pnlRightColumn.ClientSize.Width - 24; 
                
                var maxWidth = Math.Max(360, pnlRightColumn.ClientSize.Width - 50);
                foreach (Control c in flpMyPosts.Controls)
                {
                    if (c is Panel p) p.Width = maxWidth;
                }
            };

            // Header Layout
            void LayoutHeader()
            {
                pnlHeader.Height = 200;
                int padding = 24;
                btnBack.Location = new Point(padding, 16);
                pbAvatar.Location = new Point(padding, btnBack.Bottom + 16);

                var nameLocation = new Point(pbAvatar.Right + 16, pbAvatar.Top + 8);
                lblNameAge.Location = nameLocation;
                lblFriendCount.Location = new Point(nameLocation.X, lblNameAge.Bottom + 4);

                btnCreatePost.Size = new Size(140, 36);
                btnCreatePost.Location = new Point(nameLocation.X, lblFriendCount.Bottom + 12);

                btnEdit.Size = new Size(160, 38);
                btnEdit.Location = new Point(pnlHeader.ClientSize.Width - btnEdit.Width - padding, pbAvatar.Top);
            }

            pnlHeader.SizeChanged += (_, __) => LayoutHeader();
            LayoutHeader();
        }

        private static void StyleCardPanel(Panel p)
        {
            p.BackColor = Color.White;
            p.Padding = new Padding(12);
            p.Paint += (_, e) =>
            {
                var rect = p.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                using var pen = new Pen(Color.FromArgb(228, 228, 228), 1);
                e.Graphics.DrawRectangle(pen, rect);
            };
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

        private static void WireClickToChildren(Control root, Func<Task> onClickAsync)
        {
            if (root == null || onClickAsync == null) return;

            async void Handler(object sender, EventArgs e)
            {
                try { await onClickAsync(); }
                catch { }
            }

            static bool IsBlocked(Control c)
            {
                for (Control cur = c; cur != null; cur = cur.Parent)
                {
                    if (cur.Tag is string s && string.Equals(s, "no_open_detail", StringComparison.Ordinal))
                        return true;
                }
                return false;
            }

            void Attach(Control c)
            {
                if (IsBlocked(c)) return;

                c.Click += Handler;
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
