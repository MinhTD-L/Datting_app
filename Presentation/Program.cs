using DataTransferObject;

namespace Presentation
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
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
    }
}