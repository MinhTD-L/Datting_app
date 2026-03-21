using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessLogic;

namespace Presentation.FormReport
{
    public class SubmitReportForm : Form
    {
        private readonly ReportBLL _reportBll;
        private readonly string _type;
        private readonly string _targetId;

        private ComboBox _cboReason;
        private TextBox _txtDesc;
        private Button _btnSubmit;
        private Button _btnCancel;

        public SubmitReportForm(string type, string targetId)
        {
            _reportBll = new ReportBLL();
            _type = type; // "user" or "post"
            _targetId = targetId;

            Text = type == "user" ? "Báo cáo người dùng" : "Báo cáo bài viết";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(400, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;

            BuildUI();
        }

        private void BuildUI()
        {
            var lblTitle = new Label
            {
                Text = Text,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(16, 16),
                AutoSize = true
            };

            var lblTarget = new Label
            {
                Text = $"Đối tượng: {(_type == "user" ? "Người dùng" : "Bài đăng")} ({_targetId})",
                Location = new Point(16, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };

            var lblReason = new Label
            {
                Text = "Lý do:",
                Location = new Point(16, 80),
                AutoSize = true
            };

            _cboReason = new ComboBox
            {
                Location = new Point(16, 100),
                Width = 350,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cboReason.Items.AddRange(new[] { "Spam", "Nội dung phản cảm", "Lừa đảo", "Ngôn từ thù ghét", "Khác" });
            _cboReason.SelectedIndex = 0;

            var lblDesc = new Label
            {
                Text = "Miêu tả cụ thể:",
                Location = new Point(16, 140),
                AutoSize = true
            };

            _txtDesc = new TextBox
            {
                Location = new Point(16, 160),
                Width = 350,
                Height = 80,
                Multiline = true
            };

            _btnCancel = new Button { Text = "Hủy", Location = new Point(206, 250), Width = 80, Height = 30, FlatStyle = FlatStyle.Flat };
            _btnCancel.Click += (_, __) => Close();

            _btnSubmit = new Button { Text = "Report", Location = new Point(296, 250), Width = 70, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(255, 30, 100), ForeColor = Color.White };
            _btnSubmit.FlatAppearance.BorderSize = 0;
            _btnSubmit.Click += async (_, __) => await SubmitAsync();

            Controls.Add(lblTitle); Controls.Add(lblTarget); Controls.Add(lblReason); Controls.Add(_cboReason);
            Controls.Add(lblDesc); Controls.Add(_txtDesc); Controls.Add(_btnCancel); Controls.Add(_btnSubmit);
        }

        private async Task SubmitAsync()
        {
            if (string.IsNullOrWhiteSpace(_targetId))
            {
                MessageBox.Show("ID đối tượng không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var reason = _cboReason.SelectedItem?.ToString();
            var desc = _txtDesc.Text.Trim();
            _btnSubmit.Enabled = false; _btnCancel.Enabled = false;

            try
            {
                if (_type == "user") await _reportBll.ReportUserAsync(_targetId, reason, desc);
                else await _reportBll.ReportPostAsync(_targetId, reason, desc);

                MessageBox.Show("Đã gửi báo cáo thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { _btnSubmit.Enabled = true; _btnCancel.Enabled = true; }
        }
    }
}