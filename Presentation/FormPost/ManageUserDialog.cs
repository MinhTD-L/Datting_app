using BusinessLogic;
using DataTransferObject;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Presentation.FormPost
{
    public class ManageUserDialog : Form
    {
        private readonly AdminBLL _adminBll;
        private readonly UserAdminViewDTO _user;

        private CheckBox _chkBanMatch;
        private CheckBox _chkBanPost;
        private CheckBox _chkBanComment;
        private TextBox _txtRestrictionReason;
        private Button _btnBan;
        private Button _btnSave;

        public ManageUserDialog(UserAdminViewDTO user)
        {
            _adminBll = BusinessLogic.AppServices.AdminBll;
            _user = user ?? throw new ArgumentNullException(nameof(user));

            Text = $"Quản lý: {_user.Username}";
            Size = new Size(450, 450);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;

            BuildUI();
            BindData();
        }

        private void BuildUI()
        {
            var lblTitle = new Label { Text = $"Quản lý người dùng: {_user.Username}", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            var lblEmail = new Label { Text = _user.Email, Font = new Font("Segoe UI", 9, FontStyle.Italic), Location = new Point(20, 50), AutoSize = true, ForeColor = Color.Gray };

            var grpRestrictions = new GroupBox { Text = "Hạn chế tính năng", Location = new Point(20, 90), Width = 390, Height = 160 };
            _chkBanMatch = new CheckBox { Text = "Cấm ghép đôi (match)", Location = new Point(20, 30), AutoSize = true };
            _chkBanPost = new CheckBox { Text = "Cấm đăng bài (post)", Location = new Point(20, 60), AutoSize = true };
            _chkBanComment = new CheckBox { Text = "Cấm bình luận (comment)", Location = new Point(20, 90), AutoSize = true };
            var lblReason = new Label { Text = "Lý do hạn chế (để trống nếu mặc định):", Location = new Point(20, 120), AutoSize = true };
            _txtRestrictionReason = new TextBox { PlaceholderText = "Vi phạm chính sách cộng đồng", Location = new Point(18, 140), Width = 350 };

            grpRestrictions.Controls.AddRange(new Control[] { _chkBanMatch, _chkBanPost, _chkBanComment, lblReason, _txtRestrictionReason });

            _btnSave = new Button { Text = "Lưu Hạn chế", Location = new Point(290, 260), Width = 120, Height = 35, BackColor = Color.DodgerBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += async (_, __) => await SaveRestrictionsAsync();

            var grpAccountStatus = new GroupBox { Text = "Trạng thái tài khoản", Location = new Point(20, 305), Width = 390, Height = 70 };
            _btnBan = new Button { Width = 150, Height = 35, Location = new Point(20, 25) };
            _btnBan.Click += async (_, __) => await ToggleBanStatusAsync();
            grpAccountStatus.Controls.Add(_btnBan);

            var btnClose = new Button { Text = "Đóng", Location = new Point(330, 385), Width = 80, Height = 30 };
            btnClose.Click += (_, __) => this.Close();

            Controls.AddRange(new Control[] { lblTitle, lblEmail, grpRestrictions, _btnSave, grpAccountStatus, btnClose });
        }

        private void BindData()
        {
            if (_user.Restrictions != null)
            {
                var features = _user.GetRestrictionFeatures();
                _chkBanMatch.Checked = features.Contains("match");
                _chkBanPost.Checked = features.Contains("post");
                _chkBanComment.Checked = features.Contains("comment");

                var firstReason = _user.Restrictions.FirstOrDefault()?.Reason;
                if (!string.IsNullOrWhiteSpace(firstReason) && firstReason != "vi phạm chính sách cộng đồng")
                {
                    _txtRestrictionReason.Text = firstReason;
                }
            }
 
            UpdateBanButton();
        }

        private void UpdateBanButton()
        {
            bool isBanned = string.Equals(_user.Status, "banned", StringComparison.OrdinalIgnoreCase);
            _btnBan.Text = isBanned ? "Bỏ cấm tài khoản" : "Cấm tài khoản";
            _btnBan.BackColor = isBanned ? Color.ForestGreen : Color.Crimson;
            _btnBan.ForeColor = Color.White;
            _btnBan.FlatStyle = FlatStyle.Flat;
            _btnBan.FlatAppearance.BorderSize = 0;
        }

        private async Task SaveRestrictionsAsync()
        {
            var restrictions = new List<string>();
            if (_chkBanMatch.Checked) restrictions.Add("match");
            if (_chkBanPost.Checked) restrictions.Add("post");
            if (_chkBanComment.Checked) restrictions.Add("comment");

            var reason = _txtRestrictionReason.Text.Trim();

            try
            {
                this.Enabled = false;
                await _adminBll.UpdateUserRestrictionsAsync(_user.Id, restrictions, reason);
                _user.Restrictions = restrictions.Select(feature => new RestrictionInfoDTO
                {
                    Feature = feature,
                    Reason = string.IsNullOrWhiteSpace(reason) ? "vi phạm chính sách cộng đồng" : reason,
                    ExpiresAt = DateTime.UtcNow.AddDays(7) 
                }).ToList();

                MessageBox.Show("Cập nhật hạn chế thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật hạn chế: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Enabled = true;
            }
        }

        private async Task ToggleBanStatusAsync()
        {
            bool isBanned = string.Equals(_user.Status, "banned", StringComparison.OrdinalIgnoreCase);
            var actionText = isBanned ? "bỏ cấm" : "cấm";
            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn {actionText} người dùng '{_user.Username}' không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                this.Enabled = false;
                if (isBanned)
                {
                    await _adminBll.UnbanUserAsync(_user.Id);
                    _user.Status = "active";
                }
                else
                {
                    await _adminBll.BanUserAsync(_user.Id);
                    _user.Status = "banned";
                }
                
                MessageBox.Show($"Đã {actionText} người dùng thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateBanButton();
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi {actionText} người dùng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Enabled = true;
            }
        }
    }
}