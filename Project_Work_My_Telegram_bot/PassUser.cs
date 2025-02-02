using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Work_My_Telegram_bot
{
    internal class PassUser
    {
        private string _passwordUser = "12345";
        private string _passwordAdmin = "qwerty"; 
        public readonly string token = "7516165506:AAHgVKs9K2zHsyKJqVwSFzY4D8BsDIpVLLE";
        public string PasswordUser
        {
            get
            {
                return _passwordUser;
            }
            set
            {
                _passwordUser = value;
            }
        }
        public string PasswordAdmin
        {
            get
            {
                return _passwordAdmin;
            }
            set
            {
                _passwordAdmin = value;
            }
        }
    }
}
