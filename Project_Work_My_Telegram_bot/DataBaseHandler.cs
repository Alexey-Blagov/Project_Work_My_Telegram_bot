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
        public static async Task SetUserNameAsync(long IdTg, string name)
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
        public static async Task SetNewObjectPathAsync(ObjectPath newObjPath)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                await db.AddAsync(newObjPath);
                await db.SaveChangesAsync();
            }
        }
        public static async Task SetNewOtherExpesesAsync(OtherExpenses newOtherExpenses)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                await db.AddAsync(newOtherExpenses);
                await db.SaveChangesAsync();
            }
        }
        public static async Task <bool> SetNewCarDriveAsync(CarDrive newCarDrive)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                CarDrive? cardrive = await db.CarDrive.FirstOrDefaultAsync(n => n.CarNumber == newCarDrive.CarNumber);

                if (cardrive is null)
                {
                    await db.AddAsync(newCarDrive);
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            }
        }
        public static async Task UpdateNewCarDriveAsync(CarDrive newCarDrive)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var cardrive = await db.CarDrive.FirstOrDefaultAsync(n => n.CarNumber == newCarDrive.CarNumber);
                cardrive.CarNumber = newCarDrive.CarNumber;  

                
                //await db.CarDrive.Update(cardrive);  
                await db.SaveChangesAsync();

            }

        }
        
        public static async Task SetUserJobTitleAsync(long IdTg, string jobTitle)
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
        public static async Task<int> GetUserRoleAsync(long IdTg)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var user = await db.User.FirstOrDefaultAsync(x => x.IdTg == IdTg);
                // случай если нет юзера в БД возврат Non 
                if (user == null) return 0;

                return user.UserRol;
            }
        }

        public static async Task<CarDrive> GetIsUserCar(long IdTg)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                CarDrive? usercar = await db.CarDrive.FirstOrDefaultAsync(x => x.isPersonalCar);
                // случай если нет юзера в БД возврат Null 
                return usercar!; 
            }
        }

        public static async Task SetUserRoleAsync(long IdTg, UserType role)
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
        public static async Task <User> GetUserAsync (long IdTg) 
        {

            using (ApplicationContext db = new ApplicationContext())
            { 
                User? user = await db.User.FirstOrDefaultAsync(x => x.IdTg == IdTg) ?? null;
                
                return user;
            }
        } 
        public static async Task AddOrUpdateUserAsync(User newUser)  
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var user = await db.User.FirstOrDefaultAsync(x => x.IdTg == newUser.IdTg); 
                if (user is null)
                {

                    db.User.Add(newUser); 
                    await db.SaveChangesAsync();
                }
                else
                {
                    db.User.Update(newUser);
                    await db.SaveChangesAsync();
                }
            }
        }

        
    }
}
