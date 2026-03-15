namespace Presentation
{
    partial class Terms
    {
        private RoundedPanel pnlBackground;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.RichTextBox rtbContent;
        private RoundedButton btnClose;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Terms));
            pnlBackground = new RoundedPanel();
            rtbContent = new RichTextBox();
            lblTitle = new Label();
            btnClose = new RoundedButton();
            pnlBackground.SuspendLayout();
            SuspendLayout();
            // 
            // pnlBackground
            // 
            pnlBackground.BackColor = Color.White;
            pnlBackground.Controls.Add(rtbContent);
            pnlBackground.Controls.Add(lblTitle);
            pnlBackground.Controls.Add(btnClose);
            pnlBackground.Dock = DockStyle.Fill;
            pnlBackground.Location = new Point(10, 10);
            pnlBackground.Name = "pnlBackground";
            pnlBackground.Padding = new Padding(20);
            pnlBackground.Size = new Size(480, 580);
            pnlBackground.TabIndex = 0;
            // 
            // rtbContent
            // 
            rtbContent.BackColor = Color.White;
            rtbContent.BorderStyle = BorderStyle.None;
            rtbContent.Dock = DockStyle.Fill;
            rtbContent.Font = new Font("Segoe UI", 10F);
            rtbContent.Location = new Point(20, 70);
            rtbContent.Name = "rtbContent";
            rtbContent.ReadOnly = true;
            rtbContent.Size = new Size(440, 445);
            rtbContent.TabIndex = 0;
            rtbContent.Text = resources.GetString("rtbContent.Text");
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("Segoe UI Semibold", 16F);
            lblTitle.ForeColor = Color.FromArgb(255, 45, 120);
            lblTitle.Location = new Point(20, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(440, 50);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "ĐIỀU KHOẢN DỊCH VỤ";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.FromArgb(255, 45, 120);
            btnClose.Dock = DockStyle.Bottom;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(20, 515);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(440, 45);
            btnClose.TabIndex = 2;
            btnClose.Text = "Đã hiểu";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            // 
            // Terms
            // 
            BackColor = Color.FromArgb(255, 245, 247);
            ClientSize = new Size(500, 600);
            Controls.Add(pnlBackground);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Terms";
            Padding = new Padding(10);
            StartPosition = FormStartPosition.CenterParent;
            pnlBackground.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}