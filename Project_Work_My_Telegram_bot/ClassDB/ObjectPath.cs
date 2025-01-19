using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Work_My_Telegram_bot.ClassDB
{
    public partial class ObjectPath
    {
        //Дата поездки: 2 варианта(клавиатура (Сегодня поездка) или ввести в формате ДД.ММ.ГГГГ)
        //Переменная отвечающая за транспорт в пути: Личный транспорт или гаражный парк ++ 
        //UsePersonalTransport(bool) true – личный транспорт; false - гаражный парк ++ 
        //Наименование пункта назначения(string) : ++
        //Дина пути с учетом обратного пути: (int) Path Length   ++
      [Key]
      public int IdPath { get; set; }
      public bool IsPersonalCar { get; set; } = true; 
      public string? ObjectName { get; set; }
      public int PathLengh { get; set; } 
    }
}
