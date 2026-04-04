namespace Presentation.FormProfile
{
    partial class UserProfile
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
            btnBack = new Button();
            btnEdit = new Button();
            lblNameAge = new Label();
            pbAvatar = new PictureBox();
            flpContent = new FlowLayoutPanel();
            pnlIntro = new Panel();
            lblBio = new Label();
            lblBioHead = new Label();
            flpMyPosts = new FlowLayoutPanel();
            lblMyPostHead = new Label();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbAvatar).BeginInit();
            pnlIntro.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(btnBack);
            pnlHeader.Controls.Add(btnEdit);
            pnlHeader.Controls.Add(lblNameAge);
            pnlHeader.Controls.Add(pbAvatar);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1000, 250);
            pnlHeader.TabIndex = 1;
            // 
            // btnBack
            // 
            btnBack.Location = new Point(0, 0);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(75, 23);
            btnBack.TabIndex = 0;
            btnBack.Text = "button1";
            btnBack.UseVisualStyleBackColor = true;
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
            // lblNameAge
            // 
            lblNameAge.AutoSize = true;
            lblNameAge.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblNameAge.Location = new Point(166, 60);
            lblNameAge.Name = "lblNameAge";
            lblNameAge.Size = new Size(158, 41);
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
            // flpContent
            // 
            flpContent.AutoScroll = true;
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
            // flpMyPosts
            // 
            flpMyPosts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flpMyPosts.AutoScroll = true;
            flpMyPosts.FlowDirection = FlowDirection.TopDown;
            flpMyPosts.Location = new Point(17, 50);
            flpMyPosts.Name = "flpMyPosts";
            flpMyPosts.Size = new Size(900, 430);
            flpMyPosts.TabIndex = 1;
            flpMyPosts.WrapContents = false;
            // 
            // lblMyPostHead
            // 
            lblMyPostHead.AutoSize = true;
            lblMyPostHead.Location = new Point(17, 23);
            lblMyPostHead.Name = "lblMyPostHead";
            lblMyPostHead.Size = new Size(117, 20);
            lblMyPostHead.TabIndex = 0;
            lblMyPostHead.Text = "Bài đăng";
            // 
            // UserProfile
            // 
            ClientSize = new Size(1000, 750);
            Controls.Add(flpContent);
            Controls.Add(pnlHeader);
            Name = "UserProfile";
            Text = "Hồ sơ người dùng";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pbAvatar).EndInit();
            pnlIntro.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.PictureBox pbAvatar;
        private System.Windows.Forms.Label lblNameAge;
        private System.Windows.Forms.FlowLayoutPanel flpContent;
        private System.Windows.Forms.Panel pnlIntro;
        private Label lblBio;
        private Label lblBioHead;
        private Button btnEdit;
        private Button btnBack;
        private Label lblMyPostHead;
        private FlowLayoutPanel flpMyPosts;
    }
}