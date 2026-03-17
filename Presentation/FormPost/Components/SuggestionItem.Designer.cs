namespace Presentation.FormPost.Components
{
    partial class SuggestionItem
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pbAvatar = new PictureBox();
            lblName = new Label();
            lblInfo = new Label();
            btnConnect = new RoundedButton();
            ((System.ComponentModel.ISupportInitialize)pbAvatar).BeginInit();
            SuspendLayout();
            // 
            // pbAvatar
            // 
            pbAvatar.BackColor = Color.Gainsboro;
            pbAvatar.Location = new Point(6, 6);
            pbAvatar.Margin = new Padding(4, 3, 4, 3);
            pbAvatar.Name = "pbAvatar";
            pbAvatar.Size = new Size(47, 46);
            pbAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            pbAvatar.TabIndex = 0;
            pbAvatar.TabStop = false;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblName.Location = new Point(58, 8);
            lblName.Margin = new Padding(4, 0, 4, 0);
            lblName.Name = "lblName";
            lblName.Size = new Size(94, 15);
            lblName.TabIndex = 1;
            lblName.Text = "Tên người dùng";
            // 
            // lblInfo
            // 
            lblInfo.AutoSize = true;
            lblInfo.Font = new Font("Segoe UI", 8F);
            lblInfo.ForeColor = Color.Gray;
            lblInfo.Location = new Point(58, 29);
            lblInfo.Margin = new Padding(4, 0, 4, 0);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(74, 13);
            lblInfo.TabIndex = 2;
            lblInfo.Text = "28 tuổi • 3km";
            // 
            // btnConnect
            // 
            btnConnect.BackColor = Color.FromArgb(230, 30, 100);
            btnConnect.FlatAppearance.BorderSize = 0;
            btnConnect.FlatStyle = FlatStyle.Flat;
            btnConnect.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnConnect.ForeColor = Color.White;
            btnConnect.Location = new Point(6, 58);
            btnConnect.Margin = new Padding(4, 3, 4, 3);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(235, 35);
            btnConnect.TabIndex = 3;
            btnConnect.Text = "Kết nối";
            btnConnect.UseVisualStyleBackColor = false;
            // 
            // SuggestionItem
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(btnConnect);
            Controls.Add(lblInfo);
            Controls.Add(lblName);
            Controls.Add(pbAvatar);
            Margin = new Padding(0, 0, 0, 6);
            Name = "SuggestionItem";
            Size = new Size(245, 102);
            ((System.ComponentModel.ISupportInitialize)pbAvatar).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.PictureBox pbAvatar;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblInfo;
        private RoundedButton btnConnect;
    }
}