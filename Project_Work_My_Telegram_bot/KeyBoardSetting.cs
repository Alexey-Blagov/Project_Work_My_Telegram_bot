using Microsoft.AspNetCore.Http.HttpResults;
using Project_Work_My_Telegram_bot.ClassDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;


namespace Project_Work_My_Telegram_bot
{
    public static class KeyBoardSetting
    {
        // Клапвиатура Start  
        public static ReplyKeyboardMarkup startkeyboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton("Пользователь"), new KeyboardButton("Администратор") })
        {
            ResizeKeyboard = true
        };
        //Клавиатура 
        public static KeyboardButton[][] GenerateKeyboard(List<CarDrive> buttonCarsData)
        {
            List<KeyboardButton[]> keyboard = new List<KeyboardButton[]>();

            for (int i = 0; i < buttonCarsData.Count; i += 2)
            {
                if (i + 1 < buttonCarsData.Count)
                {
                    keyboard.Add(new KeyboardButton[] { new KeyboardButton(buttonCarsData[i].CarName + " " + buttonCarsData[i].CarNumber),
                                                        new KeyboardButton(buttonCarsData[i + 1].CarName + " " + buttonCarsData[i + 1].CarNumber) });
                }
                else
                {
                    keyboard.Add(new KeyboardButton[] { new KeyboardButton(buttonCarsData[i].CarName + " " + buttonCarsData[i].CarNumber) });
                }
            }
            return keyboard.ToArray();
        }
        public static ReplyKeyboardMarkup GetReplyMarkup(List<CarDrive> buttonCarDrivesData)
        {
            var keyboard = GenerateKeyboard(buttonCarDrivesData);
            return new ReplyKeyboardMarkup(keyboard) { ResizeKeyboard = true };
        }
        // Клавиатура Main 
        public static KeyboardButton[][] keyboardUser =
        [
            ["👤 Профиль", "📚 Вывести отчет"],
            ["📝 Регистрация поездки", "💰 Регистрация трат"],
            ["Смена статуа User/Admin"]
        ];
        public static KeyboardButton[][] keyboardAdmin =
        [
            ["👤 Установка пороля User", "💰 Стоимость бензина"],
            ["📝 Регистрация автопарка компании", "Смена статуа Admin/User"]
        ];
        public static KeyboardButton[][] keyboardGasType =
        [
            ["🪫 ДТ", "🔋 AИ-95", "🔋 AИ-92"]
        ];
        public static ReplyKeyboardMarkup keyboardMainUser = new(keyboard: keyboardUser)
        {
            ResizeKeyboard = true,
        };
        public static ReplyKeyboardMarkup keyboardMainAdmin = new(keyboard: keyboardAdmin)
        {
            ResizeKeyboard = true,
        };
        public static ReplyKeyboardMarkup keyboardMainGasType = new(keyboard: keyboardGasType)
        {
            ResizeKeyboard = true,
        };

        // Инлайнер клапвиатура регистрации пользователя         
        public static InlineKeyboardMarkup profile = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "👤 Ф.И.О", callbackData: "username"),
                InlineKeyboardButton.WithCallbackData(text: "⛑ Должность", callbackData: " jobtitle"),
            },
             new []
             {
                InlineKeyboardButton.WithCallbackData(text: "🚗 Марка машины", callbackData: "carname"),
                InlineKeyboardButton.WithCallbackData(text: "🇷🇺 Госномер", callbackData: "carnumber"),
             },
              new []{
                InlineKeyboardButton.WithCallbackData(text: "Используемое топливо", callbackData: "typefuel"),
                InlineKeyboardButton.WithCallbackData(text: "Средний расход л.на 100 км. ", callbackData: "gasconsum"),
              },
              new []
              {
                  InlineKeyboardButton.WithCallbackData(text: "Смена User/Admin", callbackData: "change"),
                  InlineKeyboardButton.WithCallbackData(text: "Закончить регистрацию и сохраниеть", callbackData: "closed")
              },
        });
        // Инлайнер клавиатура регистрации пользователя тип Admin регистрация 
        public static InlineKeyboardMarkup report = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Сформировать отчет за месяц", callbackData: "reportbyMonth"),
                InlineKeyboardButton.WithCallbackData(text: "👤 Должность", callbackData: "jobtitle"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Закончить регистрацию и сохранить", callbackData: "closedReport")
            },
        });
        // Инлайнер клапвиатура регистрации пути следования Admin регистрация 
        public static InlineKeyboardMarkup regPath = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "🏢 Нач. и конечн. точка назначения", callbackData: "objectname")
            },
             new []
            {
                InlineKeyboardButton.WithCallbackData(text: "🏃‍♀️ Полный путь от начала до конца в, км", callbackData: "pathlengh")
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "🚗 Собственный трансопорт?", callbackData: "acceptisCar"),
                InlineKeyboardButton.WithCallbackData(text: "📆 Дата поездки", callbackData: "datepath"),
            },
            new []
             {
                InlineKeyboardButton.WithCallbackData(text: "🕹 Закончить регистрацию и сохранить", callbackData: "closedpath")
             },
        });
        public static ReplyKeyboardMarkup actionAccept = new ReplyKeyboardMarkup(new[] { new KeyboardButton("ДА"), new KeyboardButton("НЕТ") })
        {
            ResizeKeyboard = true
        };
        public static ReplyKeyboardMarkup updateAccept = new ReplyKeyboardMarkup(new[] { new KeyboardButton("Обновить"), new KeyboardButton("Выйти") })
        {
            ResizeKeyboard = true
        };
        public static InlineKeyboardMarkup regCost = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Наименование затрат", callbackData: "namecost")
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Сумма 00.00 руб", callbackData: "sumexpenses"),
                InlineKeyboardButton.WithCallbackData(text: "📆 Дата зтраты", callbackData: "dateexpenses"),
            },
             new []
             {
                InlineKeyboardButton.WithCallbackData(text: "Закончить регистрацию и сохранить", callbackData: "ClosedExpenses")
             }
        });
        public static InlineKeyboardMarkup regDriveCar = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "🚗 Марка машины", callbackData: "carname"),
                InlineKeyboardButton.WithCallbackData(text: "🇷🇺 Госномер", callbackData: "carnumber"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text:  "Расход на 100 км.", callbackData: "gasconsum"),
                InlineKeyboardButton.WithCallbackData(text:  "Используемое топливо", callbackData: "typefuel"),
            },
             new []
             {
                InlineKeyboardButton.WithCallbackData(text: "Закончить регистрацию и сохранить", callbackData: "closedDrive")
             }
        });
        public static InlineKeyboardMarkup regCoastFuel = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "💰 Стоимость 🔋 AИ-92", callbackData: "coastAi92"),
                InlineKeyboardButton.WithCallbackData(text: "💰 Стоимость 🔋 AИ-95", callbackData: "coastAi95"),
                InlineKeyboardButton.WithCallbackData(text: "💰 Стоимость  U0001faab ДТ ", callbackData: "coastDizel"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Закончить регистрацию и сохранить", callbackData: "closedDrive")
            }
        });
    }
}
