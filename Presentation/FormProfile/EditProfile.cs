using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Linq;
using BusinessLogic;
using DataTransferObject;

namespace Presentation.FormProfile
{
    public partial class EditProfile : Form
    {
        private readonly UserBLL _userBll;
        private readonly UserProfileDTO _seed;

        private PictureBox _avatar;
        private TextBox _txtFullName;
        private ComboBox _cboGender;
        private DateTimePicker _dtDob;
        private TextBox _txtBio;
        private TextBox _txtTags;
        private Button _btnSave;
        private Button _btnCancel;
        private Button _btnPickAvatar;
        private Label _lblStatus;
        private string _pickedAvatarPath;
        private bool _avatarDirty;

        public EditProfile(UserBLL userBll, UserProfileDTO seed)
        {
            InitializeComponent();
            _userBll = userBll ?? throw new System.ArgumentNullException(nameof(userBll));
            _seed = seed;

            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;

            BuildUi();
            BindSeed();
        }

        private void BuildUi()
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 62,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 10)
            };

            var lblTitle = new Label
            {
                Text = "Chỉnh sửa hồ sơ",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, 0)
            };
            header.Controls.Add(lblTitle);

            var divider = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = Color.FromArgb(235, 235, 235)
            };
            header.Controls.Add(divider);

            var body = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 12),
                AutoScroll = true
            };

            var avatarRow = new Panel { Dock = DockStyle.Top, Height = 120 };
            _avatar = new PictureBox
            {
                Width = 92,
                Height = 92,
                BackColor = Color.FromArgb(235, 235, 235),
                SizeMode = PictureBoxSizeMode.Zoom,
                Left = 0,
                Top = 8
            };
            ApplyRounded(_avatar);

            var lblAvatar = new Label
            {
                Text = "Avatar",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Left = _avatar.Right + 14,
                Top = 10
            };

            _btnPickAvatar = new Button
            {
                Text = "Chọn ảnh…",
                Width = 120,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(245, 246, 250),
                ForeColor = Color.FromArgb(45, 45, 45),
                Left = _avatar.Right + 14,
                Top = lblAvatar.Bottom + 10,
            };
            _btnPickAvatar.FlatAppearance.BorderColor = Color.FromArgb(230, 230, 230);
            _btnPickAvatar.Click += async (_, __) => await PickAvatarAsync();

            avatarRow.Controls.Add(_avatar);
            avatarRow.Controls.Add(lblAvatar);
            avatarRow.Controls.Add(_btnPickAvatar);

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 3,
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 0)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var fullNameWrap = BuildLabeledText("Họ tên", out _txtFullName);
            _txtFullName.Font = new Font("Segoe UI", 10);

            var genderWrap = BuildLabeledCombo("Giới tính", out _cboGender, new[] { "male", "female", "other" });
            _cboGender.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboGender.Font = new Font("Segoe UI", 10);

            var dobWrap = BuildLabeledDate("Ngày sinh", out _dtDob);
            _dtDob.Font = new Font("Segoe UI", 10);
            _dtDob.Format = DateTimePickerFormat.Custom;
            _dtDob.CustomFormat = "yyyy-MM-dd";
            _dtDob.ShowCheckBox = true;

            grid.Controls.Add(fullNameWrap, 0, 0);
            grid.SetColumnSpan(fullNameWrap, 2);
            grid.Controls.Add(genderWrap, 0, 1);
            grid.Controls.Add(dobWrap, 1, 1);

            var bioWrap = new Panel { Dock = DockStyle.Top, Height = 160, Padding = new Padding(0, 10, 0, 0) };
            var lblBio = new Label
            {
                Text = "Giới thiệu",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Dock = DockStyle.Top
            };
            _txtBio = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10),
                Height = 120,
                Dock = DockStyle.Top
            };
            bioWrap.Controls.Add(_txtBio);
            bioWrap.Controls.Add(lblBio);

            var tagsWrap = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(0, 10, 0, 0) };
            var lblTags = new Label
            {
                Text = "Sở thích / Tags (cách nhau bởi dấu phẩy)",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Dock = DockStyle.Top
            };
            _txtTags = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Top
            };
            tagsWrap.Controls.Add(_txtTags);
            tagsWrap.Controls.Add(lblTags);

            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(16, 10, 16, 10)
            };

            _lblStatus = new Label
            {
                Dock = DockStyle.Top,
                Height = 18,
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var btnRow = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            _btnSave = new Button
            {
                Text = "Lưu",
                Width = 120,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(24, 119, 242),
                ForeColor = Color.White
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += async (_, __) => await SaveAsync();

            _btnCancel = new Button
            {
                Text = "Huỷ",
                Width = 120,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(245, 246, 250),
                ForeColor = Color.FromArgb(45, 45, 45)
            };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Click += (_, __) => { DialogResult = DialogResult.Cancel; Close(); };

            btnRow.Controls.Add(_btnCancel);
            btnRow.Controls.Add(_btnSave);
            btnRow.SizeChanged += (_, __) =>
            {
                _btnSave.Left = btnRow.ClientSize.Width - _btnSave.Width;
                _btnCancel.Left = _btnSave.Left - _btnCancel.Width - 10;
                _btnSave.Top = 2;
                _btnCancel.Top = 2;
            };

            footer.Controls.Add(btnRow);
            footer.Controls.Add(_lblStatus);

            body.Controls.Add(tagsWrap);
            body.Controls.Add(bioWrap);
            body.Controls.Add(grid);
            body.Controls.Add(avatarRow);

            Controls.Add(body);
            Controls.Add(footer);
            Controls.Add(header);
        }

        private void BindSeed()
        {
            _txtFullName.Text = _seed?.FullName ?? "";
            _txtBio.Text = _seed?.Bio ?? "";
            _txtTags.Text = _seed?.Tags != null ? string.Join(", ", _seed.Tags) : "";

            var g = (_seed?.Gender ?? "").Trim();
            if (g == "") g = "other";
            var idx = _cboGender.Items.IndexOf(g);
            _cboGender.SelectedIndex = idx >= 0 ? idx : 0;

            if (_seed?.DateOfBirth != null)
            {
                _dtDob.Value = _seed.DateOfBirth.Value.Date;
                _dtDob.Checked = true;
            }
            else
            {
                _dtDob.Value = System.DateTime.Today.AddYears(-18);
                _dtDob.Checked = false;
            }

            _pickedAvatarPath = null;
            _avatarDirty = false;
            _ = RefreshAvatarPreviewAsync(_seed?.AvatarUrl);

            _lblStatus.Text = "Cập nhật thông tin rồi bấm Lưu.";
        }

        private async Task SaveAsync()
        {
            SetBusy(true, "Đang lưu...");
            try
            {
                string avatarUrl = _seed?.AvatarUrl;
                if (_avatarDirty && !string.IsNullOrWhiteSpace(_pickedAvatarPath))
                {
                    _lblStatus.Text = "Đang upload avatar...";
                    avatarUrl = await _userBll.UploadAvatarAsync(_pickedAvatarPath);
                }

                var tagsList = _txtTags.Text?.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();

                var dto = new SetupProfileDTO
                {
                    FullName = _txtFullName.Text?.Trim(),
                    AvatarUrl = avatarUrl?.Trim(),
                    Gender = _cboGender.SelectedItem?.ToString(),
                    DateOfBirth = _dtDob.Checked ? _dtDob.Value.Date : null,
                    Bio = _txtBio.Text?.Trim(),
                    Tags = tagsList
                };

                await _userBll.SetupProfileAsync(dto);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show(this, "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", "Hết phiên",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.Cancel;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, "Không thể lưu: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetBusy(false, "Vui lòng kiểm tra lại thông tin.");
            }
        }

        private void SetBusy(bool busy, string status)
        {
            _lblStatus.Text = status ?? "";
            _btnSave.Enabled = !busy;
            _btnCancel.Enabled = !busy;
            _btnPickAvatar.Enabled = !busy;
            _txtFullName.Enabled = !busy;
            _cboGender.Enabled = !busy;
            _dtDob.Enabled = !busy;
            _txtBio.Enabled = !busy;
            _txtTags.Enabled = !busy;
            UseWaitCursor = busy;
        }

        private static Panel BuildLabeledText(string label, out TextBox textBox)
        {
            var wrap = new Panel { Dock = DockStyle.Fill, Height = 66, Margin = new Padding(0, 0, 10, 10) };
            var lbl = new Label
            {
                Text = label,
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(0, 0)
            };
            textBox = new TextBox { Width = 300, Location = new Point(0, lbl.Bottom + 6) };
            textBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            wrap.Controls.Add(lbl);
            wrap.Controls.Add(textBox);
            return wrap;
        }

        private static Panel BuildLabeledCombo(string label, out ComboBox combo, string[] items)
        {
            var wrap = new Panel { Dock = DockStyle.Fill, Height = 66, Margin = new Padding(0, 0, 10, 10) };
            var lbl = new Label
            {
                Text = label,
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(0, 0)
            };
            combo = new ComboBox { Width = 300, Location = new Point(0, lbl.Bottom + 6) };
            combo.Items.AddRange(items);
            combo.SelectedIndex = 0;
            combo.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            wrap.Controls.Add(lbl);
            wrap.Controls.Add(combo);
            return wrap;
        }

        private static Panel BuildLabeledDate(string label, out DateTimePicker picker)
        {
            var wrap = new Panel { Dock = DockStyle.Fill, Height = 66, Margin = new Padding(10, 0, 0, 10) };
            var lbl = new Label
            {
                Text = label,
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(0, 0)
            };
            picker = new DateTimePicker { Width = 300, Location = new Point(0, lbl.Bottom + 6) };
            picker.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            wrap.Controls.Add(lbl);
            wrap.Controls.Add(picker);
            return wrap;
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

        private static async Task<Image> TryLoadImageAsync(string relativeOrFullUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeOrFullUrl)) return null;
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

        private async Task RefreshAvatarPreviewAsync(string url)
        {
            var img = await TryLoadImageAsync(url);
            if (img != null && !_avatar.IsDisposed)
                _avatar.Image = img;
        }

        private async Task PickAvatarAsync()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Chọn ảnh avatar",
                Filter = "Images|*.jpg;*.jpeg;*.png;*.gif;*.webp;*.bmp|All files|*.*",
                Multiselect = false
            };

            if (ofd.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                _pickedAvatarPath = ofd.FileName;
                _avatarDirty = true;
                var img = await TryLoadLocalImageAsync(_pickedAvatarPath);
                if (img != null && !_avatar.IsDisposed)
                    _avatar.Image = img;

                _lblStatus.Text = "Đã chọn avatar. Bấm Lưu để cập nhật.";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, "Không thể load ảnh: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _lblStatus.Text = "Không thể load ảnh.";
            }
        }

        private static Task<Image> TryLoadLocalImageAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                    return Task.FromResult<Image>(null);

                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var img = Image.FromStream(fs);
                return Task.FromResult<Image>(new Bitmap(img));
            }
            catch
            {
                return Task.FromResult<Image>(null);
            }
        }
    }
}
