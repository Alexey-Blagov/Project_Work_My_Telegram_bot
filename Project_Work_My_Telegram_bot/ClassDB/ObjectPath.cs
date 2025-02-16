using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Work_My_Telegram_bot.ClassDB
{
        //IdPath Id даблицы в пути             
        //ObjectName Наименование пункта назначения(string) : ++
        //PathLeng: Дина пути с учетом обратного тип  (float) Path Length   ++
        //DatePath: Дата поездки 2 варианта(клавиатура (Сегодня поездка) или ввести в формате ДД.ММ.ГГГГ)
        //UserId: Форинкей на юзера Юсера осуществивший поездку 
        //UserPath: Екземплыр класса Юсера осуществивший поездку
        //CarId: Форинкей Id на связь с CarDrive осуществивший поездку 
        //CarDrive: Екземпляр класса машины осуществившей поездку
    public class ObjectPath
    {
        
        public int IdPath { get; set; }
        public string? ObjectName { get; set; }
        public double? PathLengh { get; set; } = null; 
        public DateTime DatePath { get; set; } = DateTime.Now;
        public int? CarId { get; set; } 
        public CarDrive? CarDrive { get; set; }
        public long? UserId { get; set; }
        public User? UserPath { get; set; }
    }
}
