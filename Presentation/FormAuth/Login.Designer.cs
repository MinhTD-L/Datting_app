namespace Presentation
{
    partial class Login
    {
        private System.ComponentModel.IContainer components = null;

        private RoundedPanel loginCard;
        private Label lblTitle;
        private Label lblEmail;
        private Label lblPassword;
        private LinkLabel linkForgot;
        private LinkLabel linkRegister;
        private System.Windows.Forms.PictureBox pictureBox1;


        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            txtEmail = new RoundedTextBox();
            txtPassword = new RoundedTextBox();
            btnLogin = new RoundedButton();
            loginCard = new RoundedPanel();
            lblTitle = new Label();
            lblEmail = new Label();
            lblPassword = new Label();
            linkForgot = new LinkLabel();
            linkRegister = new LinkLabel();
            pictureBox1 = new PictureBox();
            loginCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // txtEmail
            // 
            txtEmail.BackColor = Color.White;
            txtEmail.BorderStyle = BorderStyle.FixedSingle;
            txtEmail.Font = new Font("Segoe UI", 10F);
            txtEmail.Location = new Point(40, 120);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(290, 35);
            txtEmail.TabIndex = 2;
            txtEmail.UseSystemPasswordChar = false;
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.White;
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.Font = new Font("Segoe UI", 10F);
            txtPassword.Location = new Point(40, 190);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(290, 35);
            txtPassword.TabIndex = 4;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(214, 106, 94);
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(40, 250);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(290, 40);
            btnLogin.TabIndex = 5;
            btnLogin.Text = "ĐĂNG NHẬP";
            btnLogin.UseVisualStyleBackColor = false;
            // 
            // loginCard
            // 
            loginCard.BackColor = Color.FromArgb(247, 229, 228);
            loginCard.Controls.Add(lblTitle);
            loginCard.Controls.Add(lblEmail);
            loginCard.Controls.Add(txtEmail);
            loginCard.Controls.Add(lblPassword);
            loginCard.Controls.Add(txtPassword);
            loginCard.Controls.Add(btnLogin);
            loginCard.Controls.Add(linkForgot);
            loginCard.Controls.Add(linkRegister);
            loginCard.Location = new Point(580, 60);
            loginCard.Name = "loginCard";
            loginCard.Size = new Size(380, 400);
            loginCard.TabIndex = 1;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(160, 85, 80);
            lblTitle.Location = new Point(120, 30);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(146, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "ĐĂNG NHẬP";
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Font = new Font("Segoe UI", 9F);
            lblEmail.Location = new Point(40, 100);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(126, 15);
            lblEmail.TabIndex = 1;
            lblEmail.Text = "Tên đăng nhập / Email";
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Font = new Font("Segoe UI", 9F);
            lblPassword.Location = new Point(40, 170);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(57, 15);
            lblPassword.TabIndex = 3;
            lblPassword.Text = "Mật khẩu";
            // 
            // linkForgot
            // 
            linkForgot.AutoSize = true;
            linkForgot.LinkColor = Color.FromArgb(150, 90, 85);
            linkForgot.Location = new Point(130, 305);
            linkForgot.Name = "linkForgot";
            linkForgot.Size = new Size(94, 15);
            linkForgot.TabIndex = 6;
            linkForgot.TabStop = true;
            linkForgot.Text = "Quên mật khẩu?";
            // 
            // linkRegister
            // 
            linkRegister.AutoSize = true;
            linkRegister.LinkColor = Color.FromArgb(150, 90, 85);
            linkRegister.Location = new Point(90, 330);
            linkRegister.Name = "linkRegister";
            linkRegister.Size = new Size(183, 15);
            linkRegister.TabIndex = 7;
            linkRegister.TabStop = true;
            linkRegister.Text = "Chưa có tài khoản? Đăng ký ngay";
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.LoginImage;
            pictureBox1.Location = new Point(66, 60);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(393, 400);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // Login
            // 
            BackColor = Color.FromArgb(238, 208, 207);
            ClientSize = new Size(1000, 520);
            Controls.Add(pictureBox1);
            Controls.Add(loginCard);
            Name = "Login";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Dating App Login";
            loginCard.ResumeLayout(false);
            loginCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        private RoundedTextBox txtEmail;
        private RoundedTextBox txtPassword;
        private RoundedButton btnLogin;
    }
}
