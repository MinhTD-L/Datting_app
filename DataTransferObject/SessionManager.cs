using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObject
{
    public static class SessionManager
    {
        public static string Token { get; set; }
        public static string UserId { get; set; }
        public static string Username { get; set; }

        public static bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(Token);
        }
        public static void Clear()
        {
            Token = string.Empty;
            UserId = string.Empty;
            Username = string.Empty;
        }
    }
}
