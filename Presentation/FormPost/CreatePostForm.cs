using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessLogic;
using DataTransferObject;

namespace Presentation
{
    public sealed class CreatePostForm : Form
    {
        private readonly PostBLL _postBll;
        private readonly UserBLL _userBll;

        private readonly TextBox _txtContent;
        private readonly FlowLayoutPanel _mediaPreview;
        private readonly Button _btnAddMedia;
        private readonly Button _btnClearMedia;
        private readonly Button _btnPost;
        private readonly Button _btnCancel;
        private readonly Label _lblStatus;
        private readonly PictureBox _picAvatar;
        private readonly Label _lblName;

        private readonly List<LocalMedia> _selected = new();
        private string _contentPlaceholder = "Bạn đang nghĩ gì thế?";
        private const string BaseUrl = "https://litmatchclone-production-944b.up.railway.app";

        private sealed class LocalMedia
        {
            public string Path { get; init; }
            public string Type { get; init; }
        }

        public CreatePostForm(PostBLL postBll, UserBLL userBll)
        {
            _postBll = postBll ?? throw new ArgumentNullException(nameof(postBll));
            _userBll = userBll ?? throw new ArgumentNullException(nameof(userBll));

            Text = "Tạo bài viết";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.White;
            Width = 620;
            Height = 560;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.White,
                Padding = new Padding(16, 10, 16, 10)
            };

            var lblTitle = new Label
            {
                AutoSize = true,
                Text = "Tạo bài viết",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 25, 25),
                Location = new Point(0, 0)
            };

