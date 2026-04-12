using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Linq;

using Color = System.Drawing.Color;
using Size = System.Drawing.Size;
using Image = System.Drawing.Image;

namespace Presentation.Reports
{
    public class PdfPreviewForm : Form
    {
        private readonly IDocument _document;
        private readonly string _defaultFileName;
        private FlowLayoutPanel _flowLayout;

        public PdfPreviewForm(IDocument document, string defaultFileName)
        {
            _document = document;
            _defaultFileName = defaultFileName;

            Text = "Xem trước Báo cáo PDF";
            Size = new Size(1000, 800);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(82, 86, 89);

            BuildUI();
        }

        private void BuildUI()
        {
            var toolBar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            
            var lblTitle = new Label
            {
                Text = "📄 Bản xem trước báo cáo",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                AutoSize = true,
                Location = new Point(20, 15)
            };

            var btnClose = new Button { Text = "Đóng", Width = 100, Height = 36, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(240, 240, 240), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnClose.Location = new Point(toolBar.Width - btnClose.Width - 20, 12);
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Cursor = Cursors.Hand;
            btnClose.Click += (_, __) => Close();

            var btnSave = new Button { Text = "⬇ Lưu PDF", Width = 130, Height = 36, BackColor = Color.FromArgb(180, 50, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnSave.Location = new Point(btnClose.Left - btnSave.Width - 10, 12);
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Cursor = Cursors.Hand;
            btnSave.Click += BtnSave_Click;

            toolBar.Controls.Add(lblTitle);
            toolBar.Controls.Add(btnClose);
            toolBar.Controls.Add(btnSave);

            _flowLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.FromArgb(82, 86, 89),
                Padding = new Padding(0, 20, 0, 20)
            };

            _flowLayout.SizeChanged += (s, e) => CenterPages();

            Controls.Add(_flowLayout);
            Controls.Add(toolBar);

            Shown += async (s, e) => await LoadPreviewAsync();
        }

        private void CenterPages()
        {
            _flowLayout.SuspendLayout();
            foreach (Control c in _flowLayout.Controls)
            {
                int leftMargin = Math.Max(0, (_flowLayout.ClientSize.Width - c.Width) / 2);
                c.Margin = new Padding(leftMargin, 10, 0, 30);
            }
            _flowLayout.ResumeLayout();
        }

        private async System.Threading.Tasks.Task LoadPreviewAsync()
        {
            try
            {
                var lblLoading = new Label { Text = "⏳ Đang tạo bản xem trước...", AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 12) };
                _flowLayout.Controls.Add(lblLoading);
                CenterPages();

                var images = await System.Threading.Tasks.Task.Run(() => _document.GenerateImages().ToList());

                _flowLayout.Controls.Clear();
                foreach (var imgBytes in images)
                {
                    var ms = new MemoryStream(imgBytes);
                    var img = Image.FromStream(ms);
                    
                    // Đặt lại kích thước hiển thị (chiều rộng 700px, tự động tính chiều cao theo tỉ lệ A4)
                    int targetWidth = 700;
                    int targetHeight = (int)(img.Height * ((float)targetWidth / img.Width));

                    var pageWrapper = new Panel
                    {
                        Size = new Size(targetWidth, targetHeight),
                        BackColor = Color.White,
                        Margin = new Padding(0, 10, 0, 30)
                    };

                    pageWrapper.Paint += (sender, e) =>
                    {
                        var rect = pageWrapper.ClientRectangle;
                        rect.Width -= 1;
                        rect.Height -= 1;
                        using var pen = new Pen(Color.FromArgb(150, 150, 150), 1);
                        e.Graphics.DrawRectangle(pen, rect);
                    };

                    var pb = new PictureBox { Image = img, Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
                    pageWrapper.Controls.Add(pb);
                    
                    _flowLayout.Controls.Add(pageWrapper);
                    
                    pb.Disposed += (sender, e) => { ms.Dispose(); img.Dispose(); };
                }
                CenterPages();
            }
            catch (Exception ex)
            {
                _flowLayout.Controls.Clear();
                MessageBox.Show("Lỗi tạo bản xem trước: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog { Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*", FileName = _defaultFileName };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try { _document.GeneratePdf(sfd.FileName); MessageBox.Show("Xuất PDF thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information); System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true }); this.Close(); }
                catch (Exception ex) { MessageBox.Show("Lỗi lưu PDF: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }
    }
}