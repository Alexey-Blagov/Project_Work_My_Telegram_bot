using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project_Work_My_Telegram_bot;
using Project_Work_My_Telegram_bot.ClassDB;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
                var user = await db.Users.FirstOrDefaultAsync(x => x.IdTg == IdTg);
                if (user is null)
                {
                    User newuser = new User();
                    newuser!.UserName = name;
                    await db.AddAsync(newuser);
                }
                else
                {
                    user!.UserName = name;
                    db.Users.Update(user);
                }
                await db.SaveChangesAsync();
            };
        }
        public static async Task SetUserAsync(User newUser)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(x => x == newUser);
                if (user is null)
                {
                    await db.AddAsync(newUser);
                }
                else
                {
                    db.Users.Update(newUser);
                }
                await db.SaveChangesAsync();
            };
        }

        public static async Task SetNewObjectPathAsync(ObjectPath newObjPath)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                try
                {
                    if (newObjPath is not null)
                    {
                        await db.AddAsync(newObjPath);
                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
        }
        public static async Task SetNewOtherExpesesAsync(OtherExpenses newOtherExpenses)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                if (newOtherExpenses is not null)
                {
                    await db.AddAsync(newOtherExpenses);
                    await db.SaveChangesAsync();
                }
            }
        }
        public static async Task<bool> SetNewPersonalCarDriveAsync(CarDrive newCarDrive)
        {
            bool isset = false;
            using (ApplicationContext db = new ApplicationContext())
            {
                CarDrive? cardrive = await db.CarDrives.FirstOrDefaultAsync(c => c.PersonalId == newCarDrive.PersonalId);

                if (cardrive is null)
                {
                    await db.AddAsync(newCarDrive);
                    await db.SaveChangesAsync();
                    isset = true;
                }
                else isset = false;
            }
            return isset;
        }
        public static async Task<bool> SetNewCommercialCarDriveAsync(CarDrive newCarDrive)
        {
            bool isset = false;
            using (ApplicationContext db = new ApplicationContext())
            {

                //Поиск в БД по номеру если такая сущ выдаем false 
                CarDrive? cardrive = await db.CarDrives.FirstOrDefaultAsync(c => c.CarNumber == newCarDrive.CarNumber);

                if (cardrive is null)
                {
                    await db.AddAsync(newCarDrive);
                    await db.SaveChangesAsync();
                    isset = true;
                }
                else isset = false;
            }
            return isset;
        }
        public static async Task UpdatePersonarCarDriveAsync(CarDrive newCarDrive)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                await db.CarDrives
                         .Where(c => c.PersonalId == newCarDrive.PersonalId)
                         .ExecuteUpdateAsync(s => s
                          .SetProperty(c => c.CarNumber, newCarDrive.CarNumber)
                          .SetProperty(c => c.CarName, newCarDrive.CarName)
                          .SetProperty(c => c.GasСonsum, newCarDrive.GasСonsum)
                          .SetProperty(c => c.TypeFuel, newCarDrive.TypeFuel));
            }
        }
        public static async Task UpdateNewCarDriveAsync(CarDrive newCarDrive)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                await db.CarDrives
                         .Where(c => c.CarNumber == newCarDrive.CarNumber)
                         .ExecuteUpdateAsync(s => s
                          .SetProperty(c => c.PersonalId, newCarDrive.PersonalId)
                          .SetProperty(c => c.isPersonalCar, newCarDrive.isPersonalCar)
                          .SetProperty(c => c.CarName, newCarDrive.CarName)
                          .SetProperty(c => c.GasСonsum, newCarDrive.GasСonsum)
                          .SetProperty(c => c.TypeFuel, newCarDrive.TypeFuel));
            }
        }
        public static async Task<int> GetUserRoleAsync(long IdTg)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(x => x.IdTg == IdTg);
                // случай если нет юзера в БД возврат Non 
                if (user == null) return 0;

                return user.UserRol;
            }
        }
        public static async Task<CarDrive> GetIsUserCar(long IdTg)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                CarDrive? userCar = await db.CarDrives.FirstOrDefaultAsync(x => x.PersonalId == IdTg);
                // случай если нет юзера в БД возврат Null 
                return userCar!;
            }
        }
        public static async Task SetUserRoleAsync(long IdTg, UserType role)
        {

            using (ApplicationContext db = new ApplicationContext())
            {
                try
                {
                    var user = await db.Users.FirstOrDefaultAsync(x => x.IdTg == IdTg);
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
                       
                    }
                    await db.SaveChangesAsync();
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
        }
        public static async Task<User> GetUserAsync(long IdTg)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                User? user = await db.Users.FirstOrDefaultAsync(x => x.IdTg == IdTg);

                return user!;
            }
        }
        public static async Task SetOrUpdateUserAsync(User newUser)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(x => x.IdTg == newUser.IdTg);

                if (user is null)
                {
                    db.Users.Add(newUser);
                    await db.SaveChangesAsync();
                }
                else
                {
                    await db.Users
                        .Where(c => c.IdTg == newUser.IdTg)
                        .ExecuteUpdateAsync(s => s
                         .SetProperty(c => c.UserName, newUser.UserName)
                         .SetProperty(c => c.TgUserName, newUser.TgUserName)
                         .SetProperty(c => c.JobTitlel, newUser.JobTitlel)
                         .SetProperty(c => c.UserRol, newUser.UserRol));
                }
            }
        }

        internal static async Task<List<CarDrive>> GetCarsDataList()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                return await db.CarDrives
                    .AsNoTracking()
                    .Where(c => c.isPersonalCar == false)
                    .ToListAsync();
            }
        }

        internal static async Task<CarDrive?> GetUserPersonalCar(long IdTg)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var userCar = await db.CarDrives.FirstOrDefaultAsync(c => c.PersonalId == IdTg && c.isPersonalCar);
                //Если Null тогда не нашли такого совпаения 
                return userCar!;
            }
        }

        internal static async Task SetNewExpensesAsync(OtherExpenses expenses)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                db.OtherExpenses.Add(expenses);
                await db.SaveChangesAsync();
            }
        }
        internal static async Task<CarDrive> GetCarDataForPath(int? carId)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                CarDrive? car = await db.CarDrives.FirstOrDefaultAsync(x => x.CarId == carId);

                return car!;
            }
        }
    }
}