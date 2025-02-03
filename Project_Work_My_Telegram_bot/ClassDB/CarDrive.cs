using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Work_My_Telegram_bot.ClassDB
{

    public class CarDrive
    {
        //CarId: 
        //CarName: Марка машины
        //isPersonalCar: true машина личная, false машина конторская или нет машины false 
        //CarNumber: Нимер машины по шаблону H 000 EE 150
        //GasСonsum: Средний расход бензина на 100 км.пути
        //TypeFuel Марка бензина для транспорта выбор из: ТД, АИ95, АИ92 из типа Enum 
        //UserCr: Екземплыр класса Юсера Null машина конторская или нет машины  конторская или нет ее 
        public int CarId { get; set; }
        public string? CarName { get; set; }
        public bool isPersonalCar { get; set; } = true;
        public string? CarNumber { get; set; }
        public double GasСonsum { get; set; } = 0.0;
        public int TypeFuel { get; set; } = (int)Fuel.ai92;
        public long? IsPersonalKey { get; set; }
        public User? UserPersonalCar { get; set; }
        public int? PathId { get; set; }
        public ObjectPath? objectPath { get; set; }
        public List<User> User { get; set; } = new();
    }
}
