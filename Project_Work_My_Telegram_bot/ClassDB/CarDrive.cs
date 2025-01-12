using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Work_My_Telegram_bot.ClassDB
{
    public enum Fuel { dizel = 1, ai95 = 2, ai92 = 3 };
    class CarDrive
    {
        //Марка машины string
        //Марка бензина для транспорта выбор из: ТД, АИ95, АИ92 Enum
        //Средний расход бензина на 100 км.пути
        [Key]
        public int CarId { get; set; }
        public string? CarName { get; set; }
        public string? CarNumber { get; set; }
        public double GasСonsum { get; set; } = 0;
        public Fuel TypeFuel { get; set; } = Fuel.ai92;

    }
}
