using BusinessLogic;
using DataTransferObject;
using Presentation.FormPost.Components;
using Presentation.FormFriend;
using Presentation.FormProfile;
using Presentation.FormChat;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Presentation
{
    public partial class MainDashboard : Form
    {
        private readonly PostBLL _postBll;
        private readonly UserBLL _userBll;
        private readonly FriendBLL _friendBll;
        private readonly ChatBLL _chatBll;
        private System.Collections.Generic.HashSet<string> _friendIds = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
        private System.Collections.Generic.HashSet<string> _sentFriendRequests = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);

        private const string BaseUrl = "https://litmatchclone-production.up.railway.app";
        private Panel _feedLoadingOverlay;
        private Panel _messagesDot;
        private bool _hasUnreadMessages;
        private const int FeedMaxWidth = 680;

        public MainDashboard()
        {
            _postBll = BusinessLogic.AppServices.PostBll;
            _userBll = BusinessLogic.AppServices.UserBll;
            _friendBll = BusinessLogic.AppServices.FriendBll;
            _chatBll = BusinessLogic.AppServices.ChatBll;
            InitializeComponent();
            InitUi();
        }

        public MainDashboard(PostBLL postBll, UserBLL userBll)
        {
            _postBll = postBll ?? throw new ArgumentNullException(nameof(postBll));
            _userBll = userBll ?? throw new ArgumentNullException(nameof(userBll));
            _friendBll = BusinessLogic.AppServices.FriendBll;
            _chatBll = BusinessLogic.AppServices.ChatBll;
            InitializeComponent();
            InitUi();
        }

        private void InitUi()
        {
            // Tăng tốc độ render, giảm flicker cho Form
            this.DoubleBuffered = true;
            
            // Tăng tốc độ render, giảm flicker cho FlowLayoutPanel (PostFeed) và các panel chứa nội dung
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, PostFeed, new object[] { true });
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, pnlContent, new object[] { true });

            PostFeed.AutoScroll = true;
            PostFeed.FlowDirection = FlowDirection.TopDown;
            PostFeed.WrapContents = false;
            PostFeed.BackColor = Color.FromArgb(245, 246, 250);

            PostFeed.SizeChanged += (_, __) => CenterFeedCards();

            BuildFeedLoadingOverlay();
            this.Load += Dashboard_Load;

            btnCreatePost.Click += async (_, __) =>
            {
                using var dlg = new CreatePostForm(_postBll, _userBll);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    await LoadPosts();
            };

            btnFriend.Click += (_, __) =>
            {
                using var dlg = new FriendsForm(_friendBll);
                dlg.ShowDialog(this);
            };

            BuildMessagesDot();
            WireChatBadge();

            btnMessages.Click += (_, __) =>
            {
                ClearMessagesDot();
                using var dlg = new ConversationsForm(_chatBll);
                dlg.ShowDialog(this);
            };
        }

        private void BuildMessagesDot()
        {
            // small red dot on Messages button
            _messagesDot = new Panel
            {
                Size = new Size(10, 10),
                BackColor = Color.FromArgb(255, 30, 100),
                Visible = false
            };

            void ApplyRound(Control c)
            {
                var rect = new Rectangle(0, 0, c.Width, c.Height);
                using var path = new GraphicsPath();
                path.AddEllipse(rect);
                c.Region = new Region(path);
            }

            _messagesDot.SizeChanged += (_, __) => ApplyRound(_messagesDot);
            ApplyRound(_messagesDot);

            btnMessages.Controls.Add(_messagesDot);
            _messagesDot.BringToFront();

            void Reposition()
            {
                _messagesDot.Left = btnMessages.Width - _messagesDot.Width - 14;
                _messagesDot.Top = 10;
            }
            btnMessages.SizeChanged += (_, __) => Reposition();
            Reposition();
        }

        private void WireChatBadge()
        {
            _chatBll.MessageReceived -= OnChatMessageForBadge;
            _chatBll.MessageReceived += OnChatMessageForBadge;
            FormClosed += (_, __) => _chatBll.MessageReceived -= OnChatMessageForBadge;
        }

        private void OnChatMessageForBadge(ChatMessageDto m)
        {
            try
            {
                if (m == null) return;
                if (string.Equals(m.From, SessionManager.UserId, StringComparison.Ordinal))
                    return; // don't badge on my own echo

                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => SetMessagesDot(true)));
                    return;
                }
                SetMessagesDot(true);
            }
            catch
            {
            }
        }

        private void SetMessagesDot(bool show)
        {
            _hasUnreadMessages = show;
            if (_messagesDot != null && !_messagesDot.IsDisposed)
                _messagesDot.Visible = show;
        }

        private void ClearMessagesDot()
        {
            SetMessagesDot(false);
        }

        private void BuildFeedLoadingOverlay()
        {
            _feedLoadingOverlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 246, 250),
                Visible = false
            };

            var lbl = new Label
            {
                Text = "Đang tải bảng tin...",
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.Gray,
            };
            var bar = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 25,
                Width = 260,
                Height = 18
            };

            _feedLoadingOverlay.Controls.Add(lbl);
            _feedLoadingOverlay.Controls.Add(bar);

            void Reposition()
            {
                lbl.Left = (pnlContent.ClientSize.Width - lbl.Width) / 2;
                lbl.Top = 140;
                bar.Left = (pnlContent.ClientSize.Width - bar.Width) / 2;
                bar.Top = lbl.Bottom + 14;
            }

            pnlContent.SizeChanged += (_, __) => Reposition();
            _feedLoadingOverlay.VisibleChanged += (_, __) => Reposition();
            pnlContent.Controls.Add(_feedLoadingOverlay);
            _feedLoadingOverlay.BringToFront();
        }

        private void SetFeedLoading(bool isLoading)
        {
            if (_feedLoadingOverlay == null) return;
            _feedLoadingOverlay.Visible = isLoading;
            _feedLoadingOverlay.BringToFront();
        }

        private async void Dashboard_Load(object sender, EventArgs e)
        {
            await LoadFriendIdsAsync();
            await LoadPosts();
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

        private async Task LoadPosts()
        {
            try
            {
                PostFeed.Controls.Clear();
                SetFeedLoading(true);

                var result = await _postBll.GetFeedAsync();

                if (result == null || result.Posts == null || result.Posts.Count == 0)
                {
                    var lblEmpty = new Label
                    {
                        Text = "Chưa có bài viết nào",
                        AutoSize = true,
                        Font = new Font("Segoe UI", 12, FontStyle.Italic),
                        ForeColor = Color.Gray,
                        Margin = new Padding(20)
                    };

                    PostFeed.Controls.Add(lblEmpty);
                    return;
                }

                foreach (var post in result.Posts)
                {
                    var card = await CreatePostCard(post);
                    PostFeed.Controls.Add(card);
                }

                CenterFeedCards();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải bài viết: " + ex.Message);
            }
            finally
            {
                SetFeedLoading(false);
            }
        }

        private async Task<Panel> CreatePostCard(PostFeedDTO post)
        {
            var maxWidth = Math.Min(FeedMaxWidth, Math.Max(360, PostFeed.ClientSize.Width - 40));
            var left = Math.Max(10, (PostFeed.ClientSize.Width - maxWidth) / 2);

            var card = new Panel
            {
                Width = maxWidth,
                Height = 200,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(left, 10, 10, 10),
                Padding = new Padding(12)
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
                Text = post.User?.Username ?? "Unknown",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(picAvatar.Right + 10, 12)
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

            var lblTime = new Label
            {
                Text = FormatTime(post.CreatedAt),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(picAvatar.Right + 10, 34)
            };

            var lblContent = new Label
            {
                Text = post.Content ?? "",
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
                Text = $"❤️ {post.LikeCount}    💬 {post.CommentCount}",
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
            if (!isLiked)
                isLiked = post?.GetIsLikedFallback() ?? false;

            post.IsLiked = isLiked;

            void UpdateLikeUi()
            {
                btnLike.Text = post.IsLiked ? "👍 Đã thích" : "👍 Thích";
                btnLike.ForeColor = post.IsLiked ? Color.FromArgb(24, 119, 242) : Color.FromArgb(70, 70, 70);
                lblStats.Text = $"❤️ {post.LikeCount}    💬 {post.CommentCount}";
            }

            UpdateLikeUi();

            card.Controls.Add(picAvatar);
            card.Controls.Add(lblUsername);
            card.Controls.Add(btnAddFriend);
            card.Controls.Add(lblTime);
            card.Controls.Add(lblContent);
            card.Controls.Add(mediaHost);
            card.Controls.Add(lblStats);
            card.Controls.Add(divider);
            card.Controls.Add(actionBar);

            void LayoutHeaderRow()
            {
                // position add-friend button next to username, clamp within card
                btnAddFriend.Left = lblUsername.Right + 10;
                btnAddFriend.Top = lblUsername.Top - 1;

                var maxLeft = card.ClientSize.Width - card.Padding.Right - btnAddFriend.Width;
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

            actionBar.Controls.Add(btnLike, 0, 0);
            actionBar.Controls.Add(btnComment, 1, 0);

            ApplyRoundedAvatar(picAvatar);
            card.Paint += (_, e) =>
            {
                var border = Color.FromArgb(228, 228, 228);
                var rect = card.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                using var pen = new Pen(border, 1);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(pen, rect);
            };

            WireClickToChildren(card, async () => await OpenPostDetailAsync(post.Id));
            btnComment.Click += async (_, __) => await OpenPostDetailAsync(post.Id, openComments: true);
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

            var contentHeight = MeasureLabelHeight(lblContent.Text, lblContent.Font, lblContent.Width, maxLines: 6);
            lblContent.Height = Math.Max(22, contentHeight);

            mediaHost.Top = lblContent.Bottom + 10;

            lblStats.Top = (mediaHost.Visible ? mediaHost.Bottom : lblContent.Bottom) + 10;
            divider.Top = lblStats.Bottom + 8;
            actionBar.Top = divider.Bottom + 6;
            card.Height = actionBar.Bottom + 10;

            actionBar.BringToFront();

            _ = LoadCardAssetsAsync();

            // Friend button visibility + action
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
                if (string.IsNullOrWhiteSpace(authorId))
                    return;

                btnAddFriend.Enabled = false;
                btnAddFriend.Text = "Đang gửi...";
                try
                {
                    await _friendBll.SendRequestAsync(authorId);
                    _sentFriendRequests.Add(authorId);
                    btnAddFriend.Text = "Đã gửi";
                }
                catch (Exception ex)
                {
                    btnAddFriend.Enabled = true;
                    btnAddFriend.Text = "Kết bạn";
                    MessageBox.Show("Không thể gửi lời mời: " + ex.Message);
                }
            };

            async Task LoadCardAssetsAsync()
            {
                try
                {
                    if (post.User != null && !string.IsNullOrWhiteSpace(post.User.AvatarURL))
                    {
                        var avatar = await LoadImageFromUrl(post.User.AvatarURL);
                        if (avatar != null && !picAvatar.IsDisposed)
                            picAvatar.Image = avatar;
                    }

                    if (post.Media != null && post.Media.Count > 0)
                    {
                        var images = post.Media.FindAll(x =>
                            !string.IsNullOrWhiteSpace(x?.Url) &&
                            string.Equals(x.Type, "image", StringComparison.OrdinalIgnoreCase));

                        if (images.Count > 0 && !mediaHost.IsDisposed)
                        {
                            mediaHost.Visible = true;
                            mediaHost.Controls.Clear();

                            await BuildMediaGridAsync(mediaHost, images);

                            lblStats.Top = (mediaHost.Visible ? mediaHost.Bottom : lblContent.Bottom) + 10;
                            divider.Top = lblStats.Bottom + 8;
                            actionBar.Top = divider.Bottom + 6;
                            card.Height = actionBar.Bottom + 10;
                            actionBar.BringToFront();
                        }
                    }
                }
                catch
                {
                }
            }

            return card;
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

        private void CenterFeedCards()
        {
            if (PostFeed == null || PostFeed.IsDisposed) return;

            var maxWidth = Math.Min(FeedMaxWidth, Math.Max(360, PostFeed.ClientSize.Width - 40));
            var left = Math.Max(10, (PostFeed.ClientSize.Width - maxWidth) / 2);

            foreach (Control c in PostFeed.Controls)
            {
                if (c is not Panel p) continue;
                p.Width = maxWidth;
                p.Margin = new Padding(left, p.Margin.Top, 10, p.Margin.Bottom);
            }
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
                if (IsBlocked(c))
                    return;

                c.Click += Handler;
                foreach (Control child in c.Controls)
                    Attach(child);
            }

            Attach(root);
        }

        private static void ApplyRoundedAvatar(PictureBox pictureBox)
        {
            void UpdateRegion()
            {
                var diameter = Math.Min(pictureBox.Width, pictureBox.Height);
                var rect = new Rectangle(0, 0, diameter, diameter);
                using var path = new GraphicsPath();
                path.AddEllipse(rect);
                pictureBox.Region = new Region(path);
            }

            pictureBox.SizeChanged += (_, __) => UpdateRegion();
            UpdateRegion();
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
                    var overlay = new Panel
                    {
                        Dock = DockStyle.Fill,
                        BackColor = Color.FromArgb(120, 0, 0, 0),
                        Margin = Padding.Empty
                    };
                    var lbl = new Label
                    {
                        Text = $"+{count - 4}",
                        ForeColor = Color.White,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Segoe UI", 16, FontStyle.Bold),
                        BackColor = Color.Transparent
                    };
                    overlay.Controls.Add(lbl);
                    cell.Controls.Add(overlay);
                    overlay.BringToFront();
                }
            }
        }

        private async Task<Image> LoadImageFromUrl(string relativeOrFullUrl)
        {
            try
            {
                using var httpClient = new HttpClient();

                string fullUrl = relativeOrFullUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
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

        private string FormatTime(DateTime createdAt)
        {
            var localTime = createdAt.ToLocalTime();
            var span = DateTime.Now - localTime;

            if (span.TotalMinutes < 1) return "Vừa xong";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} phút trước";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} giờ trước";

            return localTime.ToString("dd/MM/yyyy HH:mm");
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            Profile profile = new Profile(this);
            profile.Show();
            this.Hide();
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            LoadPosts();
        }
    }
}