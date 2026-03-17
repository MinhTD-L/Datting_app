using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessLogic;
using DataTransferObject;

namespace Presentation
{
    public class PostDetailForm : Form
    {
        private readonly PostBLL _postBll;
        private readonly string _postId;
        private readonly bool _openComments;

        private readonly Panel _root;
        private readonly Label _lblTitle;
        private readonly Label _lblLoading;

        private TextBox _commentInput;
        private Button _btnSendComment;
        private FlowLayoutPanel _commentsList;
        private Panel _loadingOverlay;
        private string _replyParentId;
        private string _replyToUsername;

        private const string BaseUrl = "https://litmatchclone-production.up.railway.app";

        public PostDetailForm(PostBLL postBll, string postId, bool openComments = false)
        {
            _postBll = postBll ?? throw new ArgumentNullException(nameof(postBll));
            _postId = postId ?? throw new ArgumentNullException(nameof(postId));
            _openComments = openComments;

            Text = "Post detail";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(720, 640);
            BackColor = Color.FromArgb(245, 246, 250);

            _root = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(18),
                BackColor = BackColor
            };

            _lblTitle = new Label
            {
                Text = "Chi tiết bài viết",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, 0)
            };

            _lblLoading = new Label
            {
                Text = "Đang tải...",
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(0, _lblTitle.Bottom + 10)
            };

            _root.Controls.Add(_lblTitle);
            _root.Controls.Add(_lblLoading);
            Controls.Add(_root);

