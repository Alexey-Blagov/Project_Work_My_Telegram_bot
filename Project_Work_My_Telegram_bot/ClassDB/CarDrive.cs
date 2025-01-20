using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Work_My_Telegram_bot.ClassDB
{
    
    public partial class CarDrive
    {
        //Марка машины string
        //Марка бензина для транспорта выбор из: ТД, АИ95, АИ92 Enum
        //Средний расход бензина на 100 км.пути
        [Key]
        public int CarId { get; set; }
        public string? CarName { get; set; }
        public string? CarNumber { get; set; }
        public double GasСonsum { get; set; } = 0;
        public string TypeFuel { get; set; } = Fuel.ai92.ToString();

    }
}
