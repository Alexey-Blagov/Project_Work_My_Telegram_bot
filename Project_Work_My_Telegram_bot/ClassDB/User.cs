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
        //UserId = назначается телеграм ботом ID
        //UserRol: Идентификатор User права доступа  
        //TgUsername: Имя при регистрации в боте (может быть не указано от телеграм бота) 
        //UserName: ФИО   
        //JobTitle: Должность 
        //Рersonalcar: Экземпляр персональной авто на данном User  
        //ObjectPath Объекты все пути связанные с юзером из БД  
        //OtherExpenses: Объекты все траты связанные с юзером из БД  
        public long IdTg { get; set; } 
        public string TgUserName { get; set; } = string.Empty; 
        public int UserRol { get; set; } = (int)UserType.Non;
        public string? UserName { get; set; }
        public string? JobTitlel { get; set; }
        public CarDrive? PersonalCar {  get; set; }
        public List<ObjectPath> ObjectPaths { get; set; } = new ();
        public List<OtherExpenses> OtherExpenses { get; set; } = new(); 
    }
}
