namespace Presentation
{
    partial class Register
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlCard = new RoundedPanel();
            lblBio = new System.Windows.Forms.Label();
            pnlBioWrap = new RoundedPanel();
            txtBio = new System.Windows.Forms.TextBox();
            lblTitle = new System.Windows.Forms.Label();
            lblSubTitle = new System.Windows.Forms.Label();
            btnRegister = new RoundedButton();
            chkTerms = new System.Windows.Forms.CheckBox();
            lnkTerms = new System.Windows.Forms.LinkLabel();
            lblLoginLink = new System.Windows.Forms.LinkLabel();
            pnlGenderWrap = new RoundedPanel();
            pnlInterestWrap = new RoundedPanel();
            cmbGender = new System.Windows.Forms.ComboBox();
            cmbInterest = new System.Windows.Forms.ComboBox();
            txtName = new RoundedTextBox();
            txtEmail = new RoundedTextBox();
            txtPass = new RoundedTextBox();
            txtConfirmPass = new RoundedTextBox();
            txtAge = new RoundedTextBox();

            pnlCard.SuspendLayout();
            pnlBioWrap.SuspendLayout();
            SuspendLayout();

            // Form Register
            BackColor = System.Drawing.Color.FromArgb(255, 245, 247);
            ClientSize = new System.Drawing.Size(1200, 850);
            Controls.Add(pnlCard);
            Name = "Register";
            Text = "Đăng ký - LoveConnect";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            // pnlCard
            pnlCard.Anchor = System.Windows.Forms.AnchorStyles.None;
            pnlCard.BackColor = System.Drawing.Color.White;
            pnlCard.Location = new System.Drawing.Point(350, 25);
            pnlCard.Size = new System.Drawing.Size(500, 800);
            pnlCard.Name = "pnlCard";

            // lblTitle
            lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 20F);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(255, 45, 120);
            lblTitle.Size = new System.Drawing.Size(500, 45);
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblTitle.Text = "Tham gia LoveConnect";

            // lblSubTitle
            lblSubTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            lblSubTitle.ForeColor = System.Drawing.Color.Gray;
            lblSubTitle.Size = new System.Drawing.Size(500, 25);
            lblSubTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblSubTitle.Text = "Tìm kiếm tình yêu đích thực của bạn";

            // btnRegister
            btnRegister.BackColor = System.Drawing.Color.FromArgb(255, 45, 120);
            btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRegister.Font = new System.Drawing.Font("Segoe UI Semibold", 12F);
            btnRegister.ForeColor = System.Drawing.Color.White;
            btnRegister.Text = "Đăng ký";
            btnRegister.Click += new System.EventHandler(btnRegister_Click);

            // chkTerms
            chkTerms.AutoSize = true;
            chkTerms.Text = "Tôi đồng ý với";
            chkTerms.CheckedChanged += new System.EventHandler(chkTerms_CheckedChanged);

            // lnkTerms
            lnkTerms.AutoSize = true;
            lnkTerms.LinkColor = System.Drawing.Color.FromArgb(255, 45, 120);
            lnkTerms.Text = "Điều khoản sử dụng và Chính sách bảo mật";
            lnkTerms.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(lnkTerms_LinkClicked);
            lblLoginLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(lnkLogin_LinkClicked);

            pnlCard.ResumeLayout(false);
            pnlCard.PerformLayout();
            pnlBioWrap.ResumeLayout(false);
            pnlBioWrap.PerformLayout();
            ResumeLayout(false);

        }

        private RoundedPanel pnlCard;
        private RoundedPanel pnlGenderWrap;
        private RoundedPanel pnlInterestWrap;
        private RoundedPanel pnlBioWrap;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubTitle;
        private RoundedTextBox txtName;
        private RoundedTextBox txtEmail;
        private RoundedTextBox txtPass;
        private RoundedTextBox txtConfirmPass;
        private RoundedTextBox txtAge;
        private System.Windows.Forms.ComboBox cmbGender;
        private System.Windows.Forms.ComboBox cmbInterest;
        private System.Windows.Forms.TextBox txtBio;
        private System.Windows.Forms.CheckBox chkTerms;
        private System.Windows.Forms.LinkLabel lnkTerms;
        private RoundedButton btnRegister;
        private System.Windows.Forms.LinkLabel lblLoginLink;
        private System.Windows.Forms.Label lblBio;
    }
}