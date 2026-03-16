using DataTransferObject;
using DataAccess;
namespace Presentation
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            LoginInputDTO dTO = new LoginInputDTO();
            dTO.Email = txtEmail.Text;
            dTO.Password = txtPassword.Text;
            var dal = new UserDAL();
            var result = await dal.Login(dTO);

            if (result == null)
            {
                MessageBox.Show("API returned null");
                return;
            }

            if(result.Status == "success")
            {
                SessionManager.Token = result.Token;
                SessionManager.UserId=result.User.Id;
                SessionManager.Username=result.User.Username;
                MainDashboard main=new MainDashboard();
                main.Show();
                this.Hide();
            }
            else if(result.Status != "success")
            {
                MessageBox.Show(result.Error);
            }
        }

        private void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Register register = new Register();
            register.Show();
            this.Hide();
        }
    }
}
