using BusinessLogic;
using DataTransferObject;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Presentation.FormAdmin
{
    public class AdminReportForm : Form
    {
        private readonly ReportBLL _reportBll;
        private readonly ChatBLL _chatBll;
        private FlowLayoutPanel _pnlContent;
        private Button _btnRefresh;
        private List<ReportDTO> _cachedReports = new List<ReportDTO>();

        public AdminReportForm()
        {
            _reportBll = new ReportBLL();
            _chatBll = BusinessLogic.AppServices.ChatBll;
            Text = "Quản lý Báo cáo (Admin)";
            Size = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 246, 250);

            BuildUI();
            _ = LoadDataAsync();
        }

        private void BuildUI()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            _btnRefresh = new Button { Text = "Tải lại", Location = new Point(20, 15), Width = 100, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(255, 30, 100), ForeColor = Color.White };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.Click += async (_, __) => await LoadDataAsync();

            var lblTitle = new Label { Text = "Danh sách Báo cáo", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(140, 15), AutoSize = true };

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
            foreach (Control c in _pnlContent.Controls)
            {
                c.Width = _pnlContent.ClientSize.Width - 60; 
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _btnRefresh.Enabled = false;
                _pnlContent.Controls.Clear();
                var res = await _reportBll.GetReportsAsync();
                if (res?.reports != null)
                {
                    _cachedReports = res.reports;
                    foreach (var r in res.reports)
                    {
                        var card = CreateReportCard(r);
                        _pnlContent.Controls.Add(card);
                    }
                    CenterCards();
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải danh sách: " + ex.Message); }
            finally { _btnRefresh.Enabled = true; }
        }

        private Panel CreateReportCard(ReportDTO report)
        {
            var isPending = report.Status == "pending";
            var card = new Panel
            {
                Height = 150,
                BackColor = isPending ? Color.White : Color.FromArgb(240, 240, 240),
                Margin = new Padding(0, 0, 0, 15),
                Padding = new Padding(15)
            };

            card.Paint += (_, e) =>
            {
                var border = isPending ? Color.FromArgb(228, 228, 228) : Color.LightGray;
                var rect = card.ClientRectangle;
                rect.Width -= 1; rect.Height -= 1;
                using var pen = new Pen(border, 1);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(pen, rect);
            };

            var type = !string.IsNullOrWhiteSpace(report.TargetPostID) ? "Bài đăng" : (!string.IsNullOrWhiteSpace(report.TargetUserID) ? "Người dùng" : "Khác");
            var targetId = !string.IsNullOrWhiteSpace(report.TargetPostID) ? report.TargetPostID : report.TargetUserID;

            var lblTarget = new Label { Text = $"Đối tượng: {type} - ID: {targetId}", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true };
            var lblReason = new Label { Text = $"Lý do: {report.Reason}", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(15, 40), AutoSize = true, ForeColor = Color.FromArgb(255, 30, 100) };
            var lblDesc = new Label { Text = $"Chi tiết: {report.Description}", Font = new Font("Segoe UI", 9), Location = new Point(15, 65), AutoSize = true, MaximumSize = new Size(600, 45) };
            var lblDate = new Label { Text = $"Ngày: {report.CreatedAt.ToLocalTime():dd/MM/yyyy HH:mm}", Font = new Font("Segoe UI", 9, FontStyle.Italic), Location = new Point(15, 120), AutoSize = true, ForeColor = Color.Gray };
            var lblStatus = new Label { Text = $"Trạng thái: {report.Status}", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(200, 120), AutoSize = true, ForeColor = isPending ? Color.Orange : Color.Green };

            var btnReview = new Button
            {
                Text = isPending ? "Xử lý" : "Đã xử lý",
                Width = 100,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = isPending ? Color.FromArgb(24, 119, 242) : Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Enabled = isPending,
                Cursor = isPending ? Cursors.Hand : Cursors.Default,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right 
            };
            btnReview.FlatAppearance.BorderSize = 0;
            btnReview.Location = new Point(card.Width - btnReview.Width - 15, card.Height - btnReview.Height - 15);

            card.SizeChanged += (_, __) => {
                btnReview.Location = new Point(card.Width - btnReview.Width - 15, card.Height - btnReview.Height - 15);
            };

            btnReview.Click += (_, __) =>
            {
                using var dlg = new ReviewReportDialog(_reportBll, _chatBll, report);
                if (dlg.ShowDialog() == DialogResult.OK) _ = LoadDataAsync();
            };

            card.Controls.Add(lblTarget);
            card.Controls.Add(lblReason);
            card.Controls.Add(lblDesc);
            card.Controls.Add(lblDate);
            card.Controls.Add(lblStatus);
            card.Controls.Add(btnReview);

            return card;
        }
    }

    public class ReviewReportDialog : Form
    {
        private readonly ReportBLL _reportBll;
        private readonly ChatBLL _chatBll;
        private readonly ReportDTO _report;
        private ComboBox _cboAction;
        private TextBox _txtNote;
        private Button _btnSubmit;

        public ReviewReportDialog(ReportBLL reportBll, ChatBLL chatBll, ReportDTO report)
        {
            _reportBll = reportBll;
            _chatBll = chatBll;
            _report = report;

            Text = "Chi tiết & Xử lý báo cáo";
            Size = new Size(420, 500);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            InitComponents();
        }

        private void InitComponents()
        {
            var targetType = !string.IsNullOrWhiteSpace(_report.TargetPostID) ? "Bài đăng" : "Người dùng";
            var targetId = !string.IsNullOrWhiteSpace(_report.TargetPostID) ? _report.TargetPostID : _report.TargetUserID;
            int currentY = 20;

            var lblTarget = new Label { Text = $"Đối tượng report: {targetType}\nID: {targetId}", Location = new Point(20, currentY), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            currentY += 45;

            var lblReasonInfo = new Label { Text = $"Lý do: {_report.Reason}", Location = new Point(20, currentY), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Red };
            currentY += 30;

            var lblDescInfo = new Label { Text = $"Chi tiết:\n{_report.Description}", Location = new Point(20, currentY), AutoSize = true, MaximumSize = new Size(360, 0) };
            currentY += 65;

            var lblAction = new Label { Text = "Hành động:", Location = new Point(20, currentY), AutoSize = true };
            currentY += 22;
            _cboAction = new ComboBox { Location = new Point(20, currentY), Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboAction.Items.Add(new ComboBoxItem("Xử lý (Đồng ý báo cáo)", "approved"));
            _cboAction.Items.Add(new ComboBoxItem("Từ chối (Báo cáo sai)", "rejected"));
            _cboAction.SelectedIndex = 0;
            currentY += 40;

            var lblNote = new Label { Text = "Review note (Ghi chú xử lý):", Location = new Point(20, currentY), AutoSize = true };
            currentY += 22;
            _txtNote = new TextBox { Location = new Point(20, currentY), Width = 360, Height = 80, Multiline = true, ScrollBars = ScrollBars.Vertical };

            _btnSubmit = new Button
            {
                Text = "Xác nhận",
                Size = new Size(110, 35),
                BackColor = Color.FromArgb(24, 119, 242),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            _btnSubmit.FlatAppearance.BorderSize = 0;
            _btnSubmit.Location = new Point(this.ClientSize.Width - _btnSubmit.Width - 20, this.ClientSize.Height - _btnSubmit.Height - 20);
            _btnSubmit.Click += async (_, __) => await SubmitAsync();

            Controls.AddRange(new Control[] { lblTarget, lblReasonInfo, lblDescInfo, lblAction, _cboAction, lblNote, _txtNote, _btnSubmit });
        }

        private async Task SubmitAsync()
        {
            try
            {
                _btnSubmit.Enabled = false;
                var selectedItem = (ComboBoxItem)_cboAction.SelectedItem;

                await _reportBll.ReviewReportAsync(_report.ReportID, selectedItem.Value, _txtNote.Text);

                if (!string.IsNullOrWhiteSpace(_report.ReporterID))
                {
                    string target = !string.IsNullOrWhiteSpace(_report.TargetPostID) ? "bài đăng" : "người dùng";
                    string msg = $"Báo cáo của bạn về {target} đã được Admin phản hồi: {selectedItem.Text}.";
                    if (!string.IsNullOrWhiteSpace(_txtNote.Text)) msg += $" Ghi chú: {_txtNote.Text}";
                    await _chatBll.SendNotificationAsync(_report.ReporterID, msg);
                }

                MessageBox.Show("Đã xử lý thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xử lý: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _btnSubmit.Enabled = true;
            }
        }
    }

    public class ComboBoxItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public ComboBoxItem(string text, string value) { Text = text; Value = value; }
        public override string ToString() => Text;
    }
}