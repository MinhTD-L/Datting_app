using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Presentation.FormAuth
{
    public partial class Verify : Form
    {
        private string _targetEmail;
        private string _correctCode = "123456"; // Mã mẫu
        public Verify(string email)
        {
            InitializeComponent();
            _targetEmail = email;
            lblEmail.Text = _targetEmail; // Hiển thị email người dùng vừa nhập
        }

        private void lblReSend_Click(object sender, EventArgs e)
        {
            // gửi lại code xác thực
        }

        private void btnVerify_Click(object sender, EventArgs e)
        {
            if (txtVerifyCode.Text == _correctCode)
            {
                this.DialogResult = DialogResult.OK; // Trả về kết quả Thành công
                this.Close();
            }
            else
            {
                MessageBox.Show("Mã xác thực không đúng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
