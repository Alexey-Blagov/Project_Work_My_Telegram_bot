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

        // получение роли юзера
        //public static async Task <User> GetUserRole(long TgId)
        //{
        //    using (ApplicationContext db = new ApplicationContext())
        //    {
        //        var user = await db.UserRoles.FirstOrDefaultAsync(x => x.TgId == TgId);
        //        return user;
        //    }
        //}
    }
}
