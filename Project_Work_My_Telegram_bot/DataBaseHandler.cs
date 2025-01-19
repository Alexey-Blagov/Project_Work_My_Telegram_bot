using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Work_My_Telegram_bot
{
    internal class DataBaseHandler
    {
        // получение роли юзера
        public static async Task <User> GetUserRole(long TgId)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var user = await db.UserRoles.FirstOrDefaultAsync(x => x.TgId == TgId);
                return user;
            }
        }
    }
}