            BuildLoadingOverlay();
            Shown += PostDetailForm_Shown;
        }

        private void BuildLoadingOverlay()
        {
            _loadingOverlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 246, 250),
                Visible = false
            };
            var lbl = new Label
            {
                Text = "Đang tải bài viết...",
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
            _loadingOverlay.Controls.Add(lbl);
            _loadingOverlay.Controls.Add(bar);

            void Reposition()
            {
                lbl.Left = (_root.ClientSize.Width - lbl.Width) / 2;
                lbl.Top = 140;
                bar.Left = (_root.ClientSize.Width - bar.Width) / 2;
                bar.Top = lbl.Bottom + 14;
            }

            _root.SizeChanged += (_, __) => Reposition();
            _loadingOverlay.VisibleChanged += (_, __) => Reposition();
            _root.Controls.Add(_loadingOverlay);
            _loadingOverlay.BringToFront();
        }

        private void SetLoading(bool isLoading)
        {
            if (_loadingOverlay == null) return;
            _loadingOverlay.Visible = isLoading;
            _loadingOverlay.BringToFront();
        }

        private async void PostDetailForm_Shown(object sender, EventArgs e)
        {
            await LoadAndRenderAsync();
        }

        private async Task LoadAndRenderAsync()
        {
            try
            {
                SetLoading(true);

                var post = await _postBll.GetPostDetailAsync(_postId);
                if (post == null)
                {
                    SetLoading(false);
                    _lblLoading.Text = "Không tìm thấy bài viết.";
                    _lblLoading.Visible = true;
                    return;
                }

                _lblLoading.Visible = false;
                RenderPost(post);
                SetLoading(false);
            }
            catch (UnauthorizedAccessException)
            {
                SetLoading(false);
                MessageBox.Show("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
                Close();
            }
            catch (Exception ex)
            {
                SetLoading(false);
                MessageBox.Show("Lỗi tải bài viết: " + ex.Message);
            }
        }

        private void RenderPost(PostFeedDTO post)
        {
            for (var i = _root.Controls.Count - 1; i >= 0; i--)
            {
                var c = _root.Controls[i];
                if (!ReferenceEquals(c, _lblTitle) && !ReferenceEquals(c, _lblLoading))
                    _root.Controls.RemoveAt(i);
            }

            var card = new Panel
            {
                Width = _root.ClientSize.Width - _root.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth,
                Height = 200,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(14),
                Location = new Point(0, _lblTitle.Bottom + 14)
            };

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

            var picAvatar = new PictureBox
            {
                Size = new Size(52, 52),
                Location = new Point(14, 14),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(238, 238, 238)
            };
            ApplyRoundedAvatar(picAvatar);

            var lblUsername = new Label
            {
                Text = post.User?.Username ?? "Unknown",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(picAvatar.Right + 12, 14)
            };

            var lblTime = new Label
            {
                Text = post.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(picAvatar.Right + 12, 40)
            };

            var lblContent = new Label
            {
                Text = post.Content ?? "",
                Font = new Font("Segoe UI", 10),
                AutoSize = false,
                Location = new Point(14, picAvatar.Bottom + 12),
                Width = card.Width - 28,
                Height = 1,
                ForeColor = Color.FromArgb(30, 30, 30)
            };

            var contentHeight = MeasureLabelHeight(lblContent.Text, lblContent.Font, lblContent.Width, maxLines: 20);
            lblContent.Height = Math.Max(22, contentHeight);

            var mediaHost = new Panel
            {
                Location = new Point(14, lblContent.Bottom + 12),
                Width = card.Width - 28,
                Height = 0,
                BackColor = Color.Transparent,
                Visible = false
            };

            var lblStats = new Label
            {
                Text = $"❤️ {post.LikeCount}    💬 {post.CommentCount}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(14, 0)
            };

            var divider1 = new Panel
            {
                Height = 1,
                Width = card.Width - 28,
                BackColor = Color.FromArgb(235, 235, 235),
                Location = new Point(14, 0)
            };

            var commentsHeader = new Label
            {
                Text = "Bình luận",
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, card.Bottom + 16)
            };

            _commentInput = new TextBox
            {
                PlaceholderText = "Viết bình luận...",
                Width = card.Width - 90,
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, commentsHeader.Bottom + 10)
            };

            _btnSendComment = new Button
            {
                Text = "Gửi",
                Width = 70,
                Height = _commentInput.Height,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(_commentInput.Right + 10, _commentInput.Top - 2),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 30, 100),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnSendComment.FlatAppearance.BorderSize = 0;

            _commentsList = new FlowLayoutPanel
            {
                Location = new Point(0, _commentInput.Bottom + 12),
                Width = card.Width,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.TopDown,
                BackColor = Color.Transparent
            };

            var actionBar = new TableLayoutPanel
            {
                Width = card.Width - 28,
                Height = 40,
                Location = new Point(14, 0),
                BackColor = Color.Transparent,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            actionBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var btnLike = CreateActionButton("👍  Thích");
            var btnComment = CreateActionButton("💬  Bình luận");
            btnLike.Dock = DockStyle.Fill;
            btnComment.Dock = DockStyle.Fill;

            void UpdateLikeUi()
            {
                btnLike.Text = post.IsLiked ? "👍  Đã thích" : "👍  Thích";
                btnLike.ForeColor = post.IsLiked ? Color.FromArgb(24, 119, 242) : Color.FromArgb(70, 70, 70);
                lblStats.Text = $"❤️ {post.LikeCount}    💬 {post.CommentCount}";
            }
            UpdateLikeUi();

            btnLike.Click += async (_, __) =>
            {
                if (string.IsNullOrWhiteSpace(post.Id))
                    return;

                btnLike.Enabled = false;

                // optimistic update
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

            btnComment.Click += (_, __) =>
            {
                // Focus comments after render
                _root.Tag = "open_comments";
                ScrollToCommentsIfNeeded();
            };

            actionBar.Controls.Add(btnLike, 0, 0);
            actionBar.Controls.Add(btnComment, 1, 0);

            card.Controls.Add(picAvatar);
            card.Controls.Add(lblUsername);
            card.Controls.Add(lblTime);
            card.Controls.Add(lblContent);
            card.Controls.Add(mediaHost);
            card.Controls.Add(lblStats);
            card.Controls.Add(divider1);
            card.Controls.Add(actionBar);

            _root.Controls.Add(card);
            card.BringToFront();

            _root.Controls.Add(commentsHeader);
            _root.Controls.Add(_commentInput);
            _root.Controls.Add(_btnSendComment);
            _root.Controls.Add(_commentsList);

            // Layout ngay lập tức để tránh UI bị "nhảy" / chồng lên nhau
            lblStats.Top = (mediaHost.Visible ? mediaHost.Bottom : lblContent.Bottom) + 12;
            divider1.Top = lblStats.Bottom + 10;
            actionBar.Top = divider1.Bottom + 8;
            card.Height = actionBar.Bottom + 12;

            commentsHeader.Top = card.Bottom + 16;
            _commentInput.Top = commentsHeader.Bottom + 10;
            _btnSendComment.Top = _commentInput.Top - 2;
            _btnSendComment.Left = _commentInput.Right + 10;
            _commentsList.Top = _commentInput.Bottom + 12;

            // Ensure action bar stays visible (avoid being covered)
            actionBar.BringToFront();

            // Render comment placeholder ngay để che delay
            _commentsList.Controls.Clear();
            _commentsList.Controls.Add(new Label
            {
                Text = "Đang tải bình luận...",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.Gray,
                Margin = new Padding(0, 0, 0, 0)
            });

            HookCommentEvents();
            _ = LoadImagesAndFinalizeAsync();

            async Task LoadImagesAndFinalizeAsync()
            {
                // Quan trọng: tuyệt đối không thao tác UI ở thread nền.
                // Nếu media/avatar load lỗi thì vẫn phải render comments (không để kẹt ở "Đang tải bình luận...").
                try
                {
                    if (post.User != null && !string.IsNullOrWhiteSpace(post.User.AvatarURL))
                    {
                        try
                        {
                            var avatar = await LoadImageFromUrl(post.User.AvatarURL);
                            if (avatar != null && !picAvatar.IsDisposed)
                                picAvatar.Image = avatar;
                        }
                        catch
                        {
                            // ignore avatar failures
                        }
                    }

                    if (post.Media != null && post.Media.Count > 0)
                    {
                        var images = post.Media.FindAll(x =>
                            !string.IsNullOrWhiteSpace(x?.Url) &&
                            string.Equals(x.Type, "image", StringComparison.OrdinalIgnoreCase));

                        if (images.Count > 0 && !mediaHost.IsDisposed)
                        {
                            mediaHost.Visible = true;
                            mediaHost.Height = (mediaHost.Width * 9) / 16;
                            mediaHost.Controls.Clear();
                            mediaHost.Controls.Add(new Panel
                            {
                                Dock = DockStyle.Fill,
                                BackColor = Color.FromArgb(245, 246, 250)
                            });

                            try
                            {
                                await BuildMediaGridAsync(mediaHost, images);
                            }
                            catch
                            {
                                // ignore media failures
                            }
                        }
                    }
                }
                finally
                {
                    // Reflow layout + render comments luôn luôn chạy
                    lblStats.Top = (mediaHost.Visible ? mediaHost.Bottom : lblContent.Bottom) + 12;
                    divider1.Top = lblStats.Bottom + 10;
                    actionBar.Top = divider1.Bottom + 8;
                    card.Height = actionBar.Bottom + 12;

                    commentsHeader.Top = card.Bottom + 16;
                    _commentInput.Top = commentsHeader.Bottom + 10;
                    _btnSendComment.Top = _commentInput.Top - 2;
                    _btnSendComment.Left = _commentInput.Right + 10;
                    _commentsList.Top = _commentInput.Bottom + 12;

                    HookCommentEvents();
                    await RenderCommentsAsync(_commentsList, post);

                    if (_openComments)
                    {
                        _root.Tag = "open_comments";
                        ScrollControlIntoView(_commentInput);
                    }
                }
            }

            void ScrollToCommentsIfNeeded()
            {
                if ((_root.Tag as string) == "open_comments")
                    ScrollControlIntoView(_commentInput);
            }
        }

        private void HookCommentEvents()
        {
            if (_commentInput == null || _btnSendComment == null) return;

            _commentInput.KeyDown -= CommentInput_KeyDown;
            _commentInput.KeyDown += CommentInput_KeyDown;

            _btnSendComment.Click -= BtnSendComment_Click;
            _btnSendComment.Click += BtnSendComment_Click;
        }

        private async void BtnSendComment_Click(object? sender, EventArgs e)
        {
            await SubmitCommentAsync();
        }

        private async void CommentInput_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                await SubmitCommentAsync();
            }
        }

        private async Task SubmitCommentAsync()
        {
            if (_commentInput == null) return;

            var content = _commentInput.Text?.Trim();
            if (string.IsNullOrWhiteSpace(content))
                return;

            try
            {
                _commentInput.Enabled = false;
                if (_btnSendComment != null)
                    _btnSendComment.Enabled = false;

                if (!string.IsNullOrWhiteSpace(_replyParentId))
                    await _postBll.ReplyCommentAsync(_postId, _replyParentId, content);
                else
                    await _postBll.CommentAsync(_postId, content);

                _replyParentId = null;
                _replyToUsername = null;
                _commentInput.Text = string.Empty;

                // Reload post detail to see new comment
                await LoadAndRenderAsync();
                _root.Tag = "open_comments";
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gửi bình luận thất bại: " + ex.Message);
            }
            finally
            {
                if (_commentInput != null)
                    _commentInput.Enabled = true;
                if (_btnSendComment != null)
                    _btnSendComment.Enabled = true;
            }
        }

        private async Task RenderCommentsAsync(FlowLayoutPanel list, PostFeedDTO post)
        {
            list.SuspendLayout();
            list.Controls.Clear();

            if (post.Comments == null || post.Comments.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "Chưa có bình luận nào.",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    Margin = new Padding(0, 0, 0, 0)
                };
                list.Controls.Add(lblEmpty);
                list.ResumeLayout();
                return;
            }

            // Xây cây comment theo parent_id
            var all = post.Comments;
            var children = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<PostCommentDto>>();
            foreach (var c in all)
            {
                if (string.IsNullOrWhiteSpace(c.ParentId)) continue;
                if (!children.TryGetValue(c.ParentId, out var listChildren))
                {
                    listChildren = new System.Collections.Generic.List<PostCommentDto>();
                    children[c.ParentId] = listChildren;
                }
                listChildren.Add(c);
            }

            async Task RenderNodeAsync(PostCommentDto c, int level)
            {
                var item = await BuildCommentItemAsync(list.Width, c, level);
                list.Controls.Add(item);

                if (children.TryGetValue(c.Id, out var replies))
                {
                    foreach (var r in replies)
                        await RenderNodeAsync(r, level + 1);
                }
            }

            foreach (var root in all)
            {
                if (!string.IsNullOrWhiteSpace(root.ParentId)) continue;
                await RenderNodeAsync(root, 0);
            }

            list.ResumeLayout();
        }

        private async Task<Control> BuildCommentItemAsync(int width, PostCommentDto comment, int level)
        {
            var row = new Panel
            {
                Width = width,
                Height = 10,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 10)
            };

            // Thụt lề theo level (reply level 1,2,...)
            var indent = level * 28;

            var avatar = new PictureBox
            {
                Size = new Size(34, 34),
                Location = new Point(indent, 0),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(238, 238, 238)
            };
            ApplyRoundedAvatar(avatar);

            var bubble = new Panel
            {
                BackColor = Color.FromArgb(242, 243, 245),
                Location = new Point(avatar.Right + 10, 0),
                Width = width - (avatar.Right + 10) - indent,
                Height = 10,
                Padding = new Padding(10, 8, 10, 8)
            };

            var lblUser = new Label
            {
                Text = comment?.User?.Username ?? "Unknown",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, 0)
            };

            var lblContent = new Label
            {
                Text = comment?.Content ?? "",
                AutoSize = false,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, lblUser.Bottom + 4),
                Width = bubble.Width - bubble.Padding.Horizontal,
                Height = 1
            };
            lblContent.Height = Math.Max(18, MeasureLabelHeight(lblContent.Text, lblContent.Font, lblContent.Width, maxLines: 10));

            var lblMeta = new Label
            {
                Text = comment?.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "",
                AutoSize = true,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(0, lblContent.Bottom + 6)
            };

            bubble.Height = lblMeta.Bottom + 2;
            bubble.Controls.Add(lblUser);
            bubble.Controls.Add(lblContent);
            bubble.Controls.Add(lblMeta);

            var btnReply = new LinkLabel
            {
                Text = "Phản hồi",
                AutoSize = true,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                LinkColor = Color.FromArgb(80, 80, 80),
                ActiveLinkColor = Color.FromArgb(40, 40, 40),
                Location = new Point(lblMeta.Right + 12, lblMeta.Top - 1)
            };
            btnReply.Click += (_, __) =>
            {
                if (_commentInput == null) return;
                var userName = comment?.User?.Username ?? "";
                _replyParentId = comment?.Id;
                _replyToUsername = userName;
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    _commentInput.Text = "@" + userName + " ";
                    _commentInput.Focus();
                    _commentInput.SelectionStart = _commentInput.Text.Length;
                }
                else
                {
                    _commentInput.Focus();
                }
            };
            bubble.Controls.Add(btnReply);

            row.Height = Math.Max(avatar.Height, bubble.Height);
            row.Controls.Add(avatar);
            row.Controls.Add(bubble);

            if (!string.IsNullOrWhiteSpace(comment?.User?.AvatarURL))
            {
                var img = await LoadImageFromUrl(comment.User.AvatarURL);
                if (img != null)
                    avatar.Image = img;
            }

            return row;
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

        private async Task<Image> LoadImageFromUrl(string relativeOrFullUrl)
        {
            try
            {
                using var httpClient = new System.Net.Http.HttpClient();

                var fullUrl = relativeOrFullUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? relativeOrFullUrl
                    : $"{BaseUrl}{relativeOrFullUrl}";

                var bytes = await httpClient.GetByteArrayAsync(fullUrl);
                using var ms = new System.IO.MemoryStream(bytes);
                return Image.FromStream(ms);
            }
            catch
            {
                return null;
            }
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
            var gap = 6;
            var maxShow = Math.Min(4, count);

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            if (count == 1)
            {
                grid.RowCount = 1;
                grid.ColumnCount = 1;
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                host.Height = (host.Width * 9) / 16;
            }
            else if (count == 2)
            {
                grid.RowCount = 1;
                grid.ColumnCount = 2;
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                host.Height = (host.Width * 9) / 20;
            }
            else if (count == 3)
            {
                grid.RowCount = 2;
                grid.ColumnCount = 2;
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
                host.Height = (host.Width * 9) / 16;
            }
            else
            {
                grid.RowCount = 2;
                grid.ColumnCount = 2;
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                host.Height = (host.Width * 9) / 16;
            }

            host.Controls.Add(grid);

            PictureBox CreateMediaBox()
            {
                return new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.FromArgb(245, 246, 250),
                    Margin = new Padding(gap / 2),
                };
            }

            async Task<Image> TryLoad(string url) => await LoadImageFromUrl(url);

            if (count == 1)
            {
                var pb = CreateMediaBox();
                pb.Margin = Padding.Empty;
                grid.Controls.Add(pb, 0, 0);
                pb.Image = await TryLoad(images[0].Url);
                return;
            }

            if (count == 2)
            {
                for (var i = 0; i < 2; i++)
                {
                    var pb = CreateMediaBox();
                    grid.Controls.Add(pb, i, 0);
                    pb.Image = await TryLoad(images[i].Url);
                }
                return;
            }

            if (count == 3)
            {
                var pb0 = CreateMediaBox();
                grid.Controls.Add(pb0, 0, 0);
                grid.SetRowSpan(pb0, 2);

                var pb1 = CreateMediaBox();
                grid.Controls.Add(pb1, 1, 0);

                var pb2 = CreateMediaBox();
                grid.Controls.Add(pb2, 1, 1);

                pb0.Image = await TryLoad(images[0].Url);
                pb1.Image = await TryLoad(images[1].Url);
                pb2.Image = await TryLoad(images[2].Url);
                return;
            }

            var cells = new (int col, int row)[] { (0, 0), (1, 0), (0, 1), (1, 1) };
            for (var i = 0; i < maxShow; i++)
            {
                var pb = CreateMediaBox();
                grid.Controls.Add(pb, cells[i].col, cells[i].row);
                pb.Image = await TryLoad(images[i].Url);

                if (i == 3 && count > 4)
                {
                    var overlay = new Panel
                    {
                        Dock = DockStyle.Fill,
                        BackColor = Color.FromArgb(120, 0, 0, 0),
                        Margin = pb.Margin
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
                    grid.Controls.Add(overlay, cells[i].col, cells[i].row);
                    overlay.BringToFront();
                }
            }
        }
    }
}

