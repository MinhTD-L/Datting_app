﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessLogic;
using DataTransferObject;

namespace Presentation.FormAuth
{
    public partial class Verify : Form
    {
        private string _targetEmail;
        private readonly UserBLL _userBll;

        public Verify(string email)
        {
            InitializeComponent();
            _targetEmail = email;
            _userBll = BusinessLogic.AppServices.UserBll;
            lblEmail.Text = _targetEmail; // Hiển thị email người dùng vừa nhập
        }

        private void lblReSend_Click(object sender, EventArgs e)
        {
            // gửi lại code xác thực
        }

        private async void btnVerify_Click(object sender, EventArgs e)
        {
            var code = txtVerifyCode.Text?.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Vui lòng nhập mã xác thực!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnVerify.Enabled = false;
                var dto = new verifyEmailDTO
                {
                    Email = _targetEmail,
                    Code = code
                };

                await _userBll.VerifyEmailAsync(dto);
                
                this.DialogResult = DialogResult.OK; 
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xác thực thất bại: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnVerify.Enabled = true;
            }
        }
    }
}
