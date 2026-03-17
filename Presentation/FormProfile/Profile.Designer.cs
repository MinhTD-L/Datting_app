namespace Presentation.FormProfile
{
    partial class Profile
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            btnEdit = new Button();
            lblAgeHead = new Label();
            lblEmailHead = new Label();
            lblAge = new Label();
            lblEmail = new Label();
            lblUsername = new Label();
            lblNameAge = new Label();
            pbAvatar = new PictureBox();
            pnlPinkBanner = new Panel();
            flpContent = new FlowLayoutPanel();
            pnlIntro = new Panel();
            lblBio = new Label();
            lblBioHead = new Label();
            tlpStats = new TableLayoutPanel();
            pnlConnect = new Panel();
            label3 = new Label();
            lblConnectHead = new Label();
            pnlCare = new Panel();
            label2 = new Label();
            lblCareHead = new Label();
            pnlGender = new Panel();
            lblGender = new Label();
            lblGenderHead = new Label();
            tlpBottomGrid = new TableLayoutPanel();
            panel1 = new Panel();
            panel2 = new Panel();
            lblMyPicture = new Label();
            lblMyPostHead = new Label();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbAvatar).BeginInit();
            flpContent.SuspendLayout();
            pnlIntro.SuspendLayout();
            tlpStats.SuspendLayout();
            pnlConnect.SuspendLayout();
            pnlCare.SuspendLayout();
            pnlGender.SuspendLayout();
            tlpBottomGrid.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(btnEdit);
            pnlHeader.Controls.Add(lblAgeHead);
            pnlHeader.Controls.Add(lblEmailHead);
            pnlHeader.Controls.Add(lblAge);
            pnlHeader.Controls.Add(lblEmail);
            pnlHeader.Controls.Add(lblUsername);
            pnlHeader.Controls.Add(lblNameAge);
            pnlHeader.Controls.Add(pbAvatar);
            pnlHeader.Controls.Add(pnlPinkBanner);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1000, 250);
            pnlHeader.TabIndex = 1;
            // 
            // btnEdit
            // 
            btnEdit.Location = new Point(810, 161);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(153, 43);
            btnEdit.TabIndex = 5;
            btnEdit.Text = "Chỉnh sửa hồ sơ";
            btnEdit.UseVisualStyleBackColor = true;
            // 
            // lblAgeHead
            // 
            lblAgeHead.Location = new Point(284, 224);
            lblAgeHead.Name = "lblAgeHead";
            lblAgeHead.Size = new Size(38, 23);
            lblAgeHead.TabIndex = 0;
            lblAgeHead.Text = "Tuổi: ";
            // 
            // lblEmailHead
            // 
            lblEmailHead.Location = new Point(35, 227);
            lblEmailHead.Name = "lblEmailHead";
            lblEmailHead.Size = new Size(45, 23);
            lblEmailHead.TabIndex = 0;
            lblEmailHead.Text = "Email:";
            // 
            // lblAge
            // 
            lblAge.Location = new Point(318, 224);
            lblAge.Name = "lblAge";
            lblAge.Size = new Size(34, 23);
            lblAge.TabIndex = 0;
            lblAge.Text = "22 ";
            // 
            // lblEmail
            // 
            lblEmail.Location = new Point(77, 227);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(201, 23);
            lblEmail.TabIndex = 0;
            lblEmail.Text = "Email@example.com";
            // 
            // lblUsername
            // 
            lblUsername.ForeColor = Color.Gray;
            lblUsername.Location = new Point(35, 215);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(100, 23);
            lblUsername.TabIndex = 1;
            lblUsername.Text = "@tag";
            // 
            // lblNameAge
            // 
            lblNameAge.AutoSize = true;
            lblNameAge.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblNameAge.Location = new Point(30, 185);
            lblNameAge.Name = "lblNameAge";
            lblNameAge.Size = new Size(128, 32);
            lblNameAge.TabIndex = 2;
            lblNameAge.Text = "Username";
            // 
            // pbAvatar
            // 
            pbAvatar.BackColor = Color.DarkGray;
            pbAvatar.Location = new Point(30, 60);
            pbAvatar.Name = "pbAvatar";
            pbAvatar.Size = new Size(120, 120);
            pbAvatar.SizeMode = PictureBoxSizeMode.StretchImage;
            pbAvatar.TabIndex = 3;
            pbAvatar.TabStop = false;
            // 
            // pnlPinkBanner
            // 
            pnlPinkBanner.BackColor = Color.FromArgb(243, 66, 140);
            pnlPinkBanner.Dock = DockStyle.Top;
            pnlPinkBanner.Location = new Point(0, 0);
            pnlPinkBanner.Name = "pnlPinkBanner";
            pnlPinkBanner.Size = new Size(1000, 120);
            pnlPinkBanner.TabIndex = 4;
            // 
            // flpContent
            // 
            flpContent.AutoScroll = true;
            flpContent.Controls.Add(pnlIntro);
            flpContent.Controls.Add(tlpStats);
            flpContent.Controls.Add(tlpBottomGrid);
            flpContent.Dock = DockStyle.Fill;
            flpContent.FlowDirection = FlowDirection.TopDown;
            flpContent.Location = new Point(0, 250);
            flpContent.Name = "flpContent";
            flpContent.Padding = new Padding(20, 10, 20, 10);
            flpContent.Size = new Size(1000, 500);
            flpContent.TabIndex = 0;
            flpContent.WrapContents = false;
            // 
            // pnlIntro
            // 
            pnlIntro.Controls.Add(lblBio);
            pnlIntro.Controls.Add(lblBioHead);
            pnlIntro.Location = new Point(20, 20);
            pnlIntro.Margin = new Padding(0, 10, 0, 10);
            pnlIntro.Name = "pnlIntro";
            pnlIntro.Size = new Size(940, 80);
            pnlIntro.TabIndex = 0;
            // 
            // lblBio
            // 
            lblBio.Location = new Point(25, 45);
            lblBio.Name = "lblBio";
            lblBio.Size = new Size(90, 23);
            lblBio.TabIndex = 0;
            lblBio.Text = "Tôi là.....";
            // 
            // lblBioHead
            // 
            lblBioHead.Location = new Point(25, 12);
            lblBioHead.Name = "lblBioHead";
            lblBioHead.Size = new Size(90, 23);
            lblBioHead.TabIndex = 0;
            lblBioHead.Text = "Giới thiệu";
            // 
            // tlpStats
            // 
            tlpStats.ColumnCount = 3;
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpStats.Controls.Add(pnlConnect, 2, 0);
            tlpStats.Controls.Add(pnlCare, 1, 0);
            tlpStats.Controls.Add(pnlGender, 0, 0);
            tlpStats.Location = new Point(23, 113);
            tlpStats.Name = "tlpStats";
            tlpStats.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tlpStats.Size = new Size(940, 100);
            tlpStats.TabIndex = 2;
            // 
            // pnlConnect
            // 
            pnlConnect.Controls.Add(label3);
            pnlConnect.Controls.Add(lblConnectHead);
            pnlConnect.Dock = DockStyle.Fill;
            pnlConnect.Location = new Point(629, 3);
            pnlConnect.Name = "pnlConnect";
            pnlConnect.Size = new Size(308, 94);
            pnlConnect.TabIndex = 8;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(52, 67);
            label3.Name = "label3";
            label3.Size = new Size(88, 15);
            label3.TabIndex = 0;
            label3.Text = "#So lan ket noi ";
            label3.TextAlign = ContentAlignment.TopCenter;
            // 
            // lblConnectHead
            // 
            lblConnectHead.AutoSize = true;
            lblConnectHead.Location = new Point(86, 28);
            lblConnectHead.Name = "lblConnectHead";
            lblConnectHead.Size = new Size(44, 15);
            lblConnectHead.TabIndex = 0;
            lblConnectHead.Text = "Kết nối";
            lblConnectHead.TextAlign = ContentAlignment.TopCenter;
            // 
            // pnlCare
            // 
            pnlCare.Controls.Add(label2);
            pnlCare.Controls.Add(lblCareHead);
            pnlCare.Dock = DockStyle.Fill;
            pnlCare.Location = new Point(316, 3);
            pnlCare.Name = "pnlCare";
            pnlCare.Size = new Size(307, 94);
            pnlCare.TabIndex = 7;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(91, 52);
            label2.Name = "label2";
            label2.Size = new Size(115, 15);
            label2.TabIndex = 0;
            label2.Text = "#gioi tinh quan tam ";
            label2.TextAlign = ContentAlignment.TopCenter;
            // 
            // lblCareHead
            // 
            lblCareHead.AutoSize = true;
            lblCareHead.Location = new Point(91, 19);
            lblCareHead.Name = "lblCareHead";
            lblCareHead.Size = new Size(83, 15);
            lblCareHead.TabIndex = 0;
            lblCareHead.Text = "Quan tâm đến";
            lblCareHead.TextAlign = ContentAlignment.TopCenter;
            // 
            // pnlGender
            // 
            pnlGender.Controls.Add(lblGender);
            pnlGender.Controls.Add(lblGenderHead);
            pnlGender.Dock = DockStyle.Fill;
            pnlGender.Location = new Point(3, 3);
            pnlGender.Name = "pnlGender";
            pnlGender.Size = new Size(307, 94);
            pnlGender.TabIndex = 6;
            // 
            // lblGender
            // 
            lblGender.AutoSize = true;
            lblGender.Location = new Point(51, 52);
            lblGender.Name = "lblGender";
            lblGender.Size = new Size(58, 15);
            lblGender.TabIndex = 0;
            lblGender.Text = "#gioi tinh";
            lblGender.TextAlign = ContentAlignment.TopCenter;
            // 
            // lblGenderHead
            // 
            lblGenderHead.AutoSize = true;
            lblGenderHead.Location = new Point(51, 19);
            lblGenderHead.Name = "lblGenderHead";
            lblGenderHead.Size = new Size(52, 15);
            lblGenderHead.TabIndex = 0;
            lblGenderHead.Text = "Giới tính";
            lblGenderHead.TextAlign = ContentAlignment.TopCenter;
            // 
            // tlpBottomGrid
            // 
            tlpBottomGrid.ColumnCount = 2;
            tlpBottomGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpBottomGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpBottomGrid.Controls.Add(panel2, 1, 0);
            tlpBottomGrid.Controls.Add(panel1, 0, 0);
            tlpBottomGrid.Location = new Point(23, 219);
            tlpBottomGrid.Name = "tlpBottomGrid";
            tlpBottomGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tlpBottomGrid.Size = new Size(940, 500);
            tlpBottomGrid.TabIndex = 3;
            // 
            // panel1
            // 
            panel1.Controls.Add(lblMyPicture);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(464, 494);
            panel1.TabIndex = 7;
            // 
            // panel2
            // 
            panel2.Controls.Add(lblMyPostHead);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(473, 3);
            panel2.Name = "panel2";
            panel2.Size = new Size(464, 494);
            panel2.TabIndex = 8;
            // 
            // lblMyPicture
            // 
            lblMyPicture.AutoSize = true;
            lblMyPicture.Location = new Point(35, 12);
            lblMyPicture.Name = "lblMyPicture";
            lblMyPicture.Size = new Size(68, 15);
            lblMyPicture.TabIndex = 0;
            lblMyPicture.Text = "Ảnh của tôi";
            // 
            // lblMyPostHead
            // 
            lblMyPostHead.AutoSize = true;
            lblMyPostHead.Location = new Point(17, 23);
            lblMyPostHead.Name = "lblMyPostHead";
            lblMyPostHead.Size = new Size(92, 15);
            lblMyPostHead.TabIndex = 0;
            lblMyPostHead.Text = "Bài đăng của tôi";
            // 
            // Profile
            // 
            ClientSize = new Size(1000, 750);
            Controls.Add(flpContent);
            Controls.Add(pnlHeader);
            Name = "Profile";
            Text = "Hồ sơ cá nhân";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pbAvatar).EndInit();
            flpContent.ResumeLayout(false);
            pnlIntro.ResumeLayout(false);
            tlpStats.ResumeLayout(false);
            pnlConnect.ResumeLayout(false);
            pnlConnect.PerformLayout();
            pnlCare.ResumeLayout(false);
            pnlCare.PerformLayout();
            pnlGender.ResumeLayout(false);
            pnlGender.PerformLayout();
            tlpBottomGrid.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlPinkBanner;
        private System.Windows.Forms.PictureBox pbAvatar;
        private System.Windows.Forms.Label lblNameAge;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.FlowLayoutPanel flpContent;
        private System.Windows.Forms.Panel pnlIntro;
        private System.Windows.Forms.TableLayoutPanel tlpStats;
        private System.Windows.Forms.TableLayoutPanel tlpBottomGrid;
        private Label lblEmailHead;
        private Label lblAgeHead;
        private Label lblAge;
        private Label lblBio;
        private Label lblBioHead;
        private Panel pnlConnect;
        private Panel pnlCare;
        private Panel pnlGender;
        private Button btnEdit;
        private Label lblCareHead;
        private Label lblGenderHead;
        private Label lblConnectHead;
        private Label label3;
        private Label label2;
        private Label lblGender;
        private Panel panel2;
        private Label lblMyPostHead;
        private Panel panel1;
        private Label lblMyPicture;
    }
}