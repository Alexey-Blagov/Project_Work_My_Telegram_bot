using Microsoft.EntityFrameworkCore;
using Project_Work_My_Telegram_bot.ClassDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Requests;


namespace Project_Work_My_Telegram_bot
{
    public class DataBaseHandler
    {
        public void GetUser()
        {
            using (ApplicationContext db = new ApplicationContext())
            {

            };
        }
        public static async Task<UserType> GetUserRole(long IdTg)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var user = await db.User.FirstOrDefaultAsync(x => x.IdTg == IdTg);
                return (UserType)user.UserRol;
            }
        }
    }
}
