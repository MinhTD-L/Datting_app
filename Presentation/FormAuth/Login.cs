using BusinessLogic;
using DataTransferObject;
using static Presentation.Program;
namespace Presentation
{
    public partial class Login : Form
    {
        private readonly UserBLL _userBll;

        public Login()
        {
            InitializeComponent();
            _userBll = BusinessLogic.AppServices.UserBll;
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            LoginInputDTO dTO = new LoginInputDTO();
            dTO.Email = txtEmail.Text;
            dTO.Password = txtPassword.Text;
            var result = await _userBll.LoginAsync(dTO);

            if (result == null)
            {
                MessageBox.Show("API returned null");
                return;
            }

            if (result.Status == "success")
            {
                SessionManager.Token = result.Token;
                SessionManager.UserId = result.User.Id;
                SessionManager.Username = result.User.Username;
                await SocketManager.Socket.Connect(SessionManager.Token);
                MainDashboard main = new MainDashboard();
                main.Show();
                this.Hide();
            }
            else if (result.Status != "success")
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
