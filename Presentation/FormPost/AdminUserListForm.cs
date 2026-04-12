using BusinessLogic;
using DataTransferObject;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using Presentation.FormProfile;
using System.Net.Http;
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
    public class AdminUserListForm : Form
    {
        private readonly AdminBLL _adminBll;
        private FlowLayoutPanel _pnlContent;
        private Button _btnRefresh;
        private List<UserAdminViewDTO> _cachedUsers = new List<UserAdminViewDTO>();
        private int _currentPage = 1;
        private bool _isLoading = false;

        public AdminUserListForm()
        {
            _adminBll = BusinessLogic.AppServices.AdminBll;

            // Cấu hình bản quyền cho thư viện QuestPDF (sử dụng bản miễn phí Community)
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            Text = "Quản lý Người dùng (Admin)";
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

            _btnRefresh = new Button { Text = "Tải lại", Location = new Point(130, 15), Width = 80, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(255, 30, 100), ForeColor = Color.White };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.Click += async (_, __) => await LoadDataAsync(true);
            _btnRefresh.Cursor = Cursors.Hand;

            var lblTitle = new Label { Text = "Danh sách Người dùng", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(340, 15), AutoSize = true };

            pnlTop.Controls.Add(btnBack);
            pnlTop.Controls.Add(_btnRefresh);
            pnlTop.Controls.Add(lblTitle);

            // Footer
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(10), BackColor = Color.WhiteSmoke };
            var line = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.LightGray };
            pnlFooter.Controls.Add(line);

            var btnExport = new Button { Text = "Xem trước CSV", Width = 140, Height = 35, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(10, 130, 60), ForeColor = Color.White };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += (_, __) => ExportUsersToCsv();
            btnExport.Cursor = Cursors.Hand;

            var btnExportPdf = new Button { Text = "Xem trước PDF", Width = 140, Height = 35, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(180, 50, 50), ForeColor = Color.White };
            btnExportPdf.FlatAppearance.BorderSize = 0;
            btnExportPdf.Click += (_, __) => ExportUsersToPdf();
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
            if (_pnlContent.IsDisposed) return;
            foreach (Control c in _pnlContent.Controls)
            {
                c.Width = _pnlContent.ClientSize.Width - 45; 
            }
        }

        private async Task LoadDataAsync(bool refresh = false)
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                _btnRefresh.Enabled = false;
                if (refresh)
                {
                    _currentPage = 1;
                    _pnlContent.Controls.Clear();
                    _cachedUsers.Clear();
                }

                var res = await _adminBll.GetUsersAsync(page: _currentPage);
                if (res?.Users != null)
                {
                    _cachedUsers.AddRange(res.Users);
                    foreach (var user in res.Users)
                    {
                        var card = CreateUserCard(user);
                        _pnlContent.Controls.Add(card);
                    }
                    CenterCards();
                    _currentPage++;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải danh sách người dùng: " + ex.Message); }
            finally { _btnRefresh.Enabled = true; _isLoading = false; }
        }

        private Panel CreateUserCard(UserAdminViewDTO user)
        {
            var card = new Panel
            {
                Height = 100,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 15),
                Padding = new Padding(15),
                Cursor = Cursors.Hand
            };

            card.Paint += (_, e) =>
            {
                var border = Color.FromArgb(228, 228, 228);
                var rect = card.ClientRectangle;
                rect.Width -= 1; rect.Height -= 1;
                using var pen = new Pen(border, 1);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(pen, rect);
            };

            var picAvatar = new PictureBox { Size = new Size(70, 70), Location = new Point(15, 15), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(238, 238, 238) };
            ApplyRoundedAvatar(picAvatar);
            if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
            {
                _ = LoadAvatarAsync(picAvatar, user.AvatarUrl);
            }

            var lblUsername = new Label { Text = user.Username, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(100, 15), AutoSize = true };
            var lblEmail = new Label { Text = user.Email, Font = new Font("Segoe UI", 9), Location = new Point(100, 45), AutoSize = true, ForeColor = Color.Gray };
            var lblStatus = new Label { Text = $"Status: {user.Status}", Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(100, 70), AutoSize = true, ForeColor = GetStatusColor(user.Status) };
            var lblRole = new Label { Text = user.Role.ToUpper(), Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(250, 70), AutoSize = true, ForeColor = user.Role == "admin" ? Color.Crimson : Color.DarkCyan };

            var btnManage = new Button { Text = "Quản lý", Width = 100, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = Color.LightGray, Anchor = AnchorStyles.Right | AnchorStyles.Top };
            btnManage.Location = new Point(card.Width - btnManage.Width - 20, 35);
            btnManage.Click += (_, __) => {
                using var dlg = new ManageUserDialog(user);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _ = LoadDataAsync(true); // Refresh list on OK
                }
            };

            card.SizeChanged += (_,__) => { btnManage.Location = new Point(card.Width - btnManage.Width - 20, 35); };

            card.Click += (_, __) => { var profileForm = new UserProfile(user.Id, this.FindForm()); profileForm.FormClosed += (s, ev) => { this.FindForm()?.Show(); }; profileForm.Show(); this.FindForm()?.Hide(); };

            card.Controls.Add(picAvatar);
            card.Controls.Add(lblUsername);
            card.Controls.Add(lblEmail);
            card.Controls.Add(lblStatus);
            card.Controls.Add(lblRole);
            card.Controls.Add(btnManage);

            return card;
        }

        private Color GetStatusColor(string status) => status switch { "active" => Color.Green, "inactive" => Color.Gray, "pending" => Color.Orange, "banned" => Color.Red, _ => Color.Black };

        private static async Task LoadAvatarAsync(PictureBox pb, string url)
        {
            try { using var httpClient = new HttpClient(); var fullUrl = url.StartsWith("http") ? url : $"{BusinessLogic.AppConfig.BaseUrl}{url}"; var bytes = await httpClient.GetByteArrayAsync(fullUrl); using var ms = new System.IO.MemoryStream(bytes); var img = Image.FromStream(ms); if (!pb.IsDisposed) pb.Image = img; } catch { /* ignore avatar load errors */ }
        }

        private static void ApplyRoundedAvatar(PictureBox pictureBox)
        {
            void UpdateRegion() { if (pictureBox.IsDisposed) return; var diameter = Math.Min(pictureBox.Width, pictureBox.Height); var rect = new Rectangle(0, 0, diameter, diameter); using var path = new GraphicsPath(); path.AddEllipse(rect); pictureBox.Region = new Region(path); }
            pictureBox.SizeChanged += (_, __) => UpdateRegion();
            UpdateRegion();
        }

        private void ExportUsersToCsv()
        {
            if (_cachedUsers == null || _cachedUsers.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu người dùng để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("ID,Username,Email,Status,Role,Restrictions");

                foreach (var user in _cachedUsers)
                {
                    var features = string.Join(";", user.GetRestrictionFeatures());
                    var line = $"{user.Id},{EscapeCsvField(user.Username)},{EscapeCsvField(user.Email)},{user.Status},{user.Role},{EscapeCsvField(features)}";
                    sb.AppendLine(line);
                }

                using var previewForm = new Presentation.Reports.TextPreviewForm(sb.ToString(), $"User_List_{DateTime.Now:yyyyMMdd_HHmm}.csv");
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

        private void ExportUsersToPdf()
        {
            if (_cachedUsers == null || _cachedUsers.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu người dùng để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                                col.Item().Text("DANH SÁCH NGƯỜI DÙNG").Style(TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2));
                                col.Item().Text($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}").Style(TextStyle.Default.FontSize(10).FontColor(Colors.Grey.Darken1));
                                col.Item().Text($"Tổng số lượng: {_cachedUsers.Count} người dùng").Style(TextStyle.Default.FontSize(10).FontColor(Colors.Grey.Darken1));
                            });
                        });

                        page.Content().PaddingVertical(15).Column(column =>
                        {
                            column.Spacing(15);
                            foreach (var user in _cachedUsers)
                            {
                                var statusColor = user.Status == "active" ? Colors.Green.Medium : (user.Status == "inactive" ? Colors.Grey.Medium : (user.Status == "banned" ? Colors.Red.Medium : Colors.Orange.Medium));
                                var roleColor = user.Role == "admin" ? Colors.Red.Medium : Colors.Blue.Medium;

                                column.Item().Background(Colors.White)
                                    .Border(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(12)
                                    .Column(card =>
                                    {
                                        card.Item().Row(row =>
                                        {
                                            row.RelativeItem().Text($"Mã định danh (ID): {user.Id}").FontSize(9).FontColor(Colors.Grey.Medium);
                                            row.ConstantItem(80).AlignRight().Text(user.Status?.ToUpper() ?? "UNKNOWN").FontSize(10).Bold().FontColor(statusColor);
                                        });

                                        card.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten3);

                                        card.Item().PaddingTop(8).Grid(grid =>
                                        {
                                            grid.Columns(12);
                                            grid.Spacing(5);

                                            grid.Item(3).Text("Tên người dùng:").SemiBold().FontSize(10);
                                            grid.Item(9).Text(user.Username ?? "Không có").FontSize(10).Bold();

                                            grid.Item(3).Text("Email:").SemiBold().FontSize(10);
                                            grid.Item(9).Text(user.Email ?? "Không có").FontSize(10);

                                            grid.Item(3).Text("Vai trò:").SemiBold().FontSize(10);
                                            grid.Item(9).Text(user.Role?.ToUpper() ?? "USER").FontSize(10).Bold().FontColor(roleColor);

                                            var features = string.Join(", ", user.GetRestrictionFeatures());
                                            if (!string.IsNullOrWhiteSpace(features)) { grid.Item(3).Text("Hạn chế:").SemiBold().FontSize(10).FontColor(Colors.Red.Medium); grid.Item(9).Text(features).FontSize(10).Italic().FontColor(Colors.Red.Medium); }
                                            else { grid.Item(3).Text("Hạn chế:").SemiBold().FontSize(10); grid.Item(9).Text("Không có").FontSize(10).Italic().FontColor(Colors.Grey.Medium); }
                                        });
                                    });
                            }
                        });
                        page.Footer().AlignCenter().Text(text => { text.CurrentPageNumber(); text.Span(" / "); text.TotalPages(); });
                    });
                });

                using var previewForm = new Presentation.Reports.PdfPreviewForm(document, $"User_List_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
                previewForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tạo báo cáo PDF: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}