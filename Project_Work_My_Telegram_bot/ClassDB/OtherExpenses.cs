using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Work_My_Telegram_bot.ClassDB
{
    internal class OtherExpenses
    {
        //Наименование затрат(string) ++
        //Сумма затраты Decimal ++
        //Дата затрат:  ++
        [Key]
        public int ExpId { get; set; } 
        public string? NameExpense { get; set; }
        public decimal Coast { get; set; } = 0;
        public DateTime dateTime { get; set; } = DateTime.Now; 
    }
}
