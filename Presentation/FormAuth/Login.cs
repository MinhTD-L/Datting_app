using BusinessLogic;
using DataTransferObject;
namespace Presentation
{
    public partial class Login : Form
    {
        private readonly UserBLL _userBll;
        private readonly ChatBLL _chatBll;

        public Login()
        {
            InitializeComponent();
            _userBll = BusinessLogic.AppServices.UserBll;
            _chatBll = BusinessLogic.AppServices.ChatBll;
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
                try
                {
                    await _chatBll.EnsureConnectedAsync(SessionManager.Token);
                }
                catch
                {
                    // ignore: chat can reconnect later when opening messages
                }
                
                // Kiểm tra Role của User
                if (!string.IsNullOrEmpty(result.User.Role) && result.User.Role.ToLower() == "admin")
                {
                    Presentation.FormAdmin.AdminMainDashboard adminForm = new Presentation.FormAdmin.AdminMainDashboard();
                    adminForm.Show();
                    this.Hide();
                }
                else
                {
                    MainDashboard main = new MainDashboard();
                    main.Show();
                    this.Hide();
                }
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
