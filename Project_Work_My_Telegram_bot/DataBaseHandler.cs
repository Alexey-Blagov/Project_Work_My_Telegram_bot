using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project_Work_My_Telegram_bot.ClassDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Requests;


namespace Project_Work_My_Telegram_bot
{
    public class DataBaseHandler
    {
        public static async Task SetUserName(long IdTg, string name)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var user = await db.User.FirstOrDefaultAsync(x => x.IdTg == IdTg);
                if (user is null)
                {
                    User newuser = new User();
                    newuser!.UserName = name;
                    await db.AddAsync(newuser);
                }
                else
                {
                    user!.UserName = name;
                    db.User.Update(user);
                }
                await db.SaveChangesAsync();
            };
        }
        public static async Task SetObjectPath(ObjectPath ObjPath, )
        {
            using (ApplicationContext db = new ApplicationContext()) 
            {
                var user = await db.User.FirstOrDefaultAsync(x => x.IdTg == IdTg);
            }
        }
        public static async Task SetUserJobTitle(long IdTg, string jobTitle)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var user = await db.User.FirstOrDefaultAsync(x => x.IdTg == IdTg);
                if (user is null)
                {
                    User newuser = new User();
                    newuser!.JobTitlel = jobTitle;
                    await db.AddAsync(newuser);
                }
                else
                {
                    user!.JobTitlel = jobTitle;
                    db.User.Update(user);
                }
                await db.SaveChangesAsync();
            };
        }
        public static async Task<int> GetUserRole(long IdTg)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var user = await db.User.FirstOrDefaultAsync(x => x.IdTg == IdTg);
                // случай если нет юзера в БД возврат Non 
                if (user == null) return 0;

                return user.UserRol;
            }
        }
        public static async Task SetUserRole(long IdTg, UserType role)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var user = await db.User.FirstOrDefaultAsync(x => x.IdTg == IdTg);
                if (user is null)
                {
                    User newuser = new User();
                    newuser!.UserRol = (int)role;
                    newuser!.IdTg = IdTg;
                    await db.AddAsync(newuser);
                }
                else
                {
                    user!.UserRol = (int)role;
                    db.User.Update(user);
                }
                await db.SaveChangesAsync();
            }
        }
        public static async Task AddOrUpdateUser(long IdTg, UserType role, string userTg, string Name, string jobTitle)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                //UserId = назначается tgbot ID  + 
                //UserRol: Идентификатор User Admin права  
                //TgUsername: Имя при регистрации в боте (может быть не указано) 
                //UserName: ФИО + 
                //JobTitle: Должность +

                var user = await db.User.FirstOrDefaultAsync(x => x.IdTg == IdTg);
                if (user is null)
                {
                    if (userTg is null) userTg = "Нет имени";

                    User newuser = new User { IdTg = IdTg, UserRol = (int)role, TgUserName = userTg, UserName = Name, JobTitlel = jobTitle };
                    await db.SaveChangesAsync();
                }
                else
                {
                    user.UserRol = (int)role;
                    user.TgUserName = userTg;
                    user.UserName = Name;
                    user.JobTitlel = jobTitle;
                    db.User.Update(user);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
