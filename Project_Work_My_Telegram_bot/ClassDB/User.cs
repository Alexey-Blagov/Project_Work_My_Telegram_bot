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
        //ObjectPath Объекты все пути 
        //OtherExpenses: 
        public long IdTg { get; set; } 
        public string TgUserName { get; set; } = string.Empty;
        public int UserRol { get; set; } = (int)UserType.FirstEnter;
        public string? UserName { get; set; }
        public string? JobTitlel { get; set; }
        public CarDrive? Рersonalcar { get; set; }
        public List<ObjectPath> ObjectPath { get; set; } = [];
        public List<OtherExpenses> OtherExpenses { get; set; } = [];
    }

}
