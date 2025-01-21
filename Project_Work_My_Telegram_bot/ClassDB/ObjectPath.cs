using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Work_My_Telegram_bot.ClassDB
{
    public class ObjectPath
    {
        //IdPath Id даблицы в пути             
        //ObjectName Наименование пункта назначения(string) : ++
        //PathLeng: Дина пути с учетом обратного тип  (float) Path Length   ++
        //DatePath: Дата поездки 2 варианта(клавиатура (Сегодня поездка) или ввести в формате ДД.ММ.ГГГГ)
        //UserId: Форинкей на юзера Юсера осуществивший поездку 
        //UserExp: Екземплыр класса Юсера осуществивший траты   
        //CarId: Id на связь с CarDrive осуществивший поездку 
        //CarDrive: Екземплыр класса машины осуществившей поездуку осуществивший траты
       
        public int IdPath { get; set; }
        public string ObjectName { get; set; } = string.Empty; 
        public float PathLengh { get; set; }
        public DateTime DatePath { get; set; } = DateTime.Now;
        public int? UserId { get; set; }
        public User? UserPath { get; set; }
        public int? CarId { get; set; }
        public CarDrive? CarDrive { get; set; }
    }
}
