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
        //        UserId(Назначаемый БД) int
        //ID_Chat: Уникальный идентификатор telegram(ChatId)  long
        //Name: ФИО(Surname, Name, SecondName)
        //Должность:  jobTitle string
        //Дата  регистрации в БД(берем из текущей даты)  Date.Now Short
        [Key]
        public int Id { get; set; }
        public long? TgChatId { get; set; }
        public string? Name { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Surname { get; set; }

        public string? JobTitle { get; set; }

        public static DateTime? CreatedAt { get; set; } = DateTime.Now;
    }

}
