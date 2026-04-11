using BusinessLogic;
using DataTransferObject;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

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
            Text = "Thống kê hệ thống";
            Size = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 246, 250);

            BuildUI();
            _ = LoadAllDataAsync();
        }

        private void BuildUI()
        {
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
                BackColor = Color.White
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
            parent.Controls.Add(_dgvTags);
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

                _pnlOverview.Controls.Add(CreateStatGroup("Người dùng", new Dictionary<string, object> { { "Tổng số", stats.Users.Total }, { "Mới hôm nay", stats.Users.NewToday }, { "Mới 7 ngày qua", stats.Users.NewLast7Days }, { "Trực tuyến", stats.Realtime.OnlineUsers } }));
                _pnlOverview.Controls.Add(CreateStatGroup("Tương tác", new Dictionary<string, object> { { "Bài đăng", stats.Engagement.Posts }, { "Lượt thích", stats.Engagement.Likes }, { "Bình luận", stats.Engagement.Comments }, { "Tin nhắn", stats.Engagement.Messages }, { "Cuộc gọi", stats.Engagement.Calls } }));
                _pnlOverview.Controls.Add(CreateStatGroup("Xã hội & Ghép đôi", new Dictionary<string, object> { { "Bạn bè", stats.Social.Friendships }, { "Yêu cầu kết bạn", stats.Social.PendingFriendRequests }, { "Ghép đôi thành công", stats.Matching.SuccessfulPairs }, { "Trong hàng chờ", stats.Matching.UsersInQueue } }));
                _pnlOverview.Controls.Add(CreateStatGroup("Kiểm duyệt", new Dictionary<string, object> { { "Tổng báo cáo", stats.Moderation.TotalReports }, { "Chờ xử lý", stats.Moderation.ByStatus.TryGetValue("pending", out var p) ? p : 0 }, { "Đã duyệt", stats.Moderation.ByStatus.TryGetValue("approved", out var a) ? a : 0 }, { "Đã từ chối", stats.Moderation.ByStatus.TryGetValue("rejected", out var r) ? r : 0 } }));
            }
            catch (Exception ex)
            {
                _pnlOverview.Controls.Clear();
                _pnlOverview.Controls.Add(new Label { Text = "Lỗi tải thống kê: " + ex.Message, AutoSize = true, ForeColor = Color.Red });
            }
        }

        private GroupBox CreateStatGroup(string title, Dictionary<string, object> data)
        {
            var group = new GroupBox { Text = title, AutoSize = true, Padding = new Padding(10), Margin = new Padding(10), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            foreach (var kvp in data)
            {
                layout.Controls.Add(new Label { Text = kvp.Key + ":", AutoSize = true, Font = new Font("Segoe UI", 9), Margin = new Padding(5) });
                layout.Controls.Add(new Label { Text = kvp.Value.ToString(), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), Margin = new Padding(5) });
            }
            group.Controls.Add(layout);
            return group;
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
    }
}