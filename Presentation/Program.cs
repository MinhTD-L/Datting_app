using DataTransferObject;
using System.Net.Sockets;

namespace Presentation
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main()
        {
            ApplicationConfiguration.Initialize();

            if (SessionManager.IsLoggedIn())
            {
                Application.Run(new MainDashboard());
            }
            else
            {
                Application.Run(new Login());
            }
        }
        public static class SocketManager
        {
            public static ChatSocketDAL Socket { get; } = new ChatSocketDAL();
        }

    }
}