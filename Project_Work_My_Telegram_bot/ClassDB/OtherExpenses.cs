using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Work_My_Telegram_bot.ClassDB
{
    public class OtherExpenses
    {
        //ExpId  ++ (ID затрат) 
        //NameExpense Наименование затрат(string) ++
        //Coast: Стоимость затрат тип Decimal ++ 
        //dateTimeExp Дата трат  
        //UserId Форинкей на юзера Юсера осуществивший траты
        //UserExp: Екземплыр класса Юсера осуществивший траты
     
        public int ExpId { get; set; } 
        public string NameExpense { get; set; } = string.Empty; 
        public decimal Coast { get; set; } = 0;
        public DateTime DateTimeExp { get; set; } = DateTime.UtcNow;
        public long? UserId { get; set; }
        public User? UserExp { get; set; }
    }
}
