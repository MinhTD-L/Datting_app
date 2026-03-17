namespace Presentation
{
    partial class MainDashboard
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlSidebar;
        private Panel pnlHeader;
        private Panel pnlRightSidebar;
        private Panel pnlContent;

        private FlowLayoutPanel pnlSuggested;

        private Label lblLogo;
        private Label lblSuggested;

        private Button btnHome;
        private Button btnMatch;
        private Button btnFriend;
        private Button btnMessages;
        private Button btnProfile;
        private Button btnCreatePost;

        private TextBox txtSearch;

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
            pnlSidebar = new Panel();
            lblLogo = new Label();
            btnHome = new Button();
            btnMatch = new Button();
            btnFriend = new Button();
            btnMessages = new Button();
            btnProfile = new Button();
            btnCreatePost = new Button();
            pnlHeader = new Panel();
            txtSearch = new TextBox();
            pnlRightSidebar = new Panel();
            pnlSuggested = new FlowLayoutPanel();
            lblSuggested = new Label();
            pnlContent = new Panel();
            PostFeed = new FlowLayoutPanel();
            pnlSidebar.SuspendLayout();
            pnlHeader.SuspendLayout();
            pnlRightSidebar.SuspendLayout();
            pnlSuggested.SuspendLayout();
            pnlContent.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = Color.White;
            pnlSidebar.Controls.Add(lblLogo);
            pnlSidebar.Controls.Add(btnHome);
            pnlSidebar.Controls.Add(btnMatch);
            pnlSidebar.Controls.Add(btnFriend);
            pnlSidebar.Controls.Add(btnMessages);
            pnlSidebar.Controls.Add(btnProfile);
            pnlSidebar.Controls.Add(btnCreatePost);
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Location = new Point(0, 0);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(250, 750);
            pnlSidebar.TabIndex = 3;
            // 
            // lblLogo
            // 
            lblLogo.AutoSize = true;
            lblLogo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblLogo.ForeColor = Color.FromArgb(230, 30, 100);
            lblLogo.Location = new Point(40, 20);
            lblLogo.Name = "lblLogo";
            lblLogo.Size = new Size(202, 32);
            lblLogo.TabIndex = 0;
            lblLogo.Text = "❤ LoveConnect";
            // 
            // btnHome
            // 
            btnHome.Location = new Point(25, 100);
            btnHome.Name = "btnHome";
            btnHome.Size = new Size(200, 45);
            btnHome.TabIndex = 1;
            btnHome.Text = "Trang chủ";
            btnHome.Click += btnHome_Click;
            // 
            // btnMatch
            // 
            btnMatch.Location = new Point(25, 160);
            btnMatch.Name = "btnMatch";
            btnMatch.Size = new Size(200, 45);
            btnMatch.TabIndex = 2;
            btnMatch.Text = "Ghép đôi";
            // 
            // btnFriend
            // 
            btnFriend.Location = new Point(25, 220);
            btnFriend.Name = "btnFriend";
            btnFriend.Size = new Size(200, 45);
            btnFriend.TabIndex = 3;
            btnFriend.Text = "Bạn bè";
            // 
            // btnMessages
            // 
            btnMessages.Location = new Point(25, 280);
            btnMessages.Name = "btnMessages";
            btnMessages.Size = new Size(200, 45);
            btnMessages.TabIndex = 4;
            btnMessages.Text = "Tin nhắn";
            // 
            // btnProfile
            // 
            btnProfile.Location = new Point(25, 340);
            btnProfile.Name = "btnProfile";
            btnProfile.Size = new Size(200, 45);
            btnProfile.TabIndex = 5;
            btnProfile.Text = "Trang cá nhân";
            btnProfile.Click += btnProfile_Click;
            // 
            // btnCreatePost
            // 
            btnCreatePost.BackColor = Color.FromArgb(255, 30, 100);
            btnCreatePost.FlatAppearance.BorderSize = 0;
            btnCreatePost.FlatStyle = FlatStyle.Flat;
            btnCreatePost.ForeColor = Color.White;
            btnCreatePost.Location = new Point(25, 500);
            btnCreatePost.Name = "btnCreatePost";
            btnCreatePost.Size = new Size(200, 45);
            btnCreatePost.TabIndex = 6;
            btnCreatePost.Text = "➕ Create Post";
            btnCreatePost.UseVisualStyleBackColor = false;
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.White;
            pnlHeader.Controls.Add(txtSearch);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(250, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1030, 70);
            pnlHeader.TabIndex = 2;
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.BackColor = Color.FromArgb(243, 244, 248);
            txtSearch.BorderStyle = BorderStyle.None;
            txtSearch.Font = new Font("Segoe UI", 11F);
            txtSearch.Location = new Point(30, 25);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "🔍 Search people, interests, locations...";
            txtSearch.Size = new Size(1430, 25);
            txtSearch.TabIndex = 0;
            // 
            // pnlRightSidebar
            // 
            pnlRightSidebar.BackColor = Color.FromArgb(250, 248, 253);
            pnlRightSidebar.Controls.Add(pnlSuggested);
            pnlRightSidebar.Dock = DockStyle.Right;
            pnlRightSidebar.Location = new Point(1000, 70);
            pnlRightSidebar.Name = "pnlRightSidebar";
            pnlRightSidebar.Size = new Size(280, 680);
            pnlRightSidebar.TabIndex = 1;
            // 
            // pnlSuggested
            // 
            pnlSuggested.BackColor = Color.White;
            pnlSuggested.Controls.Add(lblSuggested);
            pnlSuggested.Location = new Point(20, 20);
            pnlSuggested.Name = "pnlSuggested";
            pnlSuggested.Size = new Size(240, 648);
            pnlSuggested.TabIndex = 0;
            // 
            // lblSuggested
            // 
            lblSuggested.AutoSize = true;
            lblSuggested.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSuggested.Location = new Point(3, 0);
            lblSuggested.Name = "lblSuggested";
            lblSuggested.Size = new Size(157, 23);
            lblSuggested.TabIndex = 0;
            lblSuggested.Text = "Suggested for You";
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.FromArgb(245, 246, 250);
            pnlContent.Controls.Add(PostFeed);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(250, 70);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(750, 680);
            pnlContent.TabIndex = 0;
            // 
            // PostFeed
            // 
            PostFeed.AutoScroll = true;
            PostFeed.BackColor = Color.Gainsboro;
            PostFeed.Dock = DockStyle.Fill;
            PostFeed.FlowDirection = FlowDirection.TopDown;
            PostFeed.Location = new Point(0, 0);
            PostFeed.Name = "PostFeed";
            PostFeed.Padding = new Padding(20);
            PostFeed.Size = new Size(750, 680);
            PostFeed.TabIndex = 0;
            PostFeed.WrapContents = false;
            // 
            // MainDashboard
            // 
            ClientSize = new Size(1280, 750);
            Controls.Add(pnlContent);
            Controls.Add(pnlRightSidebar);
            Controls.Add(pnlHeader);
            Controls.Add(pnlSidebar);
            Name = "MainDashboard";
            Text = "LoveConnect Dashboard";
            Load += MainDashboard_Load_1;
            pnlSidebar.ResumeLayout(false);
            pnlSidebar.PerformLayout();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlRightSidebar.ResumeLayout(false);
            pnlSuggested.ResumeLayout(false);
            pnlSuggested.PerformLayout();
            pnlContent.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void StyleMenuButton(Button btn)
        {
            btn.BackColor = Color.FromArgb(255, 30, 100);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        }
    }
}