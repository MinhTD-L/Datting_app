using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataTransferObject;
using DataAccess;

namespace Presentation
{
    public partial class MainDashboard : Form
    {
        private readonly PostDAL _postDal = new PostDAL();
        private const string BaseUrl = "https://litmatchclone-production.up.railway.app";
        private Panel _feedLoadingOverlay;

        public MainDashboard()
        {
            InitializeComponent();

            PostFeed.AutoScroll = true;
            PostFeed.FlowDirection = FlowDirection.TopDown;
            PostFeed.WrapContents = false;

            BuildFeedLoadingOverlay();
            this.Load += Dashboard_Load;
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
            await LoadPosts();
        }

        private async Task LoadPosts()
        {
            try
            {
                PostFeed.Controls.Clear();
                SetFeedLoading(true);

                var result = await _postDal.GetPost();

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
            var card = new Panel
            {
                Width = PostFeed.ClientSize.Width - 30,
                Height = 200,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(10),
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
                BackColor = Color.Transparent,
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

            var actionBar = new Panel
            {
                Width = card.Width - 24,
                Height = 36,
                Location = new Point(12, 0),
                BackColor = Color.Transparent
            };

            var btnLike = CreateActionButton("👍 Thích");
            var btnComment = CreateActionButton("💬 Bình luận");
            btnLike.Left = 0;
            btnComment.Left = btnLike.Right + 10;

            card.Controls.Add(picAvatar);
            card.Controls.Add(lblUsername);
            card.Controls.Add(lblTime);
            card.Controls.Add(lblContent);
            card.Controls.Add(mediaHost);
            card.Controls.Add(lblStats);
            card.Controls.Add(divider);
            card.Controls.Add(actionBar);

            actionBar.Controls.Add(btnLike);
            actionBar.Controls.Add(btnComment);

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

            var contentHeight = MeasureLabelHeight(lblContent.Text, lblContent.Font, lblContent.Width, maxLines: 6);
            lblContent.Height = Math.Max(22, contentHeight);

            mediaHost.Top = lblContent.Bottom + 10;

            lblStats.Top = (mediaHost.Visible ? mediaHost.Bottom : lblContent.Bottom) + 10;
            divider.Top = lblStats.Bottom + 8;
            actionBar.Top = divider.Bottom + 6;
            card.Height = actionBar.Bottom + 10;

            // Ensure action bar stays visible
            actionBar.BringToFront();

            _ = LoadCardAssetsAsync();

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
                            mediaHost.Height = (mediaHost.Width * 9) / 16;
                            mediaHost.Controls.Clear();
                            mediaHost.Controls.Add(new Panel
                            {
                                Dock = DockStyle.Fill,
                                BackColor = Color.FromArgb(245, 246, 250)
                            });

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

            using var detail = new PostDetailForm(_postDal, postId, openComments);
            detail.ShowDialog(this);
        }

        private static Button CreateActionButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(70, 70, 70),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private static void WireClickToChildren(Control root, Func<Task> onClickAsync)
        {
            if (root == null || onClickAsync == null) return;

            async void Handler(object sender, EventArgs e)
            {
                try { await onClickAsync(); }
                catch { /* UI action: ignore */ }
            }

            void Attach(Control c)
            {
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
                host.Height = (host.Width * 9) / 16; // keep a nice aspect ratio
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

            // 4+
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
    }
}