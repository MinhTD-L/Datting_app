namespace Presentation.FormAuth
{
    partial class Verify
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            picVerify = new PictureBox();
            lbl1 = new Label();
            label1 = new Label();
            lblEmail = new Label();
            label2 = new Label();
            btnVerify = new RoundedButton();
            txtVerifyCode = new RoundedTextBox();
            lblReSend = new Label();
            ((System.ComponentModel.ISupportInitialize)picVerify).BeginInit();
            SuspendLayout();
            // 
            // picVerify
            // 
            picVerify.Image = Properties.Resources.VerifyImg;
            picVerify.Location = new Point(315, 40);
            picVerify.Name = "picVerify";
            picVerify.Size = new Size(175, 92);
            picVerify.SizeMode = PictureBoxSizeMode.Zoom;
            picVerify.TabIndex = 0;
            picVerify.TabStop = false;
            // 
            // lbl1
            // 
            lbl1.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold);
            lbl1.Location = new Point(0, 145);
            lbl1.Name = "lbl1";
            lbl1.Size = new Size(792, 35);
            lbl1.TabIndex = 9;
            lbl1.Text = "Kiểm tra Email của bạn";
            lbl1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            label1.Font = new Font("Segoe UI", 10F);
            label1.Location = new Point(0, 190);
            label1.Name = "label1";
            label1.Size = new Size(792, 20);
            label1.TabIndex = 8;
            label1.Text = "Mã xác thực đã được gửi đến";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblEmail
            // 
            lblEmail.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblEmail.ForeColor = Color.DodgerBlue;
            lblEmail.Location = new Point(0, 215);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(792, 20);
            lblEmail.TabIndex = 7;
            lblEmail.Text = "@Email";
            lblEmail.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            label2.Location = new Point(0, 245);
            label2.Name = "label2";
            label2.Size = new Size(792, 20);
            label2.TabIndex = 6;
            label2.Text = "Nhập mã vào đây";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnVerify
            // 
            btnVerify.BackColor = Color.FromArgb(76, 175, 80);
            btnVerify.FlatStyle = FlatStyle.Flat;
            btnVerify.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnVerify.ForeColor = Color.White;
            btnVerify.Location = new Point(337, 330);
            btnVerify.Name = "btnVerify";
            btnVerify.Size = new Size(123, 40);
            btnVerify.TabIndex = 2;
            btnVerify.Text = "Xác nhận";
            btnVerify.UseVisualStyleBackColor = false;
            btnVerify.Click += btnVerify_Click;
            // 
            // txtVerifyCode
            // 
            txtVerifyCode.BackColor = Color.White;
            txtVerifyCode.Font = new Font("Segoe UI", 14F);
            txtVerifyCode.Location = new Point(291, 275);
            txtVerifyCode.Name = "txtVerifyCode";
            txtVerifyCode.Size = new Size(211, 35);
            txtVerifyCode.TabIndex = 4;
            txtVerifyCode.UseSystemPasswordChar = false;
            // 
            // lblReSend
            // 
            lblReSend.Cursor = Cursors.Hand;
            lblReSend.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
            lblReSend.ForeColor = Color.Gray;
            lblReSend.Location = new Point(0, 385);
            lblReSend.Name = "lblReSend";
            lblReSend.Size = new Size(792, 20);
            lblReSend.TabIndex = 5;
            lblReSend.Text = "Tôi chưa nhận được mã? Gửi lại";
            lblReSend.TextAlign = ContentAlignment.MiddleCenter;
            lblReSend.Click += lblReSend_Click;
            // 
            // Verify
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(235, 243, 254);
            ClientSize = new Size(792, 450);
            Controls.Add(txtVerifyCode);
            Controls.Add(lblReSend);
            Controls.Add(btnVerify);
            Controls.Add(label2);
            Controls.Add(lblEmail);
            Controls.Add(label1);
            Controls.Add(lbl1);
            Controls.Add(picVerify);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "Verify";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Xác thực tài khoản";
            ((System.ComponentModel.ISupportInitialize)picVerify).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox picVerify;
        private Label lbl1;
        private Label label1;
        private Label lblEmail;
        private Label label2;
        private Label lblReSend;
        private RoundedTextBox txtVerifyCode;
        private RoundedButton btnVerify;
    }
}