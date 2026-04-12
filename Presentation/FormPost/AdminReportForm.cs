using BusinessLogic;
using DataTransferObject;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using Presentation;
using Presentation.FormProfile;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Diagnostics;

using Color = System.Drawing.Color;
using Size = System.Drawing.Size;
using Image = System.Drawing.Image;

namespace Presentation.FormPost
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

            // Cấu hình bản quyền cho thư viện QuestPDF (sử dụng bản miễn phí Community)
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

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
            
            var btnBack = new Button { Text = "🔙 Trở về", Location = new Point(20, 15), Width = 100, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(240, 240, 240), ForeColor = Color.Black };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (_, __) => Close();
            btnBack.Cursor = Cursors.Hand;

            _btnRefresh = new Button { Text = "Tải lại", Location = new Point(130, 15), Width = 100, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(255, 30, 100), ForeColor = Color.White };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.Click += async (_, __) => await LoadDataAsync();
            _btnRefresh.Cursor = Cursors.Hand;

            var lblTitle = new Label { Text = "Danh sách Báo cáo", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(250, 15), AutoSize = true };

            pnlTop.Controls.Add(btnBack);
            pnlTop.Controls.Add(_btnRefresh);
            pnlTop.Controls.Add(lblTitle);

            // Footer
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(10), BackColor = Color.WhiteSmoke };
            var line = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.LightGray };
            pnlFooter.Controls.Add(line);

            var btnExport = new Button { Text = "Xem trước CSV", Width = 140, Height = 35, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(10, 130, 60), ForeColor = Color.White };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += (_, __) => ExportReportsToCsv();
            btnExport.Cursor = Cursors.Hand;

            var btnExportPdf = new Button { Text = "Xem trước PDF", Width = 140, Height = 35, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(180, 50, 50), ForeColor = Color.White };
            btnExportPdf.FlatAppearance.BorderSize = 0;
            btnExportPdf.Click += (_, __) => ExportReportsToPdf();
            btnExportPdf.Cursor = Cursors.Hand;

            pnlFooter.Controls.AddRange(new Control[] { btnExport, btnExportPdf });
            btnExportPdf.BringToFront();
            btnExport.BringToFront();

            pnlFooter.Resize += (s, e) =>
            {
                btnExportPdf.Location = new Point(pnlFooter.ClientSize.Width - btnExportPdf.Width - 15, (pnlFooter.ClientSize.Height - btnExportPdf.Height) / 2);
                btnExport.Location = new Point(btnExportPdf.Left - btnExport.Width - 10, (pnlFooter.ClientSize.Height - btnExport.Height) / 2);
            };

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
            Controls.Add(pnlFooter);
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

            var lblTarget = new LinkLabel
            {
                Text = $"Đối tượng: {type} - ID: {targetId}",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(15, 15),
                AutoSize = true,
                LinkBehavior = LinkBehavior.HoverUnderline,
                LinkColor = Color.FromArgb(30, 30, 30),
                ActiveLinkColor = Color.FromArgb(24, 119, 242)
            };

            if (type != "Khác")
            {
                lblTarget.Click += (_, __) =>
                {
                    if (!string.IsNullOrWhiteSpace(report.TargetPostID))
                    {
                        var postBll = BusinessLogic.AppServices.PostBll;
                        using var frm = new PostDetailForm(postBll, report.TargetPostID);
                        frm.ShowDialog(this);
                    }
                    else if (!string.IsNullOrWhiteSpace(report.TargetUserID))
                    {
                        var userProfileForm = new UserProfile(report.TargetUserID, this);
                        userProfileForm.FormClosed += (s, ev) => { this.Show(); };
                        userProfileForm.Show();
                        this.Hide();
                    }
                };
            }

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

        private void ExportReportsToCsv()
        {
            if (_cachedReports == null || _cachedReports.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu báo cáo để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("ID,ReporterID,TargetUserID,TargetPostID,Reason,Description,Status,ResolveNote,CreatedAt");

                foreach (var r in _cachedReports)
                {
                    var line = $"{r.ReportID},{r.ReporterID},{r.TargetUserID},{r.TargetPostID},{EscapeCsvField(r.Reason)},{EscapeCsvField(r.Description)},{r.Status},{EscapeCsvField(r.ResolveNote)},{r.CreatedAt.ToLocalTime():yyyy-MM-dd HH:mm:ss}";
                    sb.AppendLine(line);
                }

                using var previewForm = new Presentation.Reports.TextPreviewForm(sb.ToString(), $"Report_List_{DateTime.Now:yyyyMMdd_HHmm}.csv");
                previewForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tạo dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }

        private void ExportReportsToPdf()
        {
            if (_cachedReports == null || _cachedReports.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu báo cáo để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30);
                        
                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("DANH SÁCH BÁO CÁO VI PHẠM").Style(TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Red.Darken2));
                                col.Item().Text($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}").Style(TextStyle.Default.FontSize(10).FontColor(Colors.Grey.Darken1));
                                col.Item().Text($"Tổng số lượng: {_cachedReports.Count} báo cáo").Style(TextStyle.Default.FontSize(10).FontColor(Colors.Grey.Darken1));
                            });
                        });

                        page.Content().PaddingVertical(15).Column(column =>
                        {
                            column.Spacing(15);
                            foreach (var r in _cachedReports)
                            {
                                var isPending = r.Status == "pending";
                                var statusColor = isPending ? Colors.Orange.Medium : (r.Status == "approved" ? Colors.Green.Medium : Colors.Red.Medium);
                                var statusText = isPending ? "ĐANG CHỜ XỬ LÝ" : (r.Status == "approved" ? "ĐÃ DUYỆT (ĐỒNG Ý)" : "ĐÃ TỪ CHỐI");
                                
                                var type = !string.IsNullOrWhiteSpace(r.TargetPostID) ? "Bài đăng" : (!string.IsNullOrWhiteSpace(r.TargetUserID) ? "Người dùng" : "Khác");
                                var targetId = !string.IsNullOrWhiteSpace(r.TargetPostID) ? r.TargetPostID : r.TargetUserID;

                                column.Item().Background(Colors.White)
                                    .Border(1).BorderColor(isPending ? Colors.Blue.Lighten2 : Colors.Grey.Lighten2)
                                    .Padding(12)
                                    .Column(card =>
                                    {
                                        card.Item().Row(row =>
                                        {
                                            row.RelativeItem().Text($"Mã báo cáo: {r.ReportID}").FontSize(9).FontColor(Colors.Grey.Medium);
                                            row.ConstantItem(120).AlignRight().Text(statusText).FontSize(10).Bold().FontColor(statusColor);
                                        });

                                        card.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten3);

                                        card.Item().PaddingTop(8).Grid(grid =>
                                        {
                                            grid.Columns(12);
                                            grid.Spacing(5);

                                            grid.Item(3).Text("Người báo cáo:").SemiBold().FontSize(10);
                                            grid.Item(9).Text(r.ReporterID).FontSize(10);

                                            grid.Item(3).Text("Đối tượng vi phạm:").SemiBold().FontSize(10);
                                            grid.Item(9).Text($"{type} ({targetId})").FontSize(10);

                                            grid.Item(3).Text("Ngày báo cáo:").SemiBold().FontSize(10);
                                            grid.Item(9).Text(r.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss")).FontSize(10);

                                            grid.Item(3).Text("Lý do:").SemiBold().FontSize(10).FontColor(Colors.Red.Medium);
                                            grid.Item(9).Text(r.Reason).FontSize(10).Bold().FontColor(Colors.Red.Medium);

                                            grid.Item(3).Text("Chi tiết:").SemiBold().FontSize(10);
                                            grid.Item(9).Text(string.IsNullOrWhiteSpace(r.Description) ? "Không có" : r.Description).FontSize(10).Italic();
                                        });

                                        if (!isPending)
                                        {
                                            card.Item().PaddingTop(10).Background(Colors.Green.Lighten5).Padding(8).Column(noteCol =>
                                            {
                                                noteCol.Item().Text("Ghi chú xử lý từ Quản trị viên:").SemiBold().FontSize(10).FontColor(Colors.Green.Darken2);
                                                noteCol.Item().PaddingTop(2).Text(string.IsNullOrWhiteSpace(r.ResolveNote) ? "Không có" : r.ResolveNote).FontSize(10).FontColor(Colors.Green.Darken3);
                                            });
                                        }
                                    });
                            }
                        });
                        page.Footer().AlignCenter().Text(text => { text.CurrentPageNumber(); text.Span(" / "); text.TotalPages(); });
                    });
                });

                using var previewForm = new Presentation.Reports.PdfPreviewForm(document, $"Report_List_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
                previewForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tạo báo cáo PDF: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            Size = new Size(460, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            InitComponents();
        }

        private void InitComponents()
        {
            var isPending = _report.Status == "pending";
            var targetType = !string.IsNullOrWhiteSpace(_report.TargetPostID) ? "Bài đăng" : "Người dùng";
            var targetId = !string.IsNullOrWhiteSpace(_report.TargetPostID) ? _report.TargetPostID : _report.TargetUserID;
            int currentY = 20;

            var lblTarget = new Label { Text = $"Đối tượng: {targetType}\nID: {targetId}", Location = new Point(20, currentY), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            currentY += 45;

            var lblReporter = new Label { Text = $"Người báo cáo: {_report.ReporterID}", Location = new Point(20, currentY), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.DimGray };
            currentY += 25;

            var lblReasonInfo = new Label { Text = $"Lý do: {_report.Reason}", Location = new Point(20, currentY), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Red };
            currentY += 30;

            var lblDescInfo = new Label { Text = $"Chi tiết:\n{_report.Description}", Location = new Point(20, currentY), AutoSize = true, MaximumSize = new Size(400, 0) };
            currentY += lblDescInfo.PreferredHeight + 15;

            if (!isPending)
            {
                var pnlNote = new Panel { Location = new Point(20, currentY), Width = 400, AutoSize = true, BackColor = Color.FromArgb(235, 245, 235), Padding = new Padding(10) };
                var lblStatus = new Label { Text = $"Trạng thái: {_report.Status.ToUpper()}", Location = new Point(10, 10), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = _report.Status == "approved" ? Color.Green : Color.DarkRed };
                var lblNoteTitle = new Label { Text = "Ghi chú xử lý:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.DarkGreen, Location = new Point(10, 35), AutoSize = true };
                var lblNoteContent = new Label { Text = string.IsNullOrWhiteSpace(_report.ResolveNote) ? "Không có" : _report.ResolveNote, Font = new Font("Segoe UI", 9.5f, FontStyle.Italic), ForeColor = Color.DarkGreen, Location = new Point(10, 55), AutoSize = true, MaximumSize = new Size(380, 0) };
                
                pnlNote.Controls.Add(lblStatus);
                pnlNote.Controls.Add(lblNoteTitle);
                pnlNote.Controls.Add(lblNoteContent);
                Controls.Add(pnlNote);
                
                currentY += pnlNote.PreferredSize.Height + 20;
            }

            var lblAction = new Label { Text = "Hành động:", Location = new Point(20, currentY), AutoSize = true };
            currentY += 22;
            _cboAction = new ComboBox { Location = new Point(20, currentY), Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboAction.Items.Add(new ComboBoxItem("Xử lý (Đồng ý báo cáo)", "approved"));
            _cboAction.Items.Add(new ComboBoxItem("Từ chối (Báo cáo sai)", "rejected"));
            _cboAction.SelectedIndex = 0;
            _cboAction.Enabled = isPending;
            currentY += 40;

            var lblNote = new Label { Text = "Review note (Ghi chú xử lý):", Location = new Point(20, currentY), AutoSize = true };
            currentY += 22;
            _txtNote = new TextBox { Location = new Point(20, currentY), Width = 400, Height = 80, Multiline = true, ScrollBars = ScrollBars.Vertical };
            _txtNote.Enabled = isPending;
            if (!isPending) _txtNote.Text = _report.ResolveNote;
            currentY += 100;

            _btnSubmit = new Button
            {
                Text = isPending ? "Xác nhận" : "Đóng",
                Size = new Size(110, 35),
                BackColor = isPending ? Color.FromArgb(24, 119, 242) : Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            _btnSubmit.FlatAppearance.BorderSize = 0;
            
            if (isPending)
                _btnSubmit.Click += async (_, __) => await SubmitAsync();
            else
                _btnSubmit.Click += (_, __) => Close();

            Controls.AddRange(new Control[] { lblTarget, lblReporter, lblReasonInfo, lblDescInfo, lblAction, _cboAction, lblNote, _txtNote, _btnSubmit });
            
            this.Height = currentY + 80;
            _btnSubmit.Location = new Point(this.ClientSize.Width - _btnSubmit.Width - 20, this.ClientSize.Height - _btnSubmit.Height - 20);
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