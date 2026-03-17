using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Presentation.FormPost.Components
{
    public partial class SuggestionItem : UserControl
    {
        public SuggestionItem()
        {
            InitializeComponent();
            ApplyRoundedAvatar();
        }

        // Tạo Property để đổ dữ liệu nhanh từ MainDashboard
        public void SetData(string name, string gender, int age, string distance, Image avatar = null)
        {
            lblName.Text = name;
            lblInfo.Text = $"{gender}, {age} tuổi • {distance}";
            if (avatar != null) pbAvatar.Image = avatar;
        }

        private void ApplyRoundedAvatar()
        {
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, pbAvatar.Width, pbAvatar.Height);
            pbAvatar.Region = new Region(path);
        }
    }
}