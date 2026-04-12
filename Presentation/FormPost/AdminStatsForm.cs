using BusinessLogic;
using DataTransferObject;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuestPDF.Fluent;
using System.Diagnostics;
using System.IO;
using Presentation.Reports;
using System.Windows.Forms.DataVisualization.Charting;
// Thêm using cho Crystal Reports ở file StatsReportViewerForm.cs

namespace Presentation.FormPost
{
    public class AdminStatsForm : Form
    {
        private readonly AdminBLL _adminBll;
        private TabControl _tabControl;

        // Tab 1: Overview
        private FlowLayoutPanel _pnlOverview;

        // Tab 2: Time Series
        private Chart _chart;
        private ComboBox _cboMetric;
        private DateTimePicker _dtpStart;
        private DateTimePicker _dtpEnd;

        // Tab 3: Popular Tags
        private DataGridView _dgvTags;

        public AdminStatsForm()
        {
            _adminBll = BusinessLogic.AppServices.AdminBll;

            // Cấu hình bản quyền cho thư viện QuestPDF (sử dụng bản miễn phí Community)
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            Text = "Thống kê hệ thống";
            Size = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 246, 250);

            BuildUI();
            _ = LoadAllDataAsync();
        }

        private void BuildUI()
        {
            // Footer panel cho các nút xuất báo cáo
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(10), BackColor = Color.WhiteSmoke };
            var line = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.LightGray };
            pnlFooter.Controls.Add(line);

            var btnExportCsv = new Button { Text = "Xem trước CSV", Width = 140, Height = 35, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(10, 130, 60), ForeColor = Color.White };
            btnExportCsv.FlatAppearance.BorderSize = 0;
            btnExportCsv.Cursor = Cursors.Hand;
            btnExportCsv.Click += async (_, __) => await ExportAllToCsvAsync();

            var btnExportPdf = new Button { Text = "Xem trước PDF", Width = 140, Height = 35, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(180, 50, 50), ForeColor = Color.White };
            btnExportPdf.FlatAppearance.BorderSize = 0;
            btnExportPdf.Cursor = Cursors.Hand;
            btnExportPdf.Click += async (_, __) => await ExportToPdfReportAsync();

            pnlFooter.Controls.AddRange(new Control[] { btnExportCsv, btnExportPdf });
            btnExportPdf.BringToFront();
            btnExportCsv.BringToFront();

            pnlFooter.Resize += (s, e) => {
                btnExportPdf.Location = new Point(pnlFooter.ClientSize.Width - btnExportPdf.Width - 15, (pnlFooter.ClientSize.Height - btnExportPdf.Height) / 2);
                btnExportCsv.Location = new Point(btnExportPdf.Left - btnExportCsv.Width - 10, (pnlFooter.ClientSize.Height - btnExportCsv.Height) / 2);
            };

            _tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };

            var tabOverview = new TabPage("Tổng quan");
            BuildOverviewTab(tabOverview);
            _tabControl.TabPages.Add(tabOverview);

            var tabTimeSeries = new TabPage("Biểu đồ theo thời gian");
            BuildTimeSeriesTab(tabTimeSeries);
            _tabControl.TabPages.Add(tabTimeSeries);

            var tabTags = new TabPage("Tags phổ biến");
            BuildTagsTab(tabTags);
            _tabControl.TabPages.Add(tabTags);

            Controls.Add(_tabControl);
            Controls.Add(pnlFooter);
        }

        private void BuildOverviewTab(TabPage parent)
        {
            _pnlOverview = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.FromArgb(245, 246, 250)
            };
            _pnlOverview.SizeChanged += (s, e) => {
                foreach (Control c in _pnlOverview.Controls) {
                    c.Width = _pnlOverview.ClientSize.Width - 40;
                    if (c.Controls.Count > 1 && c.Controls[1] is FlowLayoutPanel flp) {
                        flp.Width = c.Width;
                    }
                }
            };
            var lblLoading = new Label { Text = "Đang tải...", AutoSize = true, Margin = new Padding(10) };
            _pnlOverview.Controls.Add(lblLoading);
            parent.Controls.Add(_pnlOverview);
        }

        private void BuildTimeSeriesTab(TabPage parent)
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };
            _cboMetric = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(10, 10), Width = 150 };
            _cboMetric.Items.AddRange(new object[] {
                new KeyValuePair<string, string>("Người dùng mới", "new_users"),
                new KeyValuePair<string, string>("Ghép đôi thành công", "successful_pairs"),
                new KeyValuePair<string, string>("Bài đăng mới", "new_posts"),
                new KeyValuePair<string, string>("Báo cáo mới", "new_reports")
            });
            _cboMetric.DisplayMember = "Key";
            _cboMetric.ValueMember = "Value";
            _cboMetric.SelectedIndex = 0;

            _dtpStart = new DateTimePicker { Location = new Point(170, 10), Value = DateTime.Now.AddDays(-30) };
            _dtpEnd = new DateTimePicker { Location = new Point(380, 10) };
            var btnLoadChart = new Button { Text = "Tải", Location = new Point(590, 9), Width = 80 };
            btnLoadChart.Click += async (_, __) => await LoadChartDataAsync();

            pnlTop.Controls.AddRange(new Control[] { _cboMetric, _dtpStart, _dtpEnd, btnLoadChart });

            _chart = new Chart { Dock = DockStyle.Fill };
            var chartArea = new ChartArea("MainArea");
            _chart.ChartAreas.Add(chartArea);
            _chart.Series.Add(new Series("Data") { ChartType = SeriesChartType.Line, BorderWidth = 2 });

            parent.Controls.Add(_chart);
            parent.Controls.Add(pnlTop);
        }

        private void BuildTagsTab(TabPage parent)
        {
            _dgvTags = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            var pnlTagsTop = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };
            var btnExportTags = new Button { Text = "Xem trước CSV", Location = new Point(10, 9), Width = 130, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(20, 150, 80), ForeColor = Color.White };
            btnExportTags.FlatAppearance.BorderSize = 0;
            btnExportTags.Click += (_, __) => ExportDataGridViewToCsv(_dgvTags, "Popular_Tags");
            pnlTagsTop.Controls.Add(btnExportTags);

            parent.Controls.Add(_dgvTags);
            parent.Controls.Add(pnlTagsTop);
        }

        private async Task LoadAllDataAsync()
        {
            await LoadOverviewDataAsync();
            await LoadChartDataAsync();
            await LoadTagsDataAsync();
        }

        private async Task LoadOverviewDataAsync()
        {
            try
            {
                var stats = await _adminBll.GetDashboardStatsAsync();
                _pnlOverview.Controls.Clear();

                _pnlOverview.Controls.Add(CreateStatGroup("👥 NGƯỜI DÙNG", new Dictionary<string, object> { { "Tổng số", stats.Users.Total }, { "Mới hôm nay", stats.Users.NewToday }, { "Mới 7 ngày qua", stats.Users.NewLast7Days }, { "Trực tuyến", stats.Realtime.OnlineUsers } }, Color.FromArgb(24, 119, 242)));
                _pnlOverview.Controls.Add(CreateStatGroup("💬 TƯƠNG TÁC", new Dictionary<string, object> { { "Bài đăng", stats.Engagement.Posts }, { "Lượt thích", stats.Engagement.Likes }, { "Bình luận", stats.Engagement.Comments }, { "Tin nhắn", stats.Engagement.Messages }, { "Cuộc gọi", stats.Engagement.Calls } }, Color.FromArgb(255, 136, 0)));
                _pnlOverview.Controls.Add(CreateStatGroup("❤️ XÃ HỘI & GHÉP ĐÔI", new Dictionary<string, object> { { "Bạn bè", stats.Social.Friendships }, { "Yêu cầu kết bạn", stats.Social.PendingFriendRequests }, { "Ghép đôi thành công", stats.Matching.SuccessfulPairs }, { "Trong hàng chờ", stats.Matching.UsersInQueue } }, Color.FromArgb(230, 30, 100)));
                _pnlOverview.Controls.Add(CreateStatGroup("🛡️ KIỂM DUYỆT", new Dictionary<string, object> { { "Tổng báo cáo", stats.Moderation.TotalReports }, { "Chờ xử lý", stats.Moderation.ByStatus.TryGetValue("pending", out var p) ? p : 0 }, { "Đã duyệt", stats.Moderation.ByStatus.TryGetValue("approved", out var a) ? a : 0 }, { "Đã từ chối", stats.Moderation.ByStatus.TryGetValue("rejected", out var r) ? r : 0 } }, Color.FromArgb(180, 50, 50)));
            }
            catch (Exception ex)
            {
                _pnlOverview.Controls.Clear();
                _pnlOverview.Controls.Add(new Label { Text = "Lỗi tải thống kê: " + ex.Message, AutoSize = true, ForeColor = Color.Red });
            }
        }

        private Control CreateStatGroup(string title, Dictionary<string, object> data, Color themeColor)
        {
            var groupPanel = new Panel { AutoSize = true, Width = _pnlOverview.ClientSize.Width - 40, Margin = new Padding(0, 0, 0, 20) };
            var lblHeader = new Label { Text = title, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = themeColor, Location = new Point(5, 0), AutoSize = true };
            groupPanel.Controls.Add(lblHeader);

            var flowLayout = new FlowLayoutPanel { Location = new Point(0, 30), Width = groupPanel.Width, AutoSize = true, WrapContents = true };
            foreach (var kvp in data)
            {
                var card = new Panel { Width = 200, Height = 80, BackColor = Color.White, Margin = new Padding(5, 5, 15, 15) };
                card.Paint += (s, e) => {
                    var rect = card.ClientRectangle; rect.Width--; rect.Height--;
                    e.Graphics.DrawRectangle(new Pen(Color.FromArgb(228, 228, 228), 1), rect);
                    e.Graphics.FillRectangle(new SolidBrush(themeColor), 0, 0, 5, card.Height);
                };
                var lblKey = new Label { Text = kvp.Key, Font = new Font("Segoe UI", 9, FontStyle.Regular), ForeColor = Color.Gray, Location = new Point(15, 15), AutoSize = true };
                var lblVal = new Label { Text = kvp.Value?.ToString(), Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(40, 40, 40), Location = new Point(15, 35), AutoSize = true };
                card.Controls.Add(lblKey);
                card.Controls.Add(lblVal);
                flowLayout.Controls.Add(card);
            }
            groupPanel.Controls.Add(flowLayout);
            return groupPanel;
        }

        private async Task LoadChartDataAsync()
        {
            try
            {
                var selected = (KeyValuePair<string, string>)_cboMetric.SelectedItem;
                var metric = selected.Value;
                var data = await _adminBll.GetTimeSeriesStatsAsync(metric, "daily", _dtpStart.Value, _dtpEnd.Value);
                var series = _chart.Series[0];
                series.Points.Clear();
                series.Name = selected.Key;
                foreach (var point in data.Data)
                {
                    if (DateTime.TryParse(point.Date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var date))
                        series.Points.AddXY(date, point.Count);
                }
                _chart.ChartAreas[0].AxisX.LabelStyle.Format = "dd/MM";
                _chart.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Days;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải biểu đồ: " + ex.Message); }
        }

        private async Task LoadTagsDataAsync()
        {
            try
            {
                var data = await _adminBll.GetPopularTagsAsync();
                _dgvTags.DataSource = data.Data;
                if (_dgvTags.Columns["Tag"] != null) _dgvTags.Columns["Tag"].HeaderText = "Tag";
                if (_dgvTags.Columns["TotalPosts"] != null) _dgvTags.Columns["TotalPosts"].HeaderText = "Tổng bài đăng";
                if (_dgvTags.Columns["TotalLikes"] != null) _dgvTags.Columns["TotalLikes"].HeaderText = "Tổng lượt thích";
                if (_dgvTags.Columns["AvgLikes"] != null) { _dgvTags.Columns["AvgLikes"].HeaderText = "TB Thích/Bài"; _dgvTags.Columns["AvgLikes"].DefaultCellStyle.Format = "N2"; }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải tags: " + ex.Message); }
        }

        private void ExportDataGridViewToCsv(DataGridView dgv, string defaultFileName)
        {
            if (dgv.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var sb = new System.Text.StringBuilder();

                var headers = dgv.Columns.Cast<DataGridViewColumn>();
                sb.AppendLine(string.Join(",", headers.Select(column => EscapeCsvField(column.HeaderText)).ToArray()));

                foreach (DataGridViewRow row in dgv.Rows)
                {
                    var cells = row.Cells.Cast<DataGridViewCell>();
                    sb.AppendLine(string.Join(",", cells.Select(cell => EscapeCsvField(cell.Value?.ToString() ?? "")).ToArray()));
                }

                var defaultName = $"{defaultFileName}_{DateTime.Now:yyyyMMdd_HHmm}.csv";
                using var previewForm = new Presentation.Reports.TextPreviewForm(sb.ToString(), defaultName);
                previewForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private async Task ExportAllToCsvAsync()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Báo cáo Tổng hợp LoveConnect");
                sb.AppendLine($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine();

                // --- 1. Overview Section ---
                sb.AppendLine("--- TỔNG QUAN ---");
                sb.AppendLine("Mục,Giá trị");
                try
                {
                    var stats = await _adminBll.GetDashboardStatsAsync();
                    sb.AppendLine($"Tổng số người dùng,{stats.Users.Total}");
                    sb.AppendLine($"Người dùng mới hôm nay,{stats.Users.NewToday}");
                    sb.AppendLine($"Người dùng mới 7 ngày qua,{stats.Users.NewLast7Days}");
                    sb.AppendLine($"Người dùng trực tuyến,{stats.Realtime.OnlineUsers}");
                    sb.AppendLine($"Tổng bài đăng,{stats.Engagement.Posts}");
                    sb.AppendLine($"Tổng lượt thích,{stats.Engagement.Likes}");
                    sb.AppendLine($"Tổng bình luận,{stats.Engagement.Comments}");
                    sb.AppendLine($"Tổng tin nhắn,{stats.Engagement.Messages}");
                    sb.AppendLine($"Tổng cuộc gọi,{stats.Engagement.Calls}");
                    sb.AppendLine($"Tổng số bạn bè,{stats.Social.Friendships}");
                    sb.AppendLine($"Yêu cầu kết bạn đang chờ,{stats.Social.PendingFriendRequests}");
                    sb.AppendLine($"Ghép đôi thành công,{stats.Matching.SuccessfulPairs}");
                    sb.AppendLine($"Người dùng trong hàng chờ,{stats.Matching.UsersInQueue}");
                    sb.AppendLine($"Tổng số báo cáo,{stats.Moderation.TotalReports}");
                    sb.AppendLine($"Báo cáo chờ xử lý,{(stats.Moderation.ByStatus.TryGetValue("pending", out var p) ? p : 0)}");
                    sb.AppendLine($"Báo cáo đã duyệt,{(stats.Moderation.ByStatus.TryGetValue("approved", out var a) ? a : 0)}");
                    sb.AppendLine($"Báo cáo đã từ chối,{(stats.Moderation.ByStatus.TryGetValue("rejected", out var r) ? r : 0)}");
                }
                catch (Exception ex) { sb.AppendLine($"Lỗi tải dữ liệu tổng quan,{EscapeCsvField(ex.Message)}"); }
                sb.AppendLine();

                // --- 2. Popular Tags Section ---
                sb.AppendLine("--- TAGS PHỔ BIẾN ---");
                sb.AppendLine("Tag,Tổng bài đăng,Tổng lượt thích,TB Thích/Bài");
                try
                {
                    var tagsData = await _adminBll.GetPopularTagsAsync();
                    if (tagsData?.Data != null)
                    {
                        foreach (var tag in tagsData.Data)
                        {
                            sb.AppendLine($"{EscapeCsvField(tag.Tag)},{tag.TotalPosts},{tag.TotalLikes},{tag.AvgLikes:N2}");
                        }
                    }
                }
                catch (Exception ex) { sb.AppendLine($"Lỗi tải dữ liệu tags,{EscapeCsvField(ex.Message)}"); }
                sb.AppendLine();

                // --- 3. Time Series Section ---
                var timeSeriesMetrics = _cboMetric.Items.Cast<KeyValuePair<string, string>>();
                var endDate = DateTime.Now;
                var startDate = endDate.AddDays(-29);

                foreach (var metric in timeSeriesMetrics)
                {
                    sb.AppendLine($"--- BIỂU ĐỒ: {metric.Key} (từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}) ---");
                    sb.AppendLine("Ngày,Số lượng");
                    try
                    {
                        var timeSeriesData = await _adminBll.GetTimeSeriesStatsAsync(metric.Value, "daily", startDate, endDate);
                        if (timeSeriesData?.Data != null)
                        {
                            foreach (var point in timeSeriesData.Data)
                            {
                                if (DateTime.TryParse(point.Date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var date))
                                    sb.AppendLine($"{date:yyyy-MM-dd},{point.Count}");
                            }
                        }
                    }
                    catch (Exception ex) { sb.AppendLine($"Lỗi tải dữ liệu biểu đồ,{EscapeCsvField(ex.Message)}"); }
                    sb.AppendLine();
                }

                this.Cursor = Cursors.Default;
                var defaultName = $"LoveConnect_Report_{DateTime.Now:yyyyMMdd_HHmm}.csv";
                using var previewForm = new Presentation.Reports.TextPreviewForm(sb.ToString(), defaultName);
                previewForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("Lỗi khi tạo dữ liệu xuất: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ExportToPdfReportAsync()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                // 1. Chuẩn bị DTO để chứa tất cả dữ liệu báo cáo
                var reportData = new FullStatsReportDTO
                {
                    ReportDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    OverviewStats = new List<FullStatsReportDTO.OverviewStatItem>(),
                    PopularTags = new List<PopularTagStatDTO>(),
                    TimeSeries = new Dictionary<string, List<TimeSeriesDataPointDTO>>(),
                    TimeSeriesCharts = new Dictionary<string, byte[]>()
                };

                // 2. Lấy dữ liệu tổng quan
                try
                {
                    var stats = await _adminBll.GetDashboardStatsAsync();
                    reportData.OverviewStats.Add(new FullStatsReportDTO.OverviewStatItem("Tổng số người dùng", stats.Users.Total));
                    reportData.OverviewStats.Add(new FullStatsReportDTO.OverviewStatItem("Người dùng mới hôm nay", stats.Users.NewToday));
                    reportData.OverviewStats.Add(new FullStatsReportDTO.OverviewStatItem("Người dùng mới 7 ngày qua", stats.Users.NewLast7Days));
                    reportData.OverviewStats.Add(new FullStatsReportDTO.OverviewStatItem("Tổng bài đăng", stats.Engagement.Posts));
                    reportData.OverviewStats.Add(new FullStatsReportDTO.OverviewStatItem("Tổng lượt thích", stats.Engagement.Likes));
                    reportData.OverviewStats.Add(new FullStatsReportDTO.OverviewStatItem("Ghép đôi thành công", stats.Matching.SuccessfulPairs));
                    reportData.OverviewStats.Add(new FullStatsReportDTO.OverviewStatItem("Tổng số báo cáo", stats.Moderation.TotalReports));
                    reportData.OverviewStats.Add(new FullStatsReportDTO.OverviewStatItem("Báo cáo chờ xử lý", (stats.Moderation.ByStatus.TryGetValue("pending", out var p) ? p : 0)));
                }
                catch { /* Bỏ qua nếu lỗi, báo cáo sẽ hiển thị phần này trống */ }

                // 3. Lấy dữ liệu tags phổ biến
                try
                {
                    var tagsData = await _adminBll.GetPopularTagsAsync();
                    if (tagsData?.Data != null)
                    {
                        reportData.PopularTags.AddRange(tagsData.Data);
                    }
                }
                catch { /* Bỏ qua nếu lỗi */ }

                // 4. Lấy dữ liệu biểu đồ
                var timeSeriesMetrics = _cboMetric.Items.Cast<KeyValuePair<string, string>>();
                var endDate = DateTime.Now;
                var startDate = endDate.AddDays(-29);
                foreach (var metric in timeSeriesMetrics)
                {
                    try
                    {
                        var timeSeriesData = await _adminBll.GetTimeSeriesStatsAsync(metric.Value, "daily", startDate, endDate);
                        if (timeSeriesData?.Data != null)
                        {
                            reportData.TimeSeries[metric.Key] = timeSeriesData.Data;

                            using var tempChart = new Chart { Width = 800, Height = 400 };
                            tempChart.ChartAreas.Add(new ChartArea("MainArea"));
                            var series = new Series(metric.Key) { ChartType = SeriesChartType.Line, BorderWidth = 3, Color = Color.DodgerBlue, MarkerStyle = MarkerStyle.Circle, MarkerSize = 8 };
                            tempChart.Series.Add(series);
                            foreach (var point in timeSeriesData.Data)
                            {
                                if (DateTime.TryParse(point.Date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var date))
                                    series.Points.AddXY(date, point.Count);
                            }
                            tempChart.ChartAreas[0].AxisX.LabelStyle.Format = "dd/MM";
                            tempChart.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Days;
                            tempChart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                            tempChart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                            
                            using var ms = new MemoryStream();
                            tempChart.SaveImage(ms, ChartImageFormat.Png);
                            reportData.TimeSeriesCharts[metric.Key] = ms.ToArray();
                        }
                    }
                    catch { /* Bỏ qua nếu lỗi */ }
                }

                // 5. Tạo và xuất file PDF bằng QuestPDF
                var document = new StatsQuestPdfReport(reportData);
                
                this.Cursor = Cursors.Default;
                using var previewForm = new Presentation.Reports.PdfPreviewForm(document, $"LoveConnect_Stats_Report_{DateTime.Now:yyyyMMdd}.pdf");
                previewForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("Lỗi khi tạo báo cáo PDF: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}