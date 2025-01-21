using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Telegram.Bot.Types;
using System.ComponentModel.DataAnnotations;

namespace Project_Work_My_Telegram_bot.ClassDB
{
    
    public class User
    {
        //UserId = назначается tgbot ID  + 
        //UserRol: Идентификатор User Admin права  
        //TgUsername: Имя при регистрации в боте (может быть не указано) 
        //UserName: ФИО + 
        //JobTitle: Должность +
        //Рersonalcar: Экземпляр персональной авто на данном User 
        //CarDrive: Личная машина  
        //ObjectPath Объекты все пути 
        //OtherExpenses: 
        public int IdTg { get; set; }
        public string TgUserName { get; set; } = string.Empty; 
        public string UserRol { get; set; } = UserType.Non.ToString();
        public string? UserName { get; set; }
        public string? JobTitlel { get; set; }
        public int CarId { get; set; }    
        public CarDrive? CarDrive { get; set; } 
        public List<ObjectPath> ObjectPath { get; set; } = [];
        public List<OtherExpenses> OtherExpenses { get; set; } = []; 

       

    }

}
