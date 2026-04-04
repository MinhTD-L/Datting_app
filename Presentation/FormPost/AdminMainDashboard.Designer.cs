namespace Presentation.FormAdmin
{
    partial class AdminMainDashboard
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlHeader;
        private Panel pnlContent;
        private Panel pnlBottomNav;

        private Label lblLogo;
        private Button btnNotification;

        private Button btnUsers;
        private Button btnReports;
        private Button btnStats;

        private Panel pnlCreatePost;
        private PictureBox picUserAvatar;
        private Button btnFakeInputText;

        private FlowLayoutPanel PostFeed;

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
            lblLogo = new Label();
            btnUsers = new Button();
            btnReports = new Button();
            btnStats = new Button();
            pnlHeader = new Panel();
            btnNotification = new Button();
            pnlContent = new Panel();
            PostFeed = new FlowLayoutPanel();
            pnlCreatePost = new Panel();
            picUserAvatar = new PictureBox();
            btnFakeInputText = new Button();
            pnlBottomNav = new Panel();
            button1 = new Button();
            pnlHeader.SuspendLayout();
            pnlContent.SuspendLayout();
            pnlCreatePost.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picUserAvatar).BeginInit();
            pnlBottomNav.SuspendLayout();
            SuspendLayout();
            // 
            // lblLogo
            // 
            lblLogo.Anchor = AnchorStyles.None;
            lblLogo.AutoSize = true;
            lblLogo.Cursor = Cursors.Hand;
            lblLogo.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblLogo.ForeColor = Color.FromArgb(230, 30, 100);
            lblLogo.Location = new Point(540, 15);
            lblLogo.Name = "lblLogo";
            lblLogo.Size = new Size(228, 37);
            lblLogo.TabIndex = 0;
            lblLogo.Text = "❤ LoveConnect Admin";
            lblLogo.Click += btnHome_Click;
            // 
            // btnUsers
            // 
            btnUsers.Anchor = AnchorStyles.Top;
            btnUsers.Cursor = Cursors.Hand;
            btnUsers.FlatAppearance.BorderSize = 0;
            btnUsers.FlatStyle = FlatStyle.Flat;
            btnUsers.Font = new Font("Segoe UI Emoji", 16F);
            btnUsers.Location = new Point(480, 10);
            btnUsers.Name = "btnUsers";
            btnUsers.Size = new Size(60, 40);
            btnUsers.TabIndex = 2;
            btnUsers.Text = "👥";
            // 
            // btnReports
            // 
            btnReports.Anchor = AnchorStyles.Top;
            btnReports.Cursor = Cursors.Hand;
            btnReports.FlatAppearance.BorderSize = 0;
            btnReports.FlatStyle = FlatStyle.Flat;
            btnReports.Font = new Font("Segoe UI Emoji", 16F);
            btnReports.Location = new Point(560, 10);
            btnReports.Name = "btnReports";
            btnReports.Size = new Size(60, 40);
            btnReports.TabIndex = 3;
            btnReports.Text = "🚩";
            // 
            // btnStats
            // 
            btnStats.Anchor = AnchorStyles.Top;
            btnStats.Cursor = Cursors.Hand;
            btnStats.FlatAppearance.BorderSize = 0;
            btnStats.FlatStyle = FlatStyle.Flat;
            btnStats.Font = new Font("Segoe UI Emoji", 16F);
            btnStats.Location = new Point(640, 10);
            btnStats.Name = "btnStats";
            btnStats.Size = new Size(60, 40);
            btnStats.TabIndex = 4;
            btnStats.Text = "📊";
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(btnNotification);
            pnlHeader.Controls.Add(lblLogo);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1280, 60);
            pnlHeader.TabIndex = 2;
            // 
            // btnNotification
            // 
            btnNotification.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNotification.BackColor = Color.Transparent;
            btnNotification.Cursor = Cursors.Hand;
            btnNotification.FlatAppearance.BorderSize = 0;
            btnNotification.FlatAppearance.MouseDownBackColor = Color.FromArgb(232, 232, 232);
            btnNotification.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
            btnNotification.FlatStyle = FlatStyle.Flat;
            btnNotification.Font = new Font("Segoe UI Emoji", 14F);
            btnNotification.ForeColor = Color.FromArgb(70, 70, 70);
            btnNotification.Location = new Point(1220, 10);
            btnNotification.Name = "btnNotification";
            btnNotification.Size = new Size(40, 40);
            btnNotification.TabIndex = 1;
            btnNotification.Text = "🔔";
            btnNotification.UseVisualStyleBackColor = false;
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.FromArgb(245, 246, 250);
            pnlContent.Controls.Add(PostFeed);
            pnlContent.Controls.Add(pnlCreatePost);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 60);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(1280, 630);
            pnlContent.TabIndex = 0;
            // 
            // PostFeed
            // 
            PostFeed.AutoScroll = true;
            PostFeed.BackColor = Color.Gainsboro;
            PostFeed.Dock = DockStyle.Fill;
            PostFeed.FlowDirection = FlowDirection.TopDown;
            PostFeed.Location = new Point(0, 80);
            PostFeed.Name = "PostFeed";
            PostFeed.Padding = new Padding(20);
            PostFeed.Size = new Size(1280, 550);
            PostFeed.TabIndex = 0;
            PostFeed.WrapContents = false;
            // 
            // pnlCreatePost
            // 
            pnlCreatePost.BackColor = Color.White;
            pnlCreatePost.Controls.Add(picUserAvatar);
            pnlCreatePost.Controls.Add(btnFakeInputText);
            pnlCreatePost.Dock = DockStyle.Top;
            pnlCreatePost.Location = new Point(0, 0);
            pnlCreatePost.Name = "pnlCreatePost";
            pnlCreatePost.Size = new Size(1280, 80);
            pnlCreatePost.TabIndex = 1;
            // 
            // picUserAvatar
            // 
            picUserAvatar.Anchor = AnchorStyles.Top;
            picUserAvatar.BackColor = Color.FromArgb(238, 238, 238);
            picUserAvatar.Location = new Point(300, 15);
            picUserAvatar.Name = "picUserAvatar";
            picUserAvatar.Size = new Size(50, 50);
            picUserAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            picUserAvatar.TabIndex = 0;
            picUserAvatar.TabStop = false;
            // 
            // btnFakeInputText
            // 
            btnFakeInputText.Anchor = AnchorStyles.Top;
            btnFakeInputText.BackColor = Color.FromArgb(240, 242, 245);
            btnFakeInputText.Cursor = Cursors.Hand;
            btnFakeInputText.FlatAppearance.BorderSize = 0;
            btnFakeInputText.FlatStyle = FlatStyle.Flat;
            btnFakeInputText.Font = new Font("Segoe UI", 12F);
            btnFakeInputText.ForeColor = Color.Gray;
            btnFakeInputText.Location = new Point(365, 15);
            btnFakeInputText.Name = "btnFakeInputText";
            btnFakeInputText.Padding = new Padding(15, 0, 0, 0);
            btnFakeInputText.Size = new Size(615, 50);
            btnFakeInputText.TabIndex = 1;
            btnFakeInputText.Text = "Bạn đang nghĩ gì thế?";
            btnFakeInputText.TextAlign = ContentAlignment.MiddleLeft;
            btnFakeInputText.UseVisualStyleBackColor = false;
            // 
            // pnlBottomNav
            // 
            pnlBottomNav.BackColor = Color.White;
            pnlBottomNav.Controls.Add(button1);
            pnlBottomNav.Controls.Add(btnUsers);
            pnlBottomNav.Controls.Add(btnReports);
            pnlBottomNav.Controls.Add(btnStats);
            pnlBottomNav.Dock = DockStyle.Bottom;
            pnlBottomNav.Location = new Point(0, 690);
            pnlBottomNav.Name = "pnlBottomNav";
            pnlBottomNav.Size = new Size(1280, 60);
            pnlBottomNav.TabIndex = 4;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top;
            button1.Cursor = Cursors.Hand;
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Segoe UI Emoji", 16F);
            button1.Location = new Point(786, 10);
            button1.Name = "button1";
            button1.Size = new Size(60, 40);
            button1.TabIndex = 6;
            button1.Text = "➜]";
            button1.Click += button1_Click;
            // 
            // AdminMainDashboard
            // 
            ClientSize = new Size(1280, 750);
            Controls.Add(pnlContent);
            Controls.Add(pnlBottomNav);
            Controls.Add(pnlHeader);
            Name = "AdminMainDashboard";
            Text = "LoveConnect Admin Dashboard";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlContent.ResumeLayout(false);
            pnlCreatePost.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picUserAvatar).EndInit();
            pnlBottomNav.ResumeLayout(false);
            ResumeLayout(false);
        }
        private Button button1;
    }
}