            var headerDivider = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = Color.FromArgb(235, 235, 235)
            };

            header.Controls.Add(lblTitle);
            header.Controls.Add(headerDivider);

            var body = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 12)
            };

            var userRow = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.White
            };

            _picAvatar = new PictureBox
            {
                Width = 40,
                Height = 40,
                Left = 0,
                Top = 6,
                BackColor = Color.FromArgb(225, 227, 232),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            ApplyRoundedAvatar(_picAvatar);

            _lblName = new Label
            {
                AutoSize = true,
                Text = string.IsNullOrWhiteSpace(SessionManager.Username) ? "Bạn" : SessionManager.Username,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 25, 25),
                Left = _picAvatar.Right + 10,
                Top = 7
            };

            var cboPrivacy = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Left = _picAvatar.Right + 10,
                Top = 26,
                Width = 120
            };
            cboPrivacy.Items.AddRange(new object[] { "Công khai", "Bạn bè", "Chỉ mình tôi" });
            cboPrivacy.SelectedIndex = 0;

            userRow.Controls.Add(_picAvatar);
            userRow.Controls.Add(_lblName);
            userRow.Controls.Add(cboPrivacy);

            _txtContent = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 12),
                Height = 130,
                Dock = DockStyle.Top
            };
            _txtContent.Text = _contentPlaceholder;
            _txtContent.ForeColor = Color.Gray;
            _txtContent.GotFocus += (_, __) =>
            {
                if (string.Equals(_txtContent.Text, _contentPlaceholder, StringComparison.Ordinal))
                {
                    _txtContent.Text = string.Empty;
                    _txtContent.ForeColor = Color.FromArgb(25, 25, 25);
                }
            };
            _txtContent.LostFocus += (_, __) =>
            {
                if (string.IsNullOrWhiteSpace(_txtContent.Text))
                {
                    _txtContent.Text = _contentPlaceholder;
                    _txtContent.ForeColor = Color.Gray;
                }
            };
            _txtContent.TextChanged += (_, __) => UpdatePostButtonState();

            var contentWrap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                BackColor = Color.White,
                Padding = new Padding(8, 6, 8, 6)
            };
            contentWrap.Paint += (_, e) =>
            {
                var rect = contentWrap.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                using var pen = new Pen(Color.FromArgb(230, 230, 230), 1);
                e.Graphics.DrawRectangle(pen, rect);
            };
            contentWrap.Controls.Add(_txtContent);

            var actions = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                Padding = new Padding(0, 10, 0, 0),
                BackColor = Color.White
            };

            _btnAddMedia = new Button
            {
                Text = "Ảnh/Video",
                Height = 32,
                Width = 120,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(245, 246, 250),
                ForeColor = Color.FromArgb(45, 45, 45)
            };
            _btnAddMedia.FlatAppearance.BorderSize = 0;
            _btnAddMedia.Click += (_, __) => PickMedia();

            _btnClearMedia = new Button
            {
                Text = "Xoá",
                Height = 32,
                Width = 70,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(140, 30, 30),
                Left = _btnAddMedia.Right + 10
            };
            _btnClearMedia.FlatAppearance.BorderSize = 0;
            _btnClearMedia.Click += (_, __) =>
            {
                _selected.Clear();
                RebuildPreview();
                UpdatePostButtonState();
            };

            actions.Controls.Add(_btnAddMedia);
            actions.Controls.Add(_btnClearMedia);

            var previewWrap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 210,
                BackColor = Color.FromArgb(245, 246, 250),
                Padding = new Padding(10),
                Visible = false
            };

            _mediaPreview = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                BackColor = Color.FromArgb(245, 246, 250)
            };
            previewWrap.Controls.Add(_mediaPreview);

            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 74,
                BackColor = Color.White,
                Padding = new Padding(16, 10, 16, 12)
            };

            _lblStatus = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 20,
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var footerBtns = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40
            };

            _btnPost = new Button
            {
                Text = "Đăng",
                Width = 110,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(24, 119, 242),
                ForeColor = Color.White,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                Left = footerBtns.Width - 110
            };
            _btnPost.FlatAppearance.BorderSize = 0;
            _btnPost.Click += async (_, __) => await SubmitAsync();

            _btnCancel = new Button
            {
                Text = "Huỷ",
                Width = 110,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(245, 246, 250),
                ForeColor = Color.FromArgb(45, 45, 45),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Click += (_, __) => Close();

            footerBtns.Controls.Add(_btnCancel);
            footerBtns.Controls.Add(_btnPost);

            footerBtns.SizeChanged += (_, __) =>
            {
                _btnPost.Left = footerBtns.ClientSize.Width - _btnPost.Width;
                _btnCancel.Left = _btnPost.Left - _btnCancel.Width - 10;
                _btnPost.Top = 2;
                _btnCancel.Top = 2;
            };

            footer.Controls.Add(footerBtns);
            footer.Controls.Add(_lblStatus);

            var spacer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            body.Controls.Add(spacer);
            body.Controls.Add(previewWrap);
            body.Controls.Add(actions);
            body.Controls.Add(contentWrap);
            body.Controls.Add(userRow);

            Controls.Add(body);
            Controls.Add(footer);
            Controls.Add(header);

            Shown += async (_, __) =>
            {
                _txtContent.Focus();
                _lblStatus.Text = "Thêm nội dung hoặc ảnh/video rồi bấm Đăng.";
                await LoadCurrentUserAsync();
                UpdatePostButtonState();
            };
        }

        private void PickMedia()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Chọn ảnh/video",
                Multiselect = true,
                Filter = "Media|*.jpg;*.jpeg;*.png;*.gif;*.webp;*.bmp;*.mp4;*.mov;*.avi;*.mkv|All files|*.*"
            };

            if (ofd.ShowDialog(this) != DialogResult.OK)
                return;

            foreach (var p in ofd.FileNames)
            {
                if (string.IsNullOrWhiteSpace(p) || !File.Exists(p))
                    continue;

                var type = GuessType(p);
                if (type == null)
                    continue;

                _selected.Add(new LocalMedia
                {
                    Path = p,
                    Type = type
                });
            }

            RebuildPreview();
            UpdatePostButtonState();
        }

        private static string GuessType(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant() ?? string.Empty;

            if (ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp")
                return "image";

            if (ext is ".mp4" or ".mov" or ".avi" or ".mkv")
                return "video";

            return null;
        }

        private void RebuildPreview()
        {
            _mediaPreview.SuspendLayout();
            _mediaPreview.Controls.Clear();

            if (_selected.Count == 0)
            {
                if (_mediaPreview.Parent != null)
                    _mediaPreview.Parent.Visible = false;

                _mediaPreview.ResumeLayout();
                return;
            }

            if (_mediaPreview.Parent != null)
                _mediaPreview.Parent.Visible = true;

            foreach (var item in _selected.ToArray())
            {
                var tile = new Panel
                {
                    Width = 150,
                    Height = 150,
                    BackColor = Color.White,
                    Margin = new Padding(6),
                    Padding = new Padding(6)
                };

                tile.Paint += (_, e) =>
                {
                    var rect = tile.ClientRectangle;
                    rect.Width -= 1;
                    rect.Height -= 1;
                    using var pen = new Pen(Color.FromArgb(230, 230, 230), 1);
                    e.Graphics.DrawRectangle(pen, rect);
                };

                var thumb = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.FromArgb(245, 246, 250)
                };

                var badge = new Label
                {
                    AutoSize = true,
                    Text = item.Type.Equals("video", StringComparison.OrdinalIgnoreCase) ? "VIDEO" : "IMAGE",
                    BackColor = Color.FromArgb(30, 30, 30),
                    ForeColor = Color.White,
                    Padding = new Padding(6, 2, 6, 2)
                };

                tile.Controls.Add(thumb);
                tile.Controls.Add(badge);
                badge.BringToFront();
                badge.Left = 6;
                badge.Top = 6;

                if (item.Type == "image")
                {
                    try
                    {
                        using var fs = new FileStream(item.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        using var img = Image.FromStream(fs);
                        thumb.Image = new Bitmap(img);
                    }
                    catch
                    {
                        // bỏ qua lỗi preview
                    }
                }

                var btnRemove = new Button
                {
                    Text = "✕",
                    Width = 28,
                    Height = 28,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(160, 30, 30),
                    Cursor = Cursors.Hand
                };
                btnRemove.FlatAppearance.BorderSize = 1;
                btnRemove.FlatAppearance.BorderColor = Color.FromArgb(230, 230, 230);
                btnRemove.Left = tile.Width - btnRemove.Width - 6;
                btnRemove.Top = 6;
                btnRemove.Click += (_, __) =>
                {
                    _selected.Remove(item);
                    RebuildPreview();
                    UpdatePostButtonState();
                };

                tile.Controls.Add(btnRemove);
                btnRemove.BringToFront();

                _mediaPreview.Controls.Add(tile);
            }

            _mediaPreview.ResumeLayout();
        }

        private async Task SubmitAsync()
        {
            var raw = _txtContent.Text ?? string.Empty;
            var content = string.Equals(raw, _contentPlaceholder, StringComparison.Ordinal)
                ? string.Empty
                : raw.Trim();

            if (string.IsNullOrWhiteSpace(content) && _selected.Count == 0)
            {
                MessageBox.Show(
                    this,
                    "Vui lòng nhập nội dung hoặc chọn ít nhất 1 media.",
                    "Thiếu thông tin",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            SetBusy(true, "Đang tải lên dữ liệu...");

            try
            {
                    var mediaPaths = new List<string>();
                foreach (var item in _selected)
                {
                        mediaPaths.Add(item.Path);
                }

                _lblStatus.Text = "Đang tạo bài viết...";
                    var result = await _postBll.CreatePostAsync(content, mediaPaths);

                if (result?.Post == null || string.IsNullOrWhiteSpace(result.Post.Id))
                    throw new Exception("Tạo bài viết thất bại (Server không trả về ID bài viết).");

                _lblStatus.Text = "Đăng bài thành công.";
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(
                    this,
                    "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
                    "Hết phiên",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += "\n\nNguyên nhân gốc (Inner Exception):\n" + ex.InnerException.Message;
                }
                errorMsg += "\n\nStack Trace:\n" + ex.StackTrace;

                MessageBox.Show(
                    this,
                    "Không thể đăng bài. Chi tiết lỗi:\n\n" + errorMsg,
                    "Thông tin gỡ lỗi chi tiết",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false, "Chọn ảnh/video (nếu có) rồi bấm Đăng.");
                UpdatePostButtonState();
            }
        }

        private void SetBusy(bool busy, string status)
        {
            _lblStatus.Text = status ?? string.Empty;
            _btnPost.Enabled = !busy;
            _btnAddMedia.Enabled = !busy;
            _btnClearMedia.Enabled = !busy;
            _btnCancel.Enabled = !busy;
            _txtContent.ReadOnly = busy;
            UseWaitCursor = busy;
        }

        private void UpdatePostButtonState()
        {
            if (_btnPost == null) return;
            if (!_btnPost.Enabled && UseWaitCursor) return;

            var raw = _txtContent?.Text ?? string.Empty;
            var hasText = !string.IsNullOrWhiteSpace(raw) &&
                          !string.Equals(raw, _contentPlaceholder, StringComparison.Ordinal);

            var canPost = hasText || _selected.Count > 0;

            _btnPost.Enabled = canPost && !UseWaitCursor;
            _btnClearMedia.Enabled = _selected.Count > 0 && !UseWaitCursor;

            _btnPost.BackColor = _btnPost.Enabled
                ? Color.FromArgb(24, 119, 242)
                : Color.FromArgb(200, 224, 250);

            _btnPost.ForeColor = _btnPost.Enabled
                ? Color.White
                : Color.FromArgb(120, 120, 120);
        }

        private async Task LoadCurrentUserAsync()
        {
            try
            {
                var profile = await _userBll.GetMyProfileAsync();

                var displayName = (profile?.FullName
                                   ?? profile?.UserName
                                   ?? SessionManager.Username
                                   ?? "Bạn").Trim();

                if (string.IsNullOrWhiteSpace(displayName))
                    displayName = "Bạn";

                if (!_lblName.IsDisposed)
                    _lblName.Text = displayName;

                _contentPlaceholder = $"{displayName} ơi, bạn đang nghĩ gì thế?";

                if (!_txtContent.IsDisposed &&
                    (string.IsNullOrWhiteSpace(_txtContent.Text)
                     || string.Equals(_txtContent.Text, "Bạn đang nghĩ gì thế?", StringComparison.Ordinal)))
                {
                    _txtContent.Text = _contentPlaceholder;
                    _txtContent.ForeColor = Color.Gray;
                }

                if (profile != null && !string.IsNullOrWhiteSpace(profile.AvatarUrl))
                {
                    var img = await LoadImageFromUrl(profile.AvatarUrl);
                    if (img != null && !_picAvatar.IsDisposed)
                        _picAvatar.Image = img;
                }
            }
            catch
            {
                var displayName = string.IsNullOrWhiteSpace(SessionManager.Username)
                    ? "Bạn"
                    : SessionManager.Username;

                _contentPlaceholder = $"{displayName} ơi, bạn đang nghĩ gì thế?";
            }
        }

        private static async Task<Image> LoadImageFromUrl(string relativeOrFullUrl)
        {
            try
            {
                using var httpClient = new HttpClient();

                var fullUrl = relativeOrFullUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
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

        private static void ApplyRoundedAvatar(PictureBox pictureBox)
        {
            void UpdateRegion()
            {
                var diameter = Math.Min(pictureBox.Width, pictureBox.Height);
                var rect = new Rectangle(0, 0, diameter, diameter);

                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddEllipse(rect);
                pictureBox.Region = new Region(path);
            }

            pictureBox.SizeChanged += (_, __) => UpdateRegion();
            UpdateRegion();
        }
    }
}