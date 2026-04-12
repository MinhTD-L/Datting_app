using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Presentation.Reports
{
    public class TextPreviewForm : Form
    {
        private readonly string _content;
        private readonly string _defaultFileName;

        public TextPreviewForm(string content, string defaultFileName)
        {
            _content = content;
            _defaultFileName = defaultFileName;

            Text = "Xem trước Dữ liệu CSV";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            BuildUI();
        }

        private void BuildUI()
        {
            var toolBar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.WhiteSmoke };
            
            var btnClose = new Button { Text = "Đóng", Location = new Point(20, 15), Width = 100, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = Color.LightGray };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (_, __) => Close();

            var btnSave = new Button { Text = "Lưu CSV", Location = new Point(130, 15), Width = 120, Height = 30, BackColor = Color.FromArgb(10, 130, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            toolBar.Controls.Add(btnClose);
            toolBar.Controls.Add(btnSave);

            var txtContent = new TextBox
            {
                Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both, ReadOnly = true,
                Font = new Font("Consolas", 10), Text = _content, BackColor = Color.White, WordWrap = false
            };

            Controls.Add(txtContent);
            Controls.Add(toolBar);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*", FileName = _defaultFileName };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try { File.WriteAllText(sfd.FileName, _content, Encoding.UTF8); MessageBox.Show("Xuất CSV thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information); this.Close(); }
                catch (Exception ex) { MessageBox.Show("Lỗi lưu CSV: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }
    }
}