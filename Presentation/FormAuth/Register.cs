using System;
using System.Drawing;
using System.Windows.Forms;

namespace Presentation
{
    public partial class Register : Form
    {
        public Register()
        {
            InitializeComponent();
            LoadManualLayout();
            btnRegister.Enabled = false;
        }
        private void LoadManualLayout()
        {
            pnlCard.Controls.Clear();

            pnlCard.Controls.Add(lblTitle);
            pnlCard.Controls.Add(lblSubTitle);

            // Hàng 1
            CreateField(pnlCard, "Họ tên *", txtName, 40, 110, 200);
            CreateField(pnlCard, "Email *", txtEmail, 260, 110, 200);

            // Hàng 2
            CreateField(pnlCard, "Mật khẩu *", txtPass, 40, 185, 200);
            txtPass.UseSystemPasswordChar = true;
            CreateField(pnlCard, "Xác nhận mật khẩu *", txtConfirmPass, 260, 185, 200);
            txtConfirmPass.UseSystemPasswordChar = true;

            // --- Row 3: Tuổi & Giới tính ---
            CreateField(pnlCard, "Tuổi *", txtAge, 40, 260, 200);
            SetupRoundedCombo(pnlCard, pnlGenderWrap, cmbGender, "Giới tính *", 260, 260, 200);
            cmbGender.Items.Clear();
            cmbGender.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });

            // --- Row 4: Sở thích hẹn hò ---
            SetupRoundedCombo(pnlCard, pnlInterestWrap, cmbInterest, "Sở thích hẹn hò *", 40, 335, 420);
            cmbInterest.Items.Clear();
            cmbInterest.Items.AddRange(new object[] { "Nam", "Nữ" });

            // Phần giới thiệu bản thân
            lblBio.Text = "Giới thiệu bản thân";
            lblBio.Font = new Font("Segoe UI Semibold", 9F);
            lblBio.Location = new Point(40, 410);
            lblBio.AutoSize = true;

            pnlBioWrap.Location = new Point(40, 435);
            pnlBioWrap.Size = new Size(420, 120);
            pnlBioWrap.BackColor = Color.White;
            pnlBioWrap.Padding = new Padding(10);

            txtBio.BorderStyle = BorderStyle.None;
            txtBio.Dock = DockStyle.Fill;
            txtBio.Multiline = true;
            txtBio.Font = new Font("Segoe UI", 10.5F);
            txtBio.Text = "Chia sẻ về bản thân bạn...";
            txtBio.ForeColor = Color.Gray;

            pnlCard.Controls.Add(lblBio);
            pnlCard.Controls.Add(pnlBioWrap);

            // Điều khoản & Nút bấm
            chkTerms.Location = new Point(40, 580);
            lnkTerms.Location = new Point(133, 580);
            btnRegister.Location = new Point(40, 630);
            btnRegister.Size = new Size(420, 50);

            lblLoginLink.Text = "Đã có tài khoản? Đăng nhập";
            lblLoginLink.Location = new Point(0, 700);
            lblLoginLink.Size = new Size(500, 25);
            lblLoginLink.TextAlign = ContentAlignment.MiddleCenter;

            pnlCard.Controls.Add(chkTerms);
            pnlCard.Controls.Add(lnkTerms);
            pnlCard.Controls.Add(btnRegister);
            pnlCard.Controls.Add(lblLoginLink);

            // Đưa panel chính lên trên cùng
            pnlCard.BringToFront();
        }
        private void CreateField(Control parent, string labelText, Control input, int x, int y, int width)
        {
            var lbl = new Label
            {
                Text = labelText,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9F),
                ForeColor = Color.FromArgb(64, 64, 64)
            };

            input.Location = new Point(x, y + 25);
            input.Size = new Size(width, 40);
            input.Font = new Font("Segoe UI", 10.5F);

            parent.Controls.Add(lbl);
            parent.Controls.Add(input);
        }
        private void SetupRoundedCombo(Control parent, RoundedPanel wrap, ComboBox combo, string labelText, int x, int y, int width)
        {
            var lbl = new Label
            {
                Text = labelText,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9F),
                ForeColor = Color.FromArgb(64, 64, 64)
            };

            wrap.Location = new Point(x, y + 25);
            wrap.Size = new Size(width, 40);
            wrap.BackColor = Color.White;
            wrap.Padding = new Padding(10, 8, 5, 0);

            combo.Dock = DockStyle.Fill;
            combo.FlatStyle = FlatStyle.Flat;
            combo.DropDownStyle = ComboBoxStyle.DropDownList;

            wrap.Controls.Add(combo);
            parent.Controls.Add(lbl);
            parent.Controls.Add(wrap);
        }

        // CÁC SỰ KIỆN
        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (!chkTerms.Checked)
            {
                MessageBox.Show("Vui lòng đồng ý với điều khoản sử dụng!");
                return;
            }
            MessageBox.Show($"Chào mừng {txtName.Text}! Đăng ký thành công.");
        }

        private void chkTerms_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTerms.Checked)
            {
                btnRegister.Enabled = true;
            }
            else
            {
                btnRegister.Enabled = false;
            }
        }

        private void lnkTerms_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Terms terms = new Terms();
            terms.ShowDialog();
        }
        private void lnkLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Close();
        }
        
    }
}